using System;
using System.Reactive.Disposables;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Messages;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();
        [Reactive] public RoutingState Router { get; set; } = new();
        [Reactive] public bool IsMyModsButtonChecked { get; set; }
        [Reactive] public bool IsBrowseButtonChecked { get; set; }
        [Reactive] public bool IsSettingsButtonChecked { get; set; }

        public ICommand ShowInstalledAddonsCommand { get; }
        public ICommand ShowBrowseCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainWindowViewModel()
        {
            var isAddonsDirectoryValid =
                SettingsViewModel.CheckIsAddonDirectoryValid(AppSettings.Instance.AddonsDirectory);

            if (!isAddonsDirectoryValid)
                AppSettings.Instance.AddonsDirectory = string.Empty;

            this.WhenActivated((CompositeDisposable _) =>
            {
                if (isAddonsDirectoryValid)
                {
                    Router.Navigate.Execute(new InstalledAddonsViewModel());
                    IsMyModsButtonChecked = true;
                }
                else
                {
                    Router.Navigate.Execute(new SettingsViewModel());
                    IsSettingsButtonChecked = true;
                }
            });

            MessageBus.Current.Listen<ViewAddonMessage>()
                .Subscribe(message =>
                {
                    var addon = message.Addon;
                    if (addon != null)
                    {
                        var addonDetailsViewModel = new AddonDetailsViewModel();
                        addonDetailsViewModel.CopyFromAddon(addon);
                        Router.Navigate.Execute(addonDetailsViewModel);
                    }
                });

            ShowInstalledAddonsCommand = new RelayCommand
            ( 
                _ => { Router.Navigate.Execute(new InstalledAddonsViewModel()); }, 
                _ => !(Router.GetCurrentViewModel() is InstalledAddonsViewModel)
            );

            ShowBrowseCommand = new RelayCommand
            (
                _ => { Router.Navigate.Execute(new BrowseViewModel()); },
                _ => !(Router.GetCurrentViewModel() is BrowseViewModel)
            );

            ShowSettingsCommand = new RelayCommand
            (
                _ => { Router.Navigate.Execute(new SettingsViewModel()); },
                _ => !(Router.GetCurrentViewModel() is SettingsViewModel)
            );
        }

    }
}
