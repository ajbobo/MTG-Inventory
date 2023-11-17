using System.Globalization;

namespace mauiapp;

public class PriceColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        CardData card = value as CardData;
        if (card.Card.Price >= 10)
            return Colors.LightGreen;
        
        return Colors.LightSlateGray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}