// <copyright file="ValueConverters.cs" company="TeamNut">
// Copyright (c) TeamNut. All rights reserved.
// </copyright>

namespace TeamNut
{
    using System;
    using Microsoft.UI.Text;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Data;
    using Microsoft.UI.Xaml.Media;
    using Windows.UI;
    using Windows.UI.Text;

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
    public partial class BoolToVisibilityConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a boolean to a <see cref="FontWeight"/>.</summary>
    public partial class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b && b
                ? ConverterConstants.FontBold
                : ConverterConstants.FontNormal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Inverts a boolean value.</summary>
    public partial class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is bool b ? !b : true;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => value is bool b ? !b : true;
    }

    /// <summary>Converts an empty string to a <see cref="Visibility"/> value.</summary>
    public partial class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(value?.ToString());
            bool inverse =
                parameter?.ToString() == ConverterConstants.ParamInverse;

            return (inverse ? !isEmpty : isEmpty)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a role string to a <see cref="HorizontalAlignment"/>.</summary>
    public partial class RoleToHorizontalAlignmentConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts an integer equal to zero to a <see cref="Visibility"/> value.</summary>
    public partial class IntZeroToVisibilityConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts a role string to a background brush colour.</summary>
    public partial class RoleToBackgroundConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Highlights conversations that have unanswered messages (nutritionist view only).</summary>
    public partial class UnansweredToHighlightConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

    /// <summary>Converts an integer greater than zero to a <see cref="Visibility"/> value.</summary>
    public partial class IntGreaterThanZeroToVisibilityConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}
