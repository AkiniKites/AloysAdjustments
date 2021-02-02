using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace AloysAdjustments.WPF.UI
{
    public class UIColors
    {
        public static readonly Color ErrorColor = Color.FromRgb(204, 0, 0);
        public static readonly Brush ErrorBrush = new SolidColorBrush(ErrorColor);
        public static readonly Color OkColor = Colors.ForestGreen;
        public static readonly Brush OkBrush = new SolidColorBrush(OkColor);
    }
}
