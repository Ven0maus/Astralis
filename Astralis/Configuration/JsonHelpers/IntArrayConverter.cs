using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Astralis.Configuration.JsonHelpers
{
    internal class IntArrayConverter : JsonConverter<int[]>
    {
        public override int[] ReadJson(JsonReader reader, Type objectType, int[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var ints = new List<int>();
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
                        if (str.Length == 1)
                            ints.Add(str[0]);
                    }
                    else if (reader.TokenType == JsonToken.Integer)
                    {
                        ints.Add((int)(long)reader.Value);
                    }
                    else
                    {
                        throw new Exception($"Invalid value [{reader.TokenType}:{reader.Value ?? "null"}], cannot convert.");
                    }
                }

                return ints.ToArray();
            }
            throw new Exception($"Could not convert value [{reader.TokenType}:{reader.Value ?? "null"}].");
        }

        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
