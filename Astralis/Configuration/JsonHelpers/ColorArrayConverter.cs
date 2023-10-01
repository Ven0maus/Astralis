using Astralis.Extended;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Configuration.JsonHelpers
{
    public class ColorArrayConverter : JsonConverter<Color[]>
    {
        public override Color[] ReadJson(JsonReader reader, Type objectType, Color[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var colors = new List<Color>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    if (reader.Value == null) continue;
                    if (reader.TokenType == JsonToken.String)
                    {
                        var str = (string)reader.Value;
                        colors.Add(str.HexToColor());
                    }
                    else
                    {
                        throw new Exception($"Invalid value [{reader.TokenType}:{reader.Value ?? "null"}], cannot convert.");
                    }
                }

                return colors.ToArray();
            }
            throw new Exception($"Could not convert value [{reader.TokenType}:{reader.Value ?? "null"}].");
        }

        public override void WriteJson(JsonWriter writer, Color[] value, JsonSerializer serializer)
        {
            writer.WriteValue(string.Join(", ", value.Select(a => a.ToHex())));
        }
    }
}
