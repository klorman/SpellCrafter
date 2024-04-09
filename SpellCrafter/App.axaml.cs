using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Styles.Themes;
using SpellCrafter.Views;

namespace SpellCrafter
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();

            var lightPrimaryColor = Color.Parse("#FF204E");
            var midPrimaryColor = Color.Parse("#A0153E");
            var darkPrimaryColor = Color.Parse("#5D0E41");
            var accentColor = Color.Parse("#00224D");

            var theme = Theme.Create(Theme.Dark, midPrimaryColor, accentColor);
            theme.PrimaryLight = lightPrimaryColor;
            theme.PrimaryDark = darkPrimaryColor;

            var themeBootstrap = this.LocateMaterialTheme<MaterialThemeBase>();
            themeBootstrap.CurrentTheme = theme;
        }
    }
}