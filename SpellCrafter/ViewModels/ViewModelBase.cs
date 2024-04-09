using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace SpellCrafter.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public Interaction<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?> OpenFilePickerAsync { get; } = new();
        public Interaction<FilePickerSaveOptions, IStorageFile?> SaveFilePickerAsync { get; } = new();
        public Interaction<FolderPickerOpenOptions, string> OpenFolderPickerAsync { get; } = new();
       
        public event Action? OnRequestClose;

        public readonly ShowDialogEventFactory DialogFactory = new();

        public async Task<IReadOnlyList<IStorageFile>?> HandleOpenFilePickerAsync(FilePickerOpenOptions options) =>
            await OpenFilePickerAsync.Handle(options);

        public async Task<IStorageFile?> HandleSaveFilePickerAsync(FilePickerSaveOptions options) =>
            await SaveFilePickerAsync.Handle(options);

        public async Task<string> HandleOpenFolderPickerAsync(FolderPickerOpenOptions options) =>
            await OpenFolderPickerAsync.Handle(options);

        public void CloseWindow()
        {
            OnRequestClose?.Invoke();
        }

        public async Task<TViewModel?> ShowDialog<TViewModel>(TViewModel vm)
            where TViewModel : ViewModelBase
        {
            return await DialogFactory.RaiseEvent(vm);
        }
    }
}
