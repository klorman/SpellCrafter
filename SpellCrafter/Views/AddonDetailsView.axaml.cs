using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class AddonDetailsView : ReactiveUserControl<AddonDetailsViewModel>
    {
        public AddonDetailsView()
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
