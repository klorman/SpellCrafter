using System;
using System.Windows.Input;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpellCrafter.Enums;
using SpellCrafter.Messages;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] public MainTabControlTabs MainTabControlSelectedIndex { get; set; } = 0;
        [Reactive] public Addon? SelectedAddon { get; set; }

        public ICommand OpenSettingsDialogCommand { get; }

        public MainWindowViewModel() : base()
        {
            MessageBus.Current.Listen<AddonUpdatedMessage>()
                .Subscribe(message =>
                {
                    SelectedAddon = message.UpdatedAddon;
                    MainTabControlSelectedIndex = MainTabControlTabs.ModPage;
                });

            OpenSettingsDialogCommand = new RelayCommand(async param =>
            {
                if (param is bool isOpened && isOpened)
                    await ShowMainDialogAsync(new SettingsDialogViewModel());
                else
                    CloseMainDialog();
            });
        }
    }
}
