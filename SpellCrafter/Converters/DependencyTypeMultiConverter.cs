using Avalonia.Data.Converters;
using SpellCrafter.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using SpellCrafter.ViewModels;

namespace SpellCrafter.Converters
{
    public class DependencyTypeMultiConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is not [Addon addon, _] || values[1] == null)
                return null;

            return values[1] switch
            {
                InstalledAddonsViewModel => addon.LocalDependencies,
                BrowseViewModel => addon.OnlineDependencies,
                _ => null
            };
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}