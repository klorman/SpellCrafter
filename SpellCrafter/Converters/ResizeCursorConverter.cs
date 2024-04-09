using Avalonia.Input;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SpellCrafter.Converters
{
    public class ResizeCursorConverter : IValueConverter
    {
        private static ResizeCursorConverter s_converter = new ResizeCursorConverter();

        public static ResizeCursorConverter Converter => s_converter;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool canResize && parameter is string cursorName)
            {
                if (canResize)
                {
                    return Cursor.Parse(cursorName);
                }
            }

            return new Cursor(StandardCursorType.Arrow);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
