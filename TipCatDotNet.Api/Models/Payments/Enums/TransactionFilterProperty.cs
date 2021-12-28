using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFilterProperty
{
    CreatedAsc = 0,
    CreatedDesc = 1,
    AmountAsc = 2,
    AmountDesc = 3
}
