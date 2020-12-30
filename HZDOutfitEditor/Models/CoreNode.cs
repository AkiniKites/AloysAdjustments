using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;

namespace HZDOutfitEditor.Models
{
    public class CoreNode
    {
        public static CoreNode FromObj(object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            var fName = type.GetField("Name");
            var fId = type.GetField("ObjectUUID");

            var node = new CoreNode()
            {
                Type = type,
                Name = fName?.GetValue(obj)?.ToString() ?? "null",
                Id = fId?.GetValue(obj) as BaseGGUUID,
                Value = obj
            };

            return node;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public BaseGGUUID Id { get; set; }
        public object Value { get; set; }

        public T GetField<T>(string name)
        {
            var fi = Type.GetField(name);
            return (T)fi?.GetValue(Value);
        }
        public string GetString(string name)
        {
            var fi = Type.GetField(name);
            return fi?.GetValue(Value)?.ToString() ?? "null";
        }
    }
}
