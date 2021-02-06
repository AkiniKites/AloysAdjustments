using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace AloysAdjustments.Plugins.Outfits.Data
{
    public class Model : INotifyPropertyChanged
    {
        public BaseGGUUID Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string DisplayName { get; set; }

        public bool? Checked { get; set; }

        public Model()
        {
            Checked = false;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Model model &&
                EqualityComparer<BaseGGUUID>.Default.Equals(Id, model.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}