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

            var addonsFolderPath = IniParser.GetParam(IniDefines.AddonsFolderPath);

            var vm = new MainWindowViewModel();
            DataContext = vm;

            this.WhenActivated(async _ =>
            {
                if (string.IsNullOrEmpty(addonsFolderPath))
                {
                    Debug.WriteLine("AddonsFolderPath is empty!");
                    await vm.ShowMainDialogAsync(new AddonFolderSelectionDialogViewModel());
                }
            });
        }
    }
}