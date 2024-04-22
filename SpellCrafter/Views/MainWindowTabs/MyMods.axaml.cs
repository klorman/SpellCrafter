using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SpellCrafter.ViewModels.MainWindowTabs;
using System.Diagnostics;

namespace SpellCrafter.Views.MainWindowTabs
{
    public partial class MyMods : ReactiveUserControl<MyModsViewModel>
    {
        public MyMods()
        {
            InitializeComponent();

            var vm = new MyModsViewModel();
            DataContext = vm;

            this.WhenActivated(_ => { });
        }
    }
}
