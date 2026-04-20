using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace TeamNut.Converters
{
    /// <summary>Converts a boolean value to a <see cref="Visibility"/> value.</summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>Converts a boolean to <see cref="Visibility"/>.</summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns><see cref="Visibility.Visible"/> when <c>true</c>; otherwise <see cref="Visibility.Collapsed"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>Converts <see cref="Visibility"/> back to a boolean.</summary>
        /// <param name="value">The visibility value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">An optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns><c>true</c> when <see cref="Visibility.Visible"/>; otherwise <c>false</c>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility v)
            {
                return v == Visibility.Visible;
            }

            return false;
        }
    }
}
