using SpellCrafter.ViewModels;
using Avalonia.ReactiveUI;

namespace SpellCrafter.Views
{
    public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
    {
        public SettingsView()
        {
            InitializeComponent();

            var vm = new SettingsViewModel();
            DataContext = vm;
        }
    }
}
