using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace SpellCrafter.Services
{
    public static class StorageProviderService
    {
        private static TopLevel? _topLevel;

        public static void Initialize(TopLevel topLevel)
        {
            _topLevel = topLevel;
        }

        public static async Task<string> OpenFolderPickerAsync(FolderPickerOpenOptions options)
        {
            if (_topLevel == null) return "";

            options.SuggestedStartLocation = await _topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            var folders = await _topLevel.StorageProvider.OpenFolderPickerAsync(options);

            return (folders.Count >= 1) ? folders[0].Path.LocalPath : "";
        }
    }
}
