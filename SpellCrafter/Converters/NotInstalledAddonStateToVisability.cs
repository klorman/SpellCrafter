using Avalonia.Data.Converters;
using SpellCrafter.Enums;
using System;
using System.Globalization;

namespace SpellCrafter.Converters
{
    public class NotInstalledAddonStateToVisability : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is AddonState addonState &&
            addonState == AddonState.NotInstalled;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class InvertedNotInstalledAddonStateToVisability : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is AddonState addonState &&
            addonState != AddonState.NotInstalled;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
