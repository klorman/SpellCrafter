using Avalonia.Controls;
using System;
using Avalonia.Media.Imaging;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;

namespace SpellCrafter.Converters
{
    public class IconImageConverter : IValueConverter
    {
        private static IconImageConverter _sConverter = new IconImageConverter();

        public static IconImageConverter Converter => _sConverter;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is WindowIcon windowIcon)
            {
                Bitmap? result = null;

                using (var stream = new MemoryStream())
                {
                    windowIcon.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    result = new Bitmap(stream);
                }

                return result;
            }

            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
