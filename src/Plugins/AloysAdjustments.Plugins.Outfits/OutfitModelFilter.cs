using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Outfits
{
    public class OutfitModelFilter : INotifyPropertyChanged
    {
        public static OutfitModelFilter Armor = new OutfitModelFilter("Armors", 1);
        public static OutfitModelFilter Characters = new OutfitModelFilter("Main Characters", 2);
        public static OutfitModelFilter AllCharacters = new OutfitModelFilter("All Characters", 4);
        public static OutfitModelFilter[] All = { Armor, Characters, AllCharacters };

        private OutfitModelFilter(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public int Value { get; }

        public bool IsFlag(int flags) => (flags & Value) == Value;

        public override bool Equals(object obj)
        {
            return obj is OutfitModelFilter filter &&
                Value == filter.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(OutfitModelFilter left, OutfitModelFilter right)
        {
            return EqualityComparer<OutfitModelFilter>.Default.Equals(left, right);
        }

        public static bool operator !=(OutfitModelFilter left, OutfitModelFilter right)
        {
            return !(left == right);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}