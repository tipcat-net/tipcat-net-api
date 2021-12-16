using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFilterProperty
{
    [EnumMember(Value = "Created")]
    Created = 0,
    [EnumMember(Value = "Amount")]
    Amount = 1
}
