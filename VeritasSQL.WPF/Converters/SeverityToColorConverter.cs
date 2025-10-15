using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VeritasSQL.Core.Models;

namespace VeritasSQL.WPF;

public class SeverityToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IssueSeverity severity)
        {
            return severity switch
            {
                IssueSeverity.Error => new SolidColorBrush(Colors.Red),
                IssueSeverity.Warning => new SolidColorBrush(Colors.Orange),
                IssueSeverity.Info => new SolidColorBrush(Colors.Blue),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

