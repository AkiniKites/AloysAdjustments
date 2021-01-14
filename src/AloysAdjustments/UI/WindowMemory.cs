using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AloysAdjustments.Configuration;
using AloysAdjustments.Logic;

namespace AloysAdjustments.UI
{
    [Flags]
    public enum WindowMemoryType
    {
        Position = 1,
        Size = 2,
        All = Position | Size
    }

    public static class WindowMemory
    {
        static WindowMemory()
        {
            Windows = new Dictionary<string, TrackingWindow>();
        }

        private class TrackingWindow
        {
            public Form Window;
            public string Name;

            public void AttachEvents()
            {
                Window.Closing += Window_Closing;
            }

            private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                Window.Closing -= Window_Closing;
                SaveAndForget(Name);
            }
        }

        private static Dictionary<string, TrackingWindow> Windows { get; }

        /// <summary>
        /// Saves and restores window position
        /// Note: There must be at least one other setting in Properties.Settings. To allow the window to be saves.
        /// </summary>
        public static void ActivateWindow(Form window, string name, 
            WindowMemoryType memoryType = WindowMemoryType.All, bool attachEvents = true)
        {
            var win = new TrackingWindow()
            {
                Window = window,
                Name = name
            };

            if (Windows.ContainsKey(name))
                throw new WindowMemoryException($"Window already activated: {name}");
            Windows.Add(name, win);
            
            if (IoC.Settings.Windows.TryGetValue(win.Name, out Rectangle rect))
            {
                if (memoryType.HasFlag(WindowMemoryType.Size))
                {
                    window.Width = rect.Width;
                    window.Height = rect.Height;
                }
                if (memoryType.HasFlag(WindowMemoryType.Position))
                {
                    var area = new Rectangle(rect.Left, rect.Top, window.Width, window.Height);
                    if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(area)))
                    {
                        window.StartPosition = FormStartPosition.Manual;
                        window.Top = rect.Top;
                        window.Left = rect.Left;
                    }
                    else
                    {
                        window.StartPosition = FormStartPosition.WindowsDefaultLocation;
                    }
                }
            }

            if (attachEvents)
                win.AttachEvents();
        }

        public static void SaveAndForget(string name)
        {
            if (!Windows.TryGetValue(name, out TrackingWindow window))
                throw new WindowMemoryException($"Window name not found: {name}");

            var r = new Rectangle(window.Window.Left, window.Window.Top, 
                window.Window.Width, window.Window.Height);
            IoC.Settings.Windows[window.Name] = r;
            SettingsManager.Save();

            Windows.Remove(name);
        }
    }
    
    public class WindowMemoryException : Exception
    {
        public WindowMemoryException(string message) : base(message) { }
    }
}
