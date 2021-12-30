using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionFilterDate
{
    Day = 0,
    Month = 1,
    Year = 2
}