using Avalonia;
using Avalonia.Data;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace SpellCrafter.Controls
{
    public class ColoredSvg : Avalonia.Svg.Skia.Svg
    {
        public static readonly StyledProperty<string> SvgClassProperty =
            AvaloniaProperty.Register<ColoredSvg, string>(nameof(SvgClass), "");

        public string SvgClass
        {
            get => GetValue(SvgClassProperty);
            set => SetValue(SvgClassProperty, value);
        }

        public static readonly StyledProperty<IBrush> SvgColorProperty =
            AvaloniaProperty.Register<ColoredSvg, IBrush>(nameof(SvgColor), new SolidColorBrush(Colors.Black), defaultBindingMode: BindingMode.TwoWay);

        public IBrush SvgColor
        {
            get => GetValue(SvgColorProperty);
            set => SetValue(SvgColorProperty, value);
        }

        public ColoredSvg(Uri baseUri) : base(baseUri)
        {
            EnableCache = true;
            this.WhenAnyValue(x => x.SvgColor, x => x.SvgClass)
                .Subscribe(_ => UpdateCss());
        }

        public ColoredSvg(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            EnableCache = true;
            this.WhenAnyValue(x => x.SvgColor, x => x.SvgClass)
                .Subscribe(_ => UpdateCss());
        }

        private void UpdateCss()
        {
            if (SvgColor is ISolidColorBrush solidColorBrush)
            {
                string css = $".{SvgClass} {{ fill: #{solidColorBrush.Color.R:X2}{solidColorBrush.Color.G:X2}{solidColorBrush.Color.B:X2}; }}";
                SetCss(this, css);
            }
        }
    }
}
