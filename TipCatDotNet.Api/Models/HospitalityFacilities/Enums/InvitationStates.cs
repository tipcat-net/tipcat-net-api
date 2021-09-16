using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvitationStates
    {
        None = 0,
        NotSent = 1,
        Sent = 2,
        Accepted = 3
    }
}