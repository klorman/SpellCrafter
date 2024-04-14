using System;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : WindowViewModelBase
    {
        [Reactive] public MainTabControlTabs MainTabControlSelectedIndex { get; set; } = 0;
        [Reactive] public Addon? SelectedAddon { get; set; }

        public MainWindowViewModel() : base()
        {
            MessageBus.Current.Listen<AddonUpdatedMessage>()
                .Subscribe(message =>
                {
                    SelectedAddon = message.UpdatedAddon;
                    MainTabControlSelectedIndex = MainTabControlTabs.ModPage;
                });
        }
    }
}
