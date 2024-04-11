using Avalonia.Controls;
using SpellCrafter.ViewModels.MainWindowTabs;

namespace SpellCrafter.Views.MainWindowTabs
{
    public partial class Browse : UserControl
    {
        public Browse()
        {
            InitializeComponent();

            var vm = new BrowseViewModel();
            DataContext = vm;
        }
    }
}
