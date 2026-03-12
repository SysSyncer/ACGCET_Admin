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

    /// <summary>Maps alert severity string to a pastel background brush.</summary>
    public class SeverityToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) switch
            {
                "Critical" => new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE)),
                "High"     => new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xE0)),
                "Medium"   => new SolidColorBrush(Color.FromRgb(0xFF, 0xFD, 0xE7)),
                "Low"      => new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9)),
                _          => new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5)),
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps alert severity string to a dark foreground brush.</summary>
    public class SeverityToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value as string) switch
            {
                "Critical" => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
                "High"     => new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
                "Medium"   => new SolidColorBrush(Color.FromRgb(0xF5, 0x7F, 0x17)),
                "Low"      => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
                _          => new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61)),
            };
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps audit action type to a pastel background brush.</summary>
    public class ActionTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string ?? "";
            if (s.StartsWith("INSERT"))       return new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9));
            if (s.StartsWith("UPDATE"))       return new SolidColorBrush(Color.FromRgb(0xFF, 0xF8, 0xE1));
            if (s.StartsWith("DELETE"))       return new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE));
            if (s.Contains("MODULE"))         return new SolidColorBrush(Color.FromRgb(0xE3, 0xF2, 0xFD));
            if (s.Contains("LOCK"))           return new SolidColorBrush(Color.FromRgb(0xE8, 0xEA, 0xF6));
            if (s.Contains("CORRECTION"))     return new SolidColorBrush(Color.FromRgb(0xFC, 0xE4, 0xEC));
            return new SolidColorBrush(Color.FromRgb(0xF3, 0xE5, 0xF5));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>Maps audit action type to a dark foreground brush.</summary>
    public class ActionTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string ?? "";
            if (s.StartsWith("INSERT"))       return new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
            if (s.StartsWith("UPDATE"))       return new SolidColorBrush(Color.FromRgb(0xF5, 0x7F, 0x17));
            if (s.StartsWith("DELETE"))       return new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
            if (s.Contains("MODULE"))         return new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));
            if (s.Contains("LOCK"))           return new SolidColorBrush(Color.FromRgb(0x28, 0x3C, 0x93));
            if (s.Contains("CORRECTION"))     return new SolidColorBrush(Color.FromRgb(0x88, 0x00, 0x3E));
            return new SolidColorBrush(Color.FromRgb(0x6A, 0x1B, 0x9A));
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>bool IsLocked → pastel background (red=locked, green=open).</summary>
    public class LockToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE))
                : new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9));
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }

    /// <summary>bool IsLocked → foreground text color.</summary>
    public class LockToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b
                ? new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28))
                : new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => DependencyProperty.UnsetValue;
    }
}
