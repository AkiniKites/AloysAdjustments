using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AloysAdjustments.Logic;
using AloysAdjustments.Utility;

namespace AloysAdjustments.UI
{
    //exceptions are caught and discarded during async void events so we log them here
    public class Relay
    {
        public static async Task To(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                IoC.Notif.ShowError($"Error: {ex.Message}");
                Errors.WriteError(ex);
            }
        }
        public static async Task To<T1, T2>(T1 arg1, T2 arg2, Func<T1, T2, Task> action)
        {
            try
            {
                await action(arg1, arg2);
            }
            catch (Exception ex)
            {
                IoC.Notif.ShowError($"Error: {ex.Message}");
                Errors.WriteError(ex);
            }
        }
    }
}
