using Avalonia.Controls;
using SpellCrafter.ViewModels.MainWindowTabs;

namespace SpellCrafter.Views.MainWindowTabs
{
    public partial class MyMods : UserControl
    {
        public MyMods()
        {
            InitializeComponent();

            var vm = new MyModsViewModel();
            DataContext = vm;
        }
    }
}
