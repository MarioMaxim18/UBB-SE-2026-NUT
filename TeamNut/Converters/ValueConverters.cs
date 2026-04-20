using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Text;

namespace TeamNut
{
    internal static class ConverterConstants
    {
        public const string ParamInverse = "Inverse";
        public const string RoleNutritionist = "Nutritionist";
        public const string RoleUser = "User";
        public static readonly FontWeight FontBold = FontWeights.Bold;
        public static readonly FontWeight FontNormal = FontWeights.Normal;
        public const int VisibleThreshold = 0;
        public static readonly Color Transparent = Color.FromArgb(0, 0, 0, 0);
        public static readonly Color NutritionistBackground = Color.FromArgb(255, 180, 210, 240);
        public static readonly Color UserBackground = Color.FromArgb(255, 200, 235, 195);
        public static readonly Color UnansweredHighlight = Color.FromArgb(255, 255, 250, 200);
    }

    /// <summary>Converts a boolean or role string to a <see cref="Visibility"/> value.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>Converts a value to <see cref="Visibility"/>.</summary>
        /// <param name="value">The input value (bool or role string).</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The appropriate <see cref="Visibility"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null && value != null)
            {
                var param = parameter.ToString();
                var strVal = value.ToString();

                if (param == ConverterConstants.RoleNutritionist)
                {
                    return string.Equals(
                        strVal,
                        ConverterConstants.RoleNutritionist,
                        StringComparison.OrdinalIgnoreCase)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }

                if (param == ConverterConstants.RoleUser)
                {
                    return !string.Equals(
                        strVal,
                        ConverterConstants.RoleNutritionist,
                        StringComparison.OrdinalIgnoreCase)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }

            if (value is bool boolValue)
            {
                bool inverse =
                    parameter?.ToString() == ConverterConstants.ParamInverse;

                bool show = inverse ? !boolValue : boolValue;
                return show ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a boolean to a <see cref="FontWeight"/>.</summary>
    public class BoolToFontWeightConverter : IValueConverter
    {
        /// <summary>Converts a boolean to a font weight.</summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>Bold when <c>true</c>; otherwise Normal.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b && b
                ? ConverterConstants.FontBold
                : ConverterConstants.FontNormal;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Inverts a boolean value.</summary>
    public class InverseBoolConverter : IValueConverter
    {
        /// <summary>Returns the logical negation of the input boolean.</summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The inverted boolean.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is bool b ? !b : true;

        /// <summary>Returns the logical negation of the input boolean.</summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The inverted boolean.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => value is bool b ? !b : true;
    }

    /// <summary>Converts an empty string to a <see cref="Visibility"/> value.</summary>
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        /// <summary>Returns Visible when the string is empty (or Inverse).</summary>
        /// <param name="value">The string to check.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Pass "Inverse" to reverse the logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The appropriate <see cref="Visibility"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(value?.ToString());
            bool inverse =
                parameter?.ToString() == ConverterConstants.ParamInverse;

            return (inverse ? !isEmpty : isEmpty)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a role string to a <see cref="HorizontalAlignment"/>.</summary>
    public class RoleToHorizontalAlignmentConverter : IValueConverter
    {
        /// <summary>Returns Left for nutritionist, Right for user.</summary>
        /// <param name="value">The role string.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The appropriate <see cref="HorizontalAlignment"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var role = value?.ToString() ?? string.Empty;

            return string.Equals(
                role,
                ConverterConstants.RoleNutritionist,
                StringComparison.OrdinalIgnoreCase)
                ? HorizontalAlignment.Left
                : HorizontalAlignment.Right;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts an integer equal to zero to a <see cref="Visibility"/> value.</summary>
    public class IntZeroToVisibilityConverter : IValueConverter
    {
        /// <summary>Returns Visible when the integer is zero.</summary>
        /// <param name="value">The integer value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Pass "Inverse" to reverse the logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The appropriate <see cref="Visibility"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool inverse =
                parameter?.ToString() == ConverterConstants.ParamInverse;

            if (value == null)
            {
                return Visibility.Visible;
            }

            if (int.TryParse(value.ToString(), out int intVal))
            {
                bool isZero = intVal == ConverterConstants.VisibleThreshold;
                return (inverse ? !isZero : isZero)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a role string to a background brush colour.</summary>
    public class RoleToBackgroundConverter : IValueConverter
    {
        /// <summary>Returns a coloured brush based on the role string.</summary>
        /// <param name="value">The role string.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>A <see cref="SolidColorBrush"/> for the role.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var role = value?.ToString() ?? string.Empty;

            var color = string.Equals(
                role,
                ConverterConstants.RoleNutritionist,
                StringComparison.OrdinalIgnoreCase)
                ? ConverterConstants.NutritionistBackground
                : ConverterConstants.UserBackground;

            return new SolidColorBrush(color);
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Highlights conversations that have unanswered messages (nutritionist view only).</summary>
    public class UnansweredToHighlightConverter : IValueConverter
    {
        /// <summary>Returns a highlight brush for unanswered conversations when viewed by a nutritionist.</summary>
        /// <param name="value">The HasUnanswered boolean.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Optional converter parameter.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>A <see cref="SolidColorBrush"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                bool hasUnanswered = value is bool b && b;

                var role = TeamNut.Models.UserSession.Role ?? string.Empty;
                if (!string.Equals(
                        role,
                        ConverterConstants.RoleNutritionist,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(
                        ConverterConstants.Transparent);
                }

                return new SolidColorBrush(
                    hasUnanswered
                        ? ConverterConstants.UnansweredHighlight
                        : ConverterConstants.Transparent);
            }
            catch
            {
                return new SolidColorBrush(ConverterConstants.Transparent);
            }
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts an integer greater than zero to a <see cref="Visibility"/> value.</summary>
    public class IntGreaterThanZeroToVisibilityConverter : IValueConverter
    {
        /// <summary>Returns Visible when the integer is greater than zero.</summary>
        /// <param name="value">The integer value.</param>
        /// <param name="targetType">The target binding type.</param>
        /// <param name="parameter">Pass "Inverse" to reverse the logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The appropriate <see cref="Visibility"/>.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool inverse =
                parameter?.ToString() == ConverterConstants.ParamInverse;

            bool visible = value switch
            {
                int i => i > ConverterConstants.VisibleThreshold,
                long l => l > ConverterConstants.VisibleThreshold,
                _ => false
            };

            return (inverse ? !visible : visible)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>Not implemented.</summary>
        /// <param name="value">Unused.</param>
        /// <param name="targetType">Unused.</param>
        /// <param name="parameter">Unused.</param>
        /// <param name="language">Unused.</param>
        /// <returns>Always throws.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
