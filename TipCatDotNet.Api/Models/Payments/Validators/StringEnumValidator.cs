using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Validators
{
    /// <summary>
    /// Ovverides base json converter that automaticaly procced through controller endpoint
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StringEnumValidator<T> : JsonConverter<T> where T : struct, Enum
    {
        static readonly TypeCode typeCode = Type.GetTypeCode(typeof(T));

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // It works but returns 500 with meaningful error message. But we need 400 with Result.Failure
            T value = (T)(object)Enum.Parse<T>(reader.GetString()!);

            return value;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (typeCode == TypeCode.String)
                writer.WriteStringValue((string)(object)value);
            else
                writer.WriteStringValue(Convert.ToString(value));
        }
    }
}