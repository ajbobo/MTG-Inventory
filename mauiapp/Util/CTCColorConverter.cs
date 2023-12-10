using System.Globalization;

namespace mauiapp;

public class CTCColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // This gets called a few times with values[] not set yet - make sure that we have both values before doing anything
        if (values == null || values[0] == null || values[1] == null)
            return null;

        int count = (int)values[0];
        int tempCount = (int)values[1];
        if (tempCount > count)
            return Colors.Lime;
        else if (tempCount < count)
            return Colors.Red;
        
        return null;
    }

    public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}