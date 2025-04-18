
using System;
using System.Globalization;
using System.Windows.Data;
using Garbage_Collector.View;

namespace Garbage_Collector.Utilities
{
    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value == null || !(value is int))
            {
                return string.Empty;
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;

            if (string.IsNullOrWhiteSpace(input))
            {
                return 0; 
            }

            if (int.TryParse(input, out int result))
            {
                return result;
            }

           
            return 0;
        }
    }
}
