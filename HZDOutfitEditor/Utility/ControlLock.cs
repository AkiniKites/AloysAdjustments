using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HZDOutfitEditor.Utility
{
    public class ControlLock : IDisposable
    {
        private readonly Control _control;

        public ControlLock(Control control)
        {
            _control = control;
            control.Enabled = false;
        }

        public void Dispose()
        {
            _control.Enabled = true;
        }
    }
}
