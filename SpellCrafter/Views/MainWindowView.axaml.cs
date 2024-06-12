using ReactiveUI;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class MainWindowView : ReactiveMetroWindow<MainWindowViewModel>
    {
        public MainWindowView()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            DataContext = vm;

            this.WhenActivated(_ => { });
        }
    }
}