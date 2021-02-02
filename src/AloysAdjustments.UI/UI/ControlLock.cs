using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AloysAdjustments.UI;

namespace AloysAdjustments.WPF.UI
{
    public class ControlLock : IDisposable
    {
        private readonly Control _control;
        private readonly ControlRelay _controlRelay;

        public ControlLock(Control control)
        {
            _control = control;
            control.IsEnabled = false;
        }
        public ControlLock(ControlRelay control)
        {
            _controlRelay = control;
            control.Enabled = false;
        }

        public void Dispose()
        {
            if (_control != null) _control.IsEnabled = true;
            if (_controlRelay != null) _controlRelay.Enabled = true;
        }
    }
}
