using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace TeamNut
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // if parameter is provided, treat value as string role comparison
            if (parameter != null && value != null)
            {
                var param = parameter.ToString();
                var strVal = value.ToString();
                if (param == "Nutritionist")
                    return string.Equals(strVal, "Nutritionist", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
                if (param == "User")
                    return !string.Equals(strVal, "Nutritionist", StringComparison.OrdinalIgnoreCase) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is bool boolValue)
            {
                if (parameter?.ToString() == "Inverse")
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b && b)
            {
                return Microsoft.UI.Text.FontWeights.Bold;
            }
            return Microsoft.UI.Text.FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }
    }

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(value?.ToString());

            if (parameter?.ToString() == "Inverse")
            {
                return isEmpty ? Visibility.Collapsed : Visibility.Visible;
            }
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var role = value?.ToString() ?? string.Empty;
            return string.Equals(role, "Nutritionist", StringComparison.OrdinalIgnoreCase) ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IntZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return Visibility.Visible;
            if (int.TryParse(value.ToString(), out int intVal))
            {
                bool isZero = intVal == 0;
                if (parameter?.ToString() == "Inverse") return isZero ? Visibility.Collapsed : Visibility.Visible;
                return isZero ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class RoleToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var role = value?.ToString() ?? string.Empty;
            if (string.Equals(role, "Nutritionist", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromArgb(255, 180, 210, 240));
            }
            return new SolidColorBrush(Color.FromArgb(255, 200, 235, 195));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    // Highlights conversations that have unanswered messages, but only for nutritionist users
    public class UnansweredToHighlightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                bool hasUnanswered = false;
                if (value is bool b) hasUnanswered = b;

                // Only highlight for nutritionists
                var role = TeamNut.Models.UserSession.Role ?? string.Empty;
                if (!string.Equals(role, "Nutritionist", StringComparison.OrdinalIgnoreCase))
                {
                    return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)); // transparent
                }

                if (hasUnanswered)
                {
                    // light yellow highlight
                    return new SolidColorBrush(Color.FromArgb(255, 255, 250, 200));
                }

                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            catch
            {
                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
