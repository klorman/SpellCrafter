using Avalonia.ReactiveUI;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Views
{
    public partial class BrowseView : ReactiveUserControl<BrowseViewModel>
    {
        public BrowseView()
        {
            InitializeComponent();
        }
    }
}
