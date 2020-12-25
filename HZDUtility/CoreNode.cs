using System;
using System.Collections.Generic;
using System.Text;

namespace HZDUtility
{
    public class CoreNode
    {
        public static CoreNode FromObj(object obj)
        {
            var type = obj.GetType();

            var fName = type.GetField("Name");
            var fUuid = type.GetField("ObjectUUID");

            var node = new CoreNode()
            {
                Type = type,
                Name = fName?.GetValue(obj)?.ToString() ?? "null",
                Value = obj
            };

            return node;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public Guid Uuid { get; set; }
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
