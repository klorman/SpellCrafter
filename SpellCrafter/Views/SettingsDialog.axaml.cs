using Avalonia.Controls;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class SettingsDialog : UserControl
    {
        public SettingsDialog()
        {
            InitializeComponent();

            var vm = new SettingsDialogViewModel();
            DataContext = vm;
        }
    }
}
