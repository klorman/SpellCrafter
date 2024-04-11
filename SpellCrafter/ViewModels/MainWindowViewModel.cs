using ReactiveUI.Fody.Helpers;
using SpellCrafter.Models.DbClasses;

namespace SpellCrafter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive] public Addon? SelectedAddon { get; set; }

        public MainWindowViewModel() : base() { }
    }
}
