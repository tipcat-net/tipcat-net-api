

using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortVariant
{
    [EnumMember(Value = "ASC")]
    ASC = 0,
    [EnumMember(Value = "DESC")]
    DESC = 1
}
