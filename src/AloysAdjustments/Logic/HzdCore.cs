using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Decima.HZD;

namespace AloysAdjustments.Logic
{
    public class HzdCore
    {
        public string FilePath { get; private set; }
        public List<object> Components { get; private set; }

        public static HzdCore Load(string file)
        {
            return new HzdCore()
            {
                FilePath = file,
                Components = CoreBinary.Load(file)
            };
        }

        private HzdCore() { }

        public void Save(string filePath = null)
        {
            CoreBinary.Save(filePath ?? FilePath, Components);
        }

        public Dictionary<BaseGGUUID, T> GetTypes<T>(string typeName = null) where T : RTTIRefObject
        {
            typeName ??= typeof(T).Name;

            return Components.Where(x => x.GetType().Name == typeName)
                .ToDictionary(x => (BaseGGUUID)((T)x).ObjectUUID, x => (T)x);
        }
    }
}
