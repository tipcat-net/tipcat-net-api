namespace TipCatDotNet.Api.Services.Images;


internal static class AvatarKeyManagementService
{
    public static string BuildAccountKey(int accountId) 
        => $"accounts/{accountId}/avatar";


    public static string BuildFacilityKey(int accountId, int facilityId) 
        => $"accounts/{accountId}/facilities/{facilityId}";


    public static string BuildMemberKey(int accountId, int memberId) 
        => $"accounts/{accountId}/members/{memberId}";
}