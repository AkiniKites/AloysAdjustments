using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AloysAdjustments.Utility
{
    public class PackList
    {
        private readonly List<string> _packs;

        public List<string> Packs => _packs.ToList();

        public PackList(IEnumerable<string> packs)
        {
            _packs = packs.ToList();
        }

        public override bool Equals(object obj)
        {
            return obj is PackList list &&
                _packs.SequenceEqual(list._packs);
        }

        public override int GetHashCode()
        {
            const int modifier = 23;

            unchecked
            {
                int hash = 17;

                for (int i = 0; i < _packs.Count; i++)
                    hash += modifier * _packs[i].GetHashCode();

                return hash;
            }
        }
    }
}
