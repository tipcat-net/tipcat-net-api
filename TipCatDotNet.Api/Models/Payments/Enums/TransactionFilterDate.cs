using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFilterDate
{
    [EnumMember(Value = "Day")]
    Day = 0,
    [EnumMember(Value = "Month")]
    Month = 1,
    [EnumMember(Value = "Year")]
    Year = 2
}