using ReactiveUI;
using System.Threading.Tasks;
using SpellCrafter.ViewModels;
using System;
using System.Reactive.Disposables;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace SpellCrafter.Views
{
    public class SPPOWindowBase<TViewModel> : ReactiveMetroWindow<TViewModel>
        where TViewModel : ViewModelBase
    {
        public SPPOWindowBase()
        {
            PropertyChanged += (sender, e) =>
            {
                if (e.Property == ViewModelProperty)
                {
                    var oldValue = e.OldValue as ViewModelBase;
                    var newValue = e.NewValue as ViewModelBase;

                    if (oldValue != null)
                    {
                        oldValue.OnRequestClose -= Close;
                    }

                    if (newValue != null)
                    {
                        newValue.OnRequestClose += Close;

                        OnViewModelChanged(newValue);
                    }
                }
            };
        }

        private void OnViewModelChanged(ViewModelBase viewModel)
        {
            this.WhenActivated(
                disposables =>
                {
                    this.BindInteraction(
                        viewModel,
                        vm => vm.OpenFilePickerAsync,
                        DoOpenFilePickerAsync)
                    .DisposeWith(disposables);

                    this.BindInteraction(
                        viewModel,
                        vm => vm.SaveFilePickerAsync,
                        DoSaveFilePickerAsync)
                    .DisposeWith(disposables);

                    this.BindInteraction(
                        viewModel,
                        vm => vm.OpenFolderPickerAsync,
                        DoOpenFolderPickerAsync)
                    .DisposeWith(disposables);
                });
        }

        public async Task DoOpenFilePickerAsync(IInteractionContext<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?> interaction)
        {
            var options = interaction.Input;
            options.SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop);
            var files = await StorageProvider.OpenFilePickerAsync(options);

            interaction.SetOutput(files);
        }

        public async Task DoSaveFilePickerAsync(IInteractionContext<FilePickerSaveOptions, IStorageFile?> interaction)
        {
            var options = interaction.Input;
            options.SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop);
            var file = await StorageProvider.SaveFilePickerAsync(options);

            interaction.SetOutput(file);
        }

        public async Task DoOpenFolderPickerAsync(IInteractionContext<FolderPickerOpenOptions, string> interaction)
        {
            var options = interaction.Input;
            options.SuggestedStartLocation = await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop);
            var folders = await StorageProvider.OpenFolderPickerAsync(options);

            var folderPath = (folders.Count >= 1) ? folders[0].Path.LocalPath : "";
            interaction.SetOutput(folderPath);
        }

        public async Task<TDialogViewModel?> DoShowDialog<TDialogViewModel>(TDialogViewModel vm)
            where TDialogViewModel : ViewModelBase
        {
            var windowType = GetWindowType<TDialogViewModel>();
            if (windowType == null)
            {
                Debug.WriteLine("Не удалось получить тип окна по его ViewModel");
                return null;
            }

            var method = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m =>
                    m.Name == nameof(DoShowDialog) &&
                    m.IsGenericMethod &&
                    m.GetGenericArguments().Length == 2 &&
                    m.GetParameters().Length == 1 )
                .FirstOrDefault();

            if (method != null)
            {
                method = method.MakeGenericMethod(typeof(TDialogViewModel), windowType);
                var result = method.Invoke(this, new object[] { vm });
                if (result is Task<TDialogViewModel> taskResult)
                    return await taskResult;
            }

            return null;
        }

        public async Task<TDialogViewModel> DoShowDialog<TDialogViewModel, TDialogWindow>(TDialogViewModel vm)
            where TDialogViewModel : ViewModelBase
            where TDialogWindow : SPPOWindowBase<TDialogViewModel>, new()
        {
            var dialog = new TDialogWindow
            {
                DataContext = vm,
                ViewModel = vm
            };

            await dialog.ShowDialog(this);
            return dialog.ViewModel;
        }

        private Type? GetWindowType<TDialogViewModel>() where TDialogViewModel : ViewModelBase
        {
            var windowBaseType = typeof(SPPOWindowBase<>);
            var genericType = windowBaseType.MakeGenericType(typeof(TDialogViewModel));

            var windowTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => genericType.IsAssignableFrom(p) && !p.IsAbstract);

            return windowTypes.FirstOrDefault();
        }

        protected void BindShowDialogEvent<T>() where T : ViewModelBase
        {
            if (ViewModel == null)
                return;

            ViewModel.DialogFactory.SetEvent<T>(DoShowDialog<T>);
        }
    }
}
