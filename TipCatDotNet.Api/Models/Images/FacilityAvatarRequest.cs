using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.Images;

public readonly struct FacilityAvatarRequest
{
    public FacilityAvatarRequest(int accountId, int facilityId, FormFile? file = null)
    {
        AccountId = accountId;
        FacilityId = facilityId;
        File = file;
    }


    public int AccountId { get; }
    public int FacilityId { get; }
    public FormFile? File { get; }
}