using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Plugins.Outfits
{
    public class ValuePair<T>
    {
        public T Default { get; set; }
        public T Value { get; set; }

        public ValuePair(T defaultValue, T value)
        {
            Default = defaultValue;
            Value = value;
        }
    }
}