using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AloysAdjustments.Common.UI.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        private Visibility _visibilityWhenHidden = Visibility.Collapsed;
        public Visibility VisibilityWhenHidden
        {
            get => _visibilityWhenHidden;
            set
            {
                if (value == Visibility.Visible)
                    throw new ArgumentException("VisibilityWhenHidden cannot be set to Visibility.Visible");
                _visibilityWhenHidden = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag)
            {
                if (Inverse)
                    flag = !flag;
            }
            else
            {
                flag = false;
            }

            return flag ? Visibility.Visible : VisibilityWhenHidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}
