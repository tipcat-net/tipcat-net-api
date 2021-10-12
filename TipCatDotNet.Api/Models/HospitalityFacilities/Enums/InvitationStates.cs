using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InvitationStates
    {
        None = 0,
        NotSent = 10,
        Sent = 20,
        Accepted = 30
    }
}