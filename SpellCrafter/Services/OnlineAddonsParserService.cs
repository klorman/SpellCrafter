using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using SharpCompress.Archives;
using SpellCrafter.Models;

namespace SpellCrafter.Services
{
    public partial class OnlineAddonsParserService
    {
        private readonly HttpClient _httpClient = new();

        private const string BaseUrl = "https://www.esoui.com";
        private const string AddonsPath = "/addons.php";
        private const string DownloadsPath = "/downloads/getfile.php";

        private readonly SemaphoreSlim _semaphore = new(20);

        public async Task<List<Addon>> ParseAddonsAsync()
        {
            var addonIds = new HashSet<int>();
            var categoryUrls = new List<string>
            {
                $"{BaseUrl}{AddonsPath}",
                $"{BaseUrl}/downloads/index.php?cid=39" // Category with subcategories //TODO maybe add support for subcategories
            };

            foreach (var url in categoryUrls)
            {
                var categoryPages = await ParseCategoriesAsync(url);
                foreach (var categoryPage in categoryPages)
                {
                    var categoryPaginationPages = await ParseCategoryPaginationAsync(categoryPage); // TODO maybe parse only PageRegex

                    foreach (var categoryPaginationPage in categoryPaginationPages)
                    {
                        var ids = await ParseCategoryPageAsync(categoryPaginationPage);
                        foreach (var id in ids)
                            addonIds.Add(id);
                    }
                }
            }

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            Debug.WriteLine($"Temp folder: {tempFolder}");
            
            var addons = await ProcessAddonsAsync(addonIds, tempFolder);

            Directory.Delete(tempFolder, true);

            return addons;
        }

        private async Task<List<Addon>> ProcessAddonsAsync(IEnumerable<int> addonIds, string tempFolder)
        {
            var tasks = addonIds.Select(id => ProcessAddonAsync(id, tempFolder));
            var results = await Task.WhenAll(tasks);
            return results.Where(addon => addon != null).Select(addon => addon!).ToList();
        }

        private async Task<Addon?> ProcessAddonAsync(int id, string tempFolder)
        {
            await _semaphore.WaitAsync();
            var archivePath = await DownloadAddonArchive(id, tempFolder);
            if (string.IsNullOrEmpty(archivePath)) return null;

            try
            {
                var tempManifestPath = ExtractAddonManifest(archivePath);
                if (string.IsNullOrEmpty(tempManifestPath)) return null;

                Addon? addon;
                try
                {
                    addon = AddonManifestParser.ParseAddonManifest(tempManifestPath);
                    addon.UniqueId = id;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex}");
                    return null;
                }
                finally
                {
                    if (!string.IsNullOrEmpty(tempManifestPath) && File.Exists(tempManifestPath))
                        File.Delete(tempManifestPath);
                }

                return addon;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
                return null;
            }
            finally
            {
                if (!string.IsNullOrEmpty(archivePath) && File.Exists(archivePath))
                    File.Delete(archivePath);
                _semaphore.Release();
            }
        }

        private async Task<List<string>> ParseCategoriesAsync(string url)
        {
            var html = await _httpClient.GetStringAsync(url);
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var categoryLink = document.QuerySelectorAll("div.subtitle > a")
                .Select(node => node.GetAttribute("href"))
                .Where(href => !string.IsNullOrEmpty(href))
                .Select(href => href!)
                .ToList();

            return categoryLink;
        }

        [GeneratedRegex(@"Page \d+ of (\d+)")]
        private static partial Regex PageRegex();

        [GeneratedRegex(@"/downloads/cat(\d+).html")]
        private static partial Regex CategoryRegex();

        private async Task<List<string>> ParseCategoryPaginationAsync(string url)
        {
            var categoryPages = new List<string> { url };

            var html = await _httpClient.GetStringAsync(url);
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var pageControl = document.QuerySelector("td.vbmenu_control");
            if (pageControl == null) return categoryPages;

            var text = pageControl.TextContent;
            var matchPage = PageRegex().Match(text);
            if (!matchPage.Success) return categoryPages;

            var pageCount = int.Parse(matchPage.Groups[1].Value);
            var matchCategory = CategoryRegex().Match(url);
            if (!matchCategory.Success) return categoryPages;

            var categoryId = matchCategory.Groups[1].Value;
            for (var i = 2; i <= pageCount; ++i)
            {
                var pageUrl =
                    $"{BaseUrl}/downloads/index.php?cid={categoryId}&sb=dec_date&so=desc&pt=f&page={i}";
                categoryPages.Add(pageUrl);
            }

            return categoryPages;
        }

        [GeneratedRegex(@"info(\d+)-")]
        private static partial Regex InfoIdRegex();

        private async Task<List<int>> ParseCategoryPageAsync(string url)
        {
            var html = await _httpClient.GetStringAsync(url);
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var files = document.QuerySelectorAll("div.file > div.title > a");
            var regex = InfoIdRegex();

            return (from file in files
                select file.GetAttribute("href") into href
                select regex.Match(href) into match
                where match.Success
                select int.Parse(match.Groups[1].Value)).ToList();
        }

        private async Task<string?> DownloadAddonArchive(int addonId, string tempFolder)
        {
            var url = $"{BaseUrl}{DownloadsPath}?id={addonId}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Failed to download the file for addon ID {addonId}");
                return null;
            }

            var contentDisposition = response.Content.Headers.ContentDisposition?.FileName?.Trim('\"');
            if (string.IsNullOrEmpty(contentDisposition))
            {
                Debug.WriteLine($"Could not get an addon with an ID {addonId}. This addon has probably been deleted");
                return null;
            }

            var supportedExtensions = new HashSet<string> { ".zip", ".rar", ".7z", ".gz", ".tar" };
            var fileExtension = Path.GetExtension(contentDisposition).ToLower();
            if (!supportedExtensions.Contains(fileExtension))
            {
                Debug.WriteLine($"Unsupported archive type or not an archive: {contentDisposition}");
                return null;
            }

            var archivePath = Path.Combine(tempFolder, contentDisposition);
            await using var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(fs);

            return archivePath;
        }

        private string? ExtractAddonManifest(string archivePath)
        {
            try
            {
                using var archive = ArchiveFactory.Open(archivePath);

                var manifestName =
                    archive.Entries
                        .Where(e => !e.IsDirectory && e.Key != null && e.Key.Contains('/'))
                        .Select(e => e.Key?[..e.Key.IndexOf('/')])
                        .FirstOrDefault();
                if (string.IsNullOrEmpty(manifestName)) return null;

                var manifestFilePath = $"{manifestName}/{manifestName}.txt";

                var manifestEntry = archive.Entries.FirstOrDefault(e => e.Key != null &&
                        e.Key.Equals(manifestFilePath, StringComparison.OrdinalIgnoreCase));
                if (manifestEntry?.Key == null) return null;

                var tempManifestPath = Path.Combine(Path.GetDirectoryName(archivePath)!, $"{manifestName}.txt");

                using var reader = manifestEntry.OpenEntryStream();
                using var fileStream = File.OpenWrite(tempManifestPath);
                Debug.WriteLine($"Extracting {manifestName} manifest from {Path.GetFileName(archivePath)}");

                reader.CopyTo(fileStream);

                return tempManifestPath;
            }
            catch (InvalidDataException ex)
            {
                Debug.WriteLine($"Archive {archivePath} is corrupted! {ex}");
                return null;
            }
        }
    }
}
