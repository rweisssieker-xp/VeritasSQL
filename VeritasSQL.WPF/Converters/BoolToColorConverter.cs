using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VeritasSQL.WPF;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isConnected && isConnected)
            return Colors.Green;
        return Colors.Red;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

