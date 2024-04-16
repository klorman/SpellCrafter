using DialogHostAvalonia;
using ReactiveUI;
using System.Threading.Tasks;

namespace SpellCrafter.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private const string dialogIdentifier = "MainDialogHost";

        public async Task ShowMainDialogAsync(ViewModelBase dialogViewModel)
        {
            if (DialogHost.IsDialogOpen(dialogIdentifier))
                DialogHost.Close(dialogIdentifier);

            await DialogHost.Show(dialogViewModel, dialogIdentifier);
        }
        
        public void CloseMainDialog() =>
            DialogHost.Close(dialogIdentifier);
    }
}
