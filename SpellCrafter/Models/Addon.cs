using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using SpellCrafter.Services;
using System.IO;
using SharpCompress.Archives;
using SharpCompress.Common;
using SpellCrafter.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SpellCrafter.Models
{
    public class Addon : ReactiveObject, ILocalAddon, IOnlineAddon, ICommonAddon
    {
        private const string BaseAddonPageLink = "https://www.esoui.com/downloads/info";

        public int CommonAddonId { get; set; } = -1;
        public int? LocalAddonId { get; set; }
        public int? OnlineAddonId { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Title { get; set; } = string.Empty;
        [Reactive] public string Description { get; set; } = string.Empty;
        [Reactive] public AddonState State { get; set; } = AddonState.NotInstalled;
        [Reactive] public AddonInstallationMethod InstallationMethod { get; set; } = AddonInstallationMethod.Other;
        [Reactive] public string Downloads { get; set; } = "TODO downloads";
        [Reactive] public ObservableCollection<Author> Authors { get; set; } = [];
        IList<Author> ICommonAddon.Authors => Authors;
        [Reactive] public ObservableCollection<Category> Categories { get; set; } = [];
        IList<Category> ICommonAddon.Categories => Categories;
        [Reactive] public RangedObservableCollection<CommonAddon> LocalDependencies { get; set; } = [];
        [Reactive] public RangedObservableCollection<CommonAddon> OnlineDependencies { get; set; } = [];
        [Reactive] public int? UniqueId { get; set; }
        [Reactive] public string FileSize { get; set; } = "TODO archive size";
        [Reactive] public string Overview { get; set; } = "TODO";
        [Reactive] public string Version { get; set; } = string.Empty;
        [Reactive] public string DisplayedVersion { get; set; } = string.Empty;
        [Reactive] public string LatestVersion { get; set; } = string.Empty;
        [Reactive] public string DisplayedLatestVersion { get; set; } = string.Empty;

        public ICommand ViewModCommand { get; }
        public ICommand InstallCommand { get; }
        public ICommand ReinstallCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ViewWebsiteCommand { get; }
        public ICommand CopyLinkCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public Addon()
        {
            ViewModCommand = new RelayCommand(_ => ViewMod());
            InstallCommand = new RelayCommand(
                _ => Install(),
                _ => State == AddonState.NotInstalled
            );
            ReinstallCommand = new RelayCommand(
                _ => Reinstall(),
                _ => State != AddonState.NotInstalled
            );
            UpdateCommand = new RelayCommand(
                _ => Update(),
                _ => State is AddonState.Outdated or AddonState.InstallationError
            );
            DeleteCommand = new RelayCommand(
                _ => Delete(),
                _ => State != AddonState.NotInstalled
            );
            ViewWebsiteCommand = new RelayCommand(_ => ViewWebsite());
            CopyLinkCommand = new RelayCommand(_ => CopyLink());
            BrowseFolderCommand = new RelayCommand(
                _ => BrowseFolder(),
                _ => State != AddonState.NotInstalled
            );
        }

        private void ViewMod()
        {
            Debug.WriteLine($"Opening addon {Name} page");

            MessageBus.Current.SendMessage(new ViewAddonMessage(this));
        }

        private async Task Install(AddonInstallationMethod installationMethod = AddonInstallationMethod.SpellCrafter, bool recursive = true)
        {
            Debug.WriteLine($"Installing addon {Name}");

            if (UniqueId == null)
            {
                Debug.WriteLine($"Error, addon unique id not specified!");
                return;
            }

            var addonPath = Path.Combine(AppSettings.Instance.AddonsDirectory, Name);
            if (string.IsNullOrEmpty(addonPath))
                return;

            using var db = new EsoDataConnection();

            var oldState = State;
            State = AddonState.Installing;
            AddonDataManager.InsertOrUpdateLocalAddon(db, this);

            var tempFolder = CreateTempFolder();
            Debug.WriteLine($"Temp folder: {tempFolder}");

            try
            {
                var parser = new OnlineAddonsParserService();
                var archivePath = await parser.DownloadAddonArchive(UniqueId.Value, tempFolder);
                if (string.IsNullOrEmpty(archivePath)) return;

                ExtractArchive(archivePath, AppSettings.Instance.AddonsDirectory);

                State = AddonState.LatestVersion;
                InstallationMethod = installationMethod;
                Version = LatestVersion;
                DisplayedVersion = DisplayedLatestVersion;
                LocalDependencies.Refresh(OnlineDependencies);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                State = oldState;
                return;
            }
            finally
            {
                DeleteDirectory(tempFolder);
                AddonDataManager.InsertOrUpdateLocalAddon(db, this);
            }

            Debug.WriteLine($"Addon {Name} installed!");

            if (!recursive) return;

            Debug.WriteLine($"Installing {Name} dependencies: {string.Join(", ", LocalDependencies.Select(addon => addon.Name))}");

            foreach (var dependency in LocalDependencies)
            {
                var addon = AddonDataManager.OnlineAddons.FirstOrDefault(a => a.CommonAddonId == dependency.Id);

                if (addon == null)
                {
                    Debug.WriteLine($"Dependency {dependency.Name} not found!");
                    continue;
                }

                switch (addon.State)
                {
                    case AddonState.LatestVersion:
                        continue;
                    case AddonState.Installing:
                        continue;
                    case AddonState.NotInstalled:
                        await addon.Install(AddonInstallationMethod.Dependency);
                        continue;
                    case AddonState.Outdated:
                        await addon.Update();
                        continue;
                    case AddonState.InstallationError:
                        await addon.Reinstall();
                        continue;
                    default:
                        continue;
                }
            }
        }

        private async Task Reinstall()
        {
            Debug.WriteLine($"Reinstalling addon {Name}");

            Delete();
            await Install(InstallationMethod);
        }

        public async Task Update(bool recursive = true)
        {
            Debug.WriteLine($"Updating addon {Name}");

            if (AddonVersionComparer.CompareVersions(Version, LatestVersion) >= 0) return;

            var addonPath = Path.Combine(AppSettings.Instance.AddonsDirectory, Name);
            if (!Directory.Exists(addonPath))
                return;

            var tempFolder = CreateTempFolder();

            try
            {
                foreach (var directory in Directory.GetDirectories(addonPath, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(directory.Replace(addonPath, tempFolder));

                foreach (var file in Directory.GetFiles(addonPath, "*", SearchOption.AllDirectories))
                    File.Move(file, file.Replace(addonPath, tempFolder));

                DeleteDirectory(addonPath);
                await Install(InstallationMethod, recursive);
                Debug.WriteLine("Installation successful");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Installation failed: {ex.Message}. Restoring from backup");

                DeleteDirectory(addonPath);

                foreach (var directory in Directory.GetDirectories(tempFolder, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(directory.Replace(tempFolder, addonPath));

                foreach (var file in Directory.GetFiles(tempFolder, "*", SearchOption.AllDirectories))
                    File.Move(file, file.Replace(tempFolder, addonPath));
            }
            finally
            {
                DeleteDirectory(tempFolder);
            }
        }

        private void Delete(string? addonPath = null)
        {
            Debug.WriteLine($"Deleting addon {Name}");
            
            addonPath ??= Path.Combine(AppSettings.Instance.AddonsDirectory, Name);
            DeleteDirectory(addonPath);

            using var db = new EsoDataConnection();

            AddonDataManager.RemoveLocalAddon(db, this);
            LocalAddonId = null;
            State = AddonState.NotInstalled;
        }

        private void ViewWebsite()
        {
            Debug.WriteLine($"Opening addon {Name} website");

            if (UniqueId == null)
                return;

            var url = $"{BaseAddonPageLink}{UniqueId}";
            OpenUrl(url);
        }

        private async void CopyLink()
        {
            Debug.WriteLine($"Copying addon {Name} link");

            if (UniqueId == null)
                return;

            var url = $"{BaseAddonPageLink}{UniqueId}";

            var clipboard = ClipboardService.Get();
            if (clipboard != null)
                await clipboard.SetTextAsync(url);
        }

        private void BrowseFolder()
        {
            Debug.WriteLine("BrowseFolder!");

            var addonPath = Path.Combine(AppSettings.Instance.AddonsDirectory, Name);
            if (!Directory.Exists(addonPath))
                return;

            OpenUrl(addonPath);
        }

        private void ExtractArchive(string archivePath, string targetDirectory)
        {
            Directory.CreateDirectory(targetDirectory);

            using var archive = ArchiveFactory.Open(archivePath);
            foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
            {
                entry.WriteToDirectory(targetDirectory, new ExtractionOptions
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
        }

        private string CreateTempFolder()
        {
            var tempFolder = Path.Combine(Path.GetTempPath(), "SpellCrafter");
            DeleteDirectory(tempFolder);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }

        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        private void OpenUrl(string url)
        {
            try
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public LocalAddon ToLocalAddon() => new()
        {
            Id = LocalAddonId,
            CommonAddonId = CommonAddonId,
            Version = Version,
            DisplayedVersion = DisplayedVersion,
            State = State,
            InstallationMethod = InstallationMethod
        };

        public CommonAddon ToCommonAddon() => new()
        {
            Id = CommonAddonId,
            Name = Name,
            Title = Title,
            Description = Description,
            Authors = [..Authors],
            Categories = [..Categories]
        };
    }
}
