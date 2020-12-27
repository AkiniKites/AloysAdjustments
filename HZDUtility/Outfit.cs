using Decima;
using System;
using System.Collections.Generic;
using System.Text;

namespace HZDUtility
{
    public class Outfit : Model
    {
        public override string ToString()
        {
            var key = "playercostume_";

            var idx = Name.LastIndexOf(key) + key.Length;
            if (idx >= 0)
                return Name.Substring(idx);

            return Name;
        }
    }
}
