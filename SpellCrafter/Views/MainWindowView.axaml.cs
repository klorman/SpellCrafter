using ReactiveUI;
using SpellCrafter.Enums;
using SpellCrafter.ViewModels;
using System.Diagnostics;

namespace SpellCrafter.Views
{
    public partial class MainWindowView : ReactiveMetroWindow<MainWindowViewModel>
    {
        public MainWindowView() : base()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            DataContext = vm;
        }
    }
}