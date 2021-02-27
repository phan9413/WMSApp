using System;
using Xamarin.Forms;
using System.Globalization;
namespace WMSApp.Converters
{
    class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && value != null)
            {
                switch (value.ToString())
                {
                    case "Success":
                        return Color.Green;
                    case "Warning":
                        return Color.Yellow;
                    default:
                        return Color.Gray;
                }
            }
            else
                return Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
