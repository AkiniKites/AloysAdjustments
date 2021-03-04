using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AloysAdjustments.Configuration
{
    public class UserSettings : INotifyPropertyChanged
    {
        public UserSettings()
        {
            Windows = new Dictionary<string, Rectangle>();
        }

        public int Version { get; set; }

        public Dictionary<string, JObject> PluginSettings { get; set; }

        public string GamePath { get; set; }
        public string LastPackOpen { get; set; }

        public Dictionary<string, Rectangle> Windows { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
