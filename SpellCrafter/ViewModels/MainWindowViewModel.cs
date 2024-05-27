using System;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using SpellCrafter.Models;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        [Reactive] public RoutingState Router { get; set; } = new();

        public ICommand ShowInstalledAddonsCommand { get; }
        public ICommand ShowBrowseCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainWindowViewModel() : base()
        {
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
