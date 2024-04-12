using Avalonia.Data.Converters;
using SpellCrafter.Enums;
using System;
using System.Globalization;

namespace SpellCrafter.Converters
{
    public class AddonStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is AddonState state &&
            parameter is AddonState parameterState &&
            state.Equals(parameterState);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
