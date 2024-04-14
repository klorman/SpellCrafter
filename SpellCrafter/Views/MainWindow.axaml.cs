using Avalonia.Input;
using Avalonia.Interactivity;
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
        public void ShowSettingsDialog_Click(object sender, RoutedEventArgs e)
        {
            var isVisible = settingsDialog.IsVisible;
            settingsDialog.IsVisible = !isVisible;
            blurBackground.IsVisible = !isVisible;
        }

        public void CloseSettingsDialog_Click(object sender, RoutedEventArgs e)
        {
            settingsDialog.IsVisible = false;
            blurBackground.IsVisible = false;
        }

        public void ShowPaletteSwatchDialog_Click(object sender, RoutedEventArgs e)
        {
            var isVisible = paletteSwatchDialog.IsVisible;
            paletteSwatchDialog.IsVisible = !isVisible;
            blurBackground.IsVisible = !isVisible;
        }

        public void ClosePaletteSwatchDialog_Click(object sender, RoutedEventArgs e)
        {
            paletteSwatchDialog.IsVisible = false;
            blurBackground.IsVisible = false;
        }

        private void BlurBackground_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            settingsDialog.IsVisible = false;
            paletteSwatchDialog.IsVisible = false;
            blurBackground.IsVisible = false;
        }
    }
}