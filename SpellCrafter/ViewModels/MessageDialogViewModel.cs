using ReactiveUI.Fody.Helpers;

namespace SpellCrafter.ViewModels
{
    public class MessageDialogViewModel : ViewModelBase
    {
        [Reactive] public string Message { get; set; } = "";

        public MessageDialogViewModel() : base()
        {

        }
    }
}
