using System.Globalization;
using System.Windows.Data;
using ICSharpCode.AvalonEdit.Document;

namespace VeritasSQL.WPF;

public class TextToDocumentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            return new TextDocument(text);
        }
        return new TextDocument();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TextDocument document)
        {
            return document.Text;
        }
        return string.Empty;
    }
}

