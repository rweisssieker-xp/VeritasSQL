using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VeritasSQL.Core.Models;

namespace VeritasSQL.WPF;

public class SeverityToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IssueSeverity severity)
        {
            return severity switch
            {
                IssueSeverity.Error => new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
                IssueSeverity.Warning => new SolidColorBrush(Color.FromArgb(30, 255, 165, 0)),
                IssueSeverity.Info => new SolidColorBrush(Color.FromArgb(30, 0, 0, 255)),
                _ => new SolidColorBrush(Colors.Transparent)
            };
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

