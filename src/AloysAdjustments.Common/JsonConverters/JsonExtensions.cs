using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AloysAdjustments.Common.JsonConverters
{
    public static class JsonHelper
    {
        public static JsonConverter[] Converters { get; } = new JsonConverter[]
        {
            new BaseGGUUIDConverter2(),
            new VersionConverter()
        };
    }
}
