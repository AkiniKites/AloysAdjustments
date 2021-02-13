using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.NPC
{
    public partial class NPCPluginView
    {
        public NPCPluginView()
        {
            InitializeComponent();
        }

        private void clbModels_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            clbModels.SelectedItem = null;
        }

        private void lbNPCs_IsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!lbNPCs.IsEnabled)
                lbNPCs.SelectedItem = null;
        }
    }
}
