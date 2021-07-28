using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Newtonsoft.Json;

namespace AloysAdjustments.Common.JsonConverters
{
    public class VersionConverter : JsonConverter<Version>
    {
        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var data = reader.Value as string;
            return Version.Parse(data);
        }

        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
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