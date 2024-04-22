using DialogHostAvalonia;
using ReactiveUI;
using System.Threading.Tasks;

namespace SpellCrafter.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected const string dialogIdentifier = "MainDialogHost";

        public async Task ShowMainDialogAsync(ViewModelBase dialogViewModel)
        {
            if (DialogHost.IsDialogOpen(dialogIdentifier))
            {
                var currentContent = DialogHost.GetDialogSession(dialogIdentifier)?.Content as ViewModelBase;

                DialogHost.Close(dialogIdentifier);

                if (currentContent == null || currentContent.GetType() != dialogViewModel.GetType())
                    await DialogHost.Show(dialogViewModel, dialogIdentifier);
            }
            else
                await DialogHost.Show(dialogViewModel, dialogIdentifier);
        }
        
        public void CloseMainDialog() =>
            DialogHost.Close(dialogIdentifier);
    }
}
