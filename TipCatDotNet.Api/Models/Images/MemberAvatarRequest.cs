using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.Images;

public readonly struct MemberAvatarRequest
{
    public MemberAvatarRequest(int accountId, int memberId, FormFile? file)
    {
        AccountId = accountId;
        MemberId = memberId;
        File = file;
    }


    public int AccountId { get; }
    public FormFile? File { get; }
    public int MemberId { get; }
}