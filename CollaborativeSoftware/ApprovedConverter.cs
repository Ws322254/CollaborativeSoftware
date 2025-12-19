using System;
using System.Globalization;
using System.Windows.Data;

namespace CollaborativeSoftware
{
    public class ApprovedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool approved)
                return approved ? "Approved" : "Pending";

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
