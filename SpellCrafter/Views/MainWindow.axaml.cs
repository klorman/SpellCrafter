using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class MainWindow : SPPOWindowBase<MainWindowViewModel>
    {
        public MainWindow() : base()
        {
            InitializeComponent();

            var vm = new MainWindowViewModel();
            DataContext = vm;
        }
    }
}