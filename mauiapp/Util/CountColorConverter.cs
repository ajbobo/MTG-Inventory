using System.Globalization;

namespace mauiapp;

public class CountColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        CardData card = value as CardData;
        if (card.TotalCount >= 4)
            return Colors.Lime;
        else if (card.TotalCount > 0)
            return Colors.Orange;
        
        return Colors.LightSlateGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}