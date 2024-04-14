using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SpellCrafter.Views.MainWindowTabs
{
    public partial class AddonDetails : UserControl
    {
        public AddonDetails()
        {
            InitializeComponent();
        }

        private void OpenContextMenu_Click(object? sender, RoutedEventArgs e)
        {
            var button = sender as Control;
            button?.ContextMenu?.Open(button);
        }
    }
}
