using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AloysAdjustments.Common.UI
{
    public class UIColors
    {
        public static readonly Color ErrorColor = Color.FromRgb(204, 0, 0);
        public static readonly Brush ErrorBrush = new SolidColorBrush(ErrorColor);
        public static readonly Color OkColor = Colors.ForestGreen;
        public static readonly Brush OkBrush = new SolidColorBrush(OkColor);
    }
}
