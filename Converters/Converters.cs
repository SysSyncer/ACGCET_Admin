using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ACGCET_Admin.Converters
{
    /// <summary>Returns Collapsed when bool is true, Visible when false.</summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is Visibility v && v == Visibility.Collapsed;
    }

    /// <summary>Returns !bool.</summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }

    /// <summary>Maps alert severity string to a muted background brush.</summary>
    public class SeverityToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) switch
            {
                "Critical" => new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2)),
                "High"     => new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xEB)),
                "Medium"   => new SolidColorBrush(Color.FromRgb(0xFE, 0xFC, 0xE8)),
                "Low"      => new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xF4)),
                _          => new SolidColorBrush(Color.FromRgb(0xF4, 0xF4, 0xF5)),
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps alert severity string to a restrained foreground brush.</summary>
    public class SeverityToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) switch
            {
                "Critical" => new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)),
                "High"     => new SolidColorBrush(Color.FromRgb(0x92, 0x40, 0x0E)),
                "Medium"   => new SolidColorBrush(Color.FromRgb(0x85, 0x4D, 0x0E)),
                "Low"      => new SolidColorBrush(Color.FromRgb(0x16, 0x65, 0x34)),
                _          => new SolidColorBrush(Color.FromRgb(0x71, 0x71, 0x7A)),
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps audit action type to a muted background brush.</summary>
    public class ActionTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string ?? "";
            if (s.StartsWith("INSERT"))       return new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xF4));
            if (s.StartsWith("UPDATE"))       return new SolidColorBrush(Color.FromRgb(0xFE, 0xFC, 0xE8));
            if (s.StartsWith("DELETE"))       return new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2));
            if (s.Contains("MODULE"))         return new SolidColorBrush(Color.FromRgb(0xEF, 0xF6, 0xFF));
            if (s.Contains("LOCK"))           return new SolidColorBrush(Color.FromRgb(0xEE, 0xF2, 0xFF));
            if (s.Contains("CORRECTION"))     return new SolidColorBrush(Color.FromRgb(0xFD, 0xF2, 0xF8));
            return new SolidColorBrush(Color.FromRgb(0xF4, 0xF4, 0xF5));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps audit action type to a restrained foreground brush.</summary>
    public class ActionTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string ?? "";
            if (s.StartsWith("INSERT"))       return new SolidColorBrush(Color.FromRgb(0x16, 0x65, 0x34));
            if (s.StartsWith("UPDATE"))       return new SolidColorBrush(Color.FromRgb(0x85, 0x4D, 0x0E));
            if (s.StartsWith("DELETE"))       return new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B));
            if (s.Contains("MODULE"))         return new SolidColorBrush(Color.FromRgb(0x1D, 0x4E, 0xD8));
            if (s.Contains("LOCK"))           return new SolidColorBrush(Color.FromRgb(0x37, 0x30, 0xA3));
            if (s.Contains("CORRECTION"))     return new SolidColorBrush(Color.FromRgb(0x9D, 0x17, 0x4D));
            return new SolidColorBrush(Color.FromRgb(0x52, 0x52, 0x5B));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>bool IsLocked → muted background (red=locked, green=open).</summary>
    public class LockToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2))
                : new SolidColorBrush(Color.FromRgb(0xF0, 0xFD, 0xF4));
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>bool IsLocked → restrained foreground text color.</summary>
    public class LockToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B))
                : new SolidColorBrush(Color.FromRgb(0x16, 0x65, 0x34));
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }
}
