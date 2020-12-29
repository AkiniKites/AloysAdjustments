using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace HZDUtility.Utility
{
    public static class Extensions
    {
        public static bool TryBeginInvoke(this Control control, Action method)
        {
            if (!control.IsHandleCreated || control.IsDisposed)
                return false;

            try
            {
                if (control.InvokeRequired)
                    control.BeginInvoke(method);
                else
                    method.Invoke();
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }
    }
}
