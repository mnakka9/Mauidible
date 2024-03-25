using System.Globalization;

namespace Mauidible;

public class SelectedItemBackgroundColorConverter : IValueConverter
{
    public object Convert (object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return Colors.Transparent;

        return (Color)value == Colors.Transparent ? Colors.White : Colors.LightGray; // Change colors as needed
    }

    public object ConvertBack (object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SelectedItemTextColorConverter : IValueConverter
{
    public object Convert (object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return Colors.Black;

        return (Color)value == Colors.Transparent ? Colors.Black : Colors.White; // Change colors as needed
    }

    public object ConvertBack (object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
