using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFilterProperty
{
    [EnumMember(Value = "CreatedASC")]
    CreatedASC = 0,
    [EnumMember(Value = "CreatedDESC")]
    CreatedDESC = 1,
    [EnumMember(Value = "AmountASC")]
    AmountASC = 2,
    [EnumMember(Value = "AmountDESC")]
    AmountDESC = 3
}
