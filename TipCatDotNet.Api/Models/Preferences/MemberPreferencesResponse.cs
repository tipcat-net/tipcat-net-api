using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Models.Preferences;

public readonly struct PreferencesResponse
{
    public PreferencesResponse(AccountPreferences serverSidePreferences, string applicationSidePreferences)
    {
        ApplicationSidePreferences = applicationSidePreferences;
        ServerSidePreferences = serverSidePreferences;
    }


    public string ApplicationSidePreferences { get; }
    public AccountPreferences ServerSidePreferences { get; }
}