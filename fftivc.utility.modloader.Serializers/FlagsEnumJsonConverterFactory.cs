using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Serializers;

/// <summary>
/// Json converter factory to convert enum flags into arrays of elements for each flag.
/// </summary>
public class FlagsEnumJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum && typeToConvert.GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0;
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(FlagsEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }

    private class FlagsEnumJsonConverter<T> : JsonConverter<T> where T : struct
    {
        /// <inheritdoc/>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            long result = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType != JsonTokenType.String)
                    throw new JsonException();

                string flagString = reader.GetString()!;
                if (!Enum.TryParse(flagString, out T flag))
                    throw new JsonException($"Invalid enum flag: {flagString}");

                result |= Convert.ToInt64(flag);
            }

            return (T)Enum.ToObject(typeof(T), result);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var enumValue = Convert.ToInt64(value);
            var flags = Enum.GetValues(typeof(T))
                            .Cast<Enum>()
                            .Where(f => (Convert.ToInt64(f) & enumValue) != 0)
                            .Select(f => f.ToString())
                            .ToArray();

            writer.WriteStartArray();
            foreach (var flag in flags)
                writer.WriteStringValue(flag);

            writer.WriteEndArray();
        }
    }
}