using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AloysAdjustments.UI
{
    public class SingleLineConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return Convert(str);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public static string Convert(string text)
        {
            var idx = text.IndexOf("\r\n");
            if (idx < 0)
                idx = text.IndexOf("\n");
            if (idx < 0)
                return text;

            return text.Substring(0, idx);
        }
    }
}
