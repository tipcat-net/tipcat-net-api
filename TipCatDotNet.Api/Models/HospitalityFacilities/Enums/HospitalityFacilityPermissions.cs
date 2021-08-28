using System;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [Flags]
    public enum HospitalityFacilityPermissions
    {
        None = 1,
        Owner = 2,
        Manager = 4,
        Employee = 8
    }
}
