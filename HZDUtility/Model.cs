using Decima;
using System;
using System.Collections.Generic;

namespace HZDUtility
{
    public abstract class Model
    {
        public string Name { get; set; }
        public BaseGGUUID Id { get; set; }

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
            return HashCode.Combine(Id);
        }
    }
}