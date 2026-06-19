using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MALDiscordPresence.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string status = value?.ToString()?.ToLowerInvariant() ?? string.Empty;

        string brushKey = status switch
        {
            "watching" or "reading" => "PrimaryRubyBrush",
            "completed" => "SuccessBrush",
            "on_hold" => "WarningBrush",
            "dropped" => "TextSecondaryBrush",
            _ => "DividerBrush"
        };

        if (System.Windows.Application.Current.TryFindResource(brushKey) is System.Windows.Media.Brush brush)
        {
            return brush;
        }

        return System.Windows.Media.Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
