using Avalonia.Controls;
using SpellCrafter.Models.DbClasses;
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

        private void DataGridMods_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MyModsViewModel viewModel)
            {
                if (e.AddedItems != null)
                {
                    foreach (var item in e.AddedItems)
                    {
                        if (item is Addon addon)
                            viewModel.DataGridModsSelectedItems.Add(addon);
                    }
                }

                if (e.RemovedItems != null)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (item is Addon addon)
                            viewModel.DataGridModsSelectedItems.Remove(addon);
                    }
                }
            }
        }
    }
}
