using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloysAdjustments.Utility
{
    public static class Extensions
    {
        public static Dictionary<TKey, TValue> ToSoftDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> source, Func<TSource, TKey> key, Func<TSource, TValue> value)
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var item in source)
                dict[key(item)] = value(item);
            return dict;
        }
    }
}
