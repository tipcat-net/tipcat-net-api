using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Infrastructure.Converters.Json;

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeOnly.ParseExact(reader.GetString()!, "T");


    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToLongTimeString());
}