using System;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Flags]
    public enum MemberPermissions
    {
        None = 1,
        Manager = 2,
        Supervisor = 4,
        Employee = 8
    }
}
