using System.Text.Json.Serialization;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Models.Preferences;

public readonly struct PreferencesRequest
{
    [JsonConstructor]
    public PreferencesRequest(AccountPreferences serverSidePreferences, string applicationPreferences)
    {
        ApplicationPreferences = applicationPreferences;
        ServerSidePreferences = serverSidePreferences;
    }

    
    /// <summary>
    /// Application preferences in a free-form JSON format. Object's total length, including server-side preferences, must not exceed 256KB.
    /// </summary>
    public string ApplicationPreferences { get; }
    public AccountPreferences ServerSidePreferences { get; }
}