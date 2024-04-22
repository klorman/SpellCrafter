using ReactiveUI;
using SpellCrafter.Enums;
using SpellCrafter.ViewModels;
using System.Diagnostics;

namespace SpellCrafter.Views
{
    public partial class MainWindow : ReactiveMetroWindow<MainWindowViewModel>
    {
        public MainWindow() : base()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            DataContext = vm;
        }
    }
}