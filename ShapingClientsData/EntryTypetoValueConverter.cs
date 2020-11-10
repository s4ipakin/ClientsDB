using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShapingClientsData
{
    [ValueConversion(typeof(object), typeof(int))]
    public class EntryTypetoValueConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null || value == DBNull.Value)
            {
                return 2;
            }
            else
            {
                int number = (int)System.Convert.ChangeType(value, typeof(int));

                if (number > 0)
                    return 1;
                else
                    return 0;
            }
            
        }

        public object ConvertBack(
            object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack not supported");
        }
    }
}
