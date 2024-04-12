using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;
using SpellCrafter.Models.DbClasses;

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
