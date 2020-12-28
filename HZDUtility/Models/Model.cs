using System;
using System.Collections.Generic;
using Decima;

namespace HZDUtility.Models
{
    public class Model
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