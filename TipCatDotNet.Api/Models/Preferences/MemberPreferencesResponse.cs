using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Models.Preferences;

public readonly struct PreferencesResponse
{
    public PreferencesResponse(AccountPreferences serverSidePreferences, string applicationPreferences)
    {
        ApplicationPreferences = applicationPreferences;
        ServerSidePreferences = serverSidePreferences;
    }


    public string ApplicationPreferences { get; }
    public AccountPreferences ServerSidePreferences { get; }
}