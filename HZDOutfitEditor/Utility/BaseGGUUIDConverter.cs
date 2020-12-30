using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Decima;
using Newtonsoft.Json;

namespace HZDOutfitEditor.Utility
{
    public class BaseGGUUIDConverter : JsonConverter<BaseGGUUID>
    {
        public override BaseGGUUID ReadJson(JsonReader reader, Type objectType, [AllowNull] BaseGGUUID existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var data = reader.Value as string;
            if (data.Length != 32)
                throw new HzdException($"Invalid BaseGGUUID data length: {data.Length}");

            var i = 0;
            var id = new BaseGGUUID();
            id.Data3 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data2 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data1 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data0 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);

            id.Data5 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data4 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);

            id.Data7 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data6 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);

            id.Data8 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data9 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data10 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data11 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data12 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data13 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data14 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);
            id.Data15 = Convert.ToByte(data.Substring((i++ * 2), 2), 16);

            return id;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] BaseGGUUID value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var str = $"{value.Data3:X2}{value.Data2:X2}{value.Data1:X2}{value.Data0:X2}{value.Data5:X2}{value.Data4:X2}{value.Data7:X2}" +
                    $"{value.Data6:X2}{value.Data8:X2}{value.Data9:X2}{value.Data10:X2}{value.Data11:X2}{value.Data12:X2}{value.Data13:X2}{value.Data14:X2}{value.Data15:X2}";
                writer.WriteValue(str);
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
