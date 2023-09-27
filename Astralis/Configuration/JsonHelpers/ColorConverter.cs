using Astralis.Extended;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;

namespace Astralis.Configuration.JsonHelpers
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string hexColor = (string)reader.Value;
                return hexColor.HexToColor();
            }
            throw new Exception($"Invalid color value [{reader.TokenType}:{reader.Value}], cannot convert.");
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToHex());
        }
    }
}
