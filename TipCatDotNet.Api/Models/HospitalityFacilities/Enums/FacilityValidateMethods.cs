using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FacilityValidateMethods
    {
        Add,
        AddDefault,
        Get,
        GetAll,
        Remove,
        Update,
        TransferMember
    }
}