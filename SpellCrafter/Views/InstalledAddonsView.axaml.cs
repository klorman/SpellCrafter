using Avalonia.ReactiveUI;
using ReactiveUI;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class InstalledAddonsView : ReactiveUserControl<InstalledAddonsViewModel>
    {
        public InstalledAddonsView()
        {
            InitializeComponent();

            var vm = new InstalledAddonsViewModel();
            DataContext = vm;

            this.WhenActivated(_ => { });
        }
    }
}
