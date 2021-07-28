using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Newtonsoft.Json;

namespace AloysAdjustments.Common.JsonConverters
{
    public class BaseGGUUIDConverter2 : JsonConverter<BaseGGUUID>
    {
        public override BaseGGUUID ReadJson(JsonReader reader, Type objectType, BaseGGUUID existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var data = reader.Value as string;
            return data;
        }

        public override void WriteJson(JsonWriter writer, BaseGGUUID value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}