using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace SpellCrafter.Controls
{
    public class MenuButton : RadioButton
    {
        public static readonly StyledProperty<StreamGeometry> IconProperty =
            AvaloniaProperty.Register<MenuButton, StreamGeometry>(nameof(Icon));

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MenuButton, string>(nameof(Text));

        public static readonly StyledProperty<bool> IsTextVisibleProperty =
            AvaloniaProperty.Register<MenuButton, bool>(nameof(IsTextVisible), defaultValue: true);

        public StreamGeometry Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public bool IsTextVisible
        {
            get => GetValue(IsTextVisibleProperty);
            set => SetValue(IsTextVisibleProperty, value);
        }
    }
}
