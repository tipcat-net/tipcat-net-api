using System;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.Payments.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum ActiveStripeAccountType
{
    Organizational = 1,
    Personal = 2
}