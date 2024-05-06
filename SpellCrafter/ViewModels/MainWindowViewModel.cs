using System;
using System.Windows.Input;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using SpellCrafter.Models;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] public MainTabControlTabs MainTabControlSelectedIndex { get; set; } = 0;
        [Reactive] public Addon? SelectedAddon { get; set; }

        public RelayCommand OpenSettingsDialogCommand { get; }

        public MainWindowViewModel() : base()
        {
            MessageBus.Current.Listen<AddonUpdatedMessage>()
                .Subscribe(message =>
                {
                    SelectedAddon = message.UpdatedAddon;
                    MainTabControlSelectedIndex = MainTabControlTabs.ModPage;
                });

            OpenSettingsDialogCommand = new RelayCommand(async _ =>
            {
                await ShowMainDialogAsync(new SettingsDialogViewModel());
            });
        }
    }
}
