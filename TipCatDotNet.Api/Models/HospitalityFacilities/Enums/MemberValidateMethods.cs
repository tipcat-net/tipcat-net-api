using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MemberValidateMethods
    {
        Add,
        Get,
        GetAll,
        Remove,
        Update,
        RegenerateQR
    }

}