using Microsoft.AspNetCore.Http;

namespace TipCatDotNet.Api.Models.Images;

public readonly struct AccountAvatarRequest
{
    public AccountAvatarRequest(int accountId, FormFile? file = null)
    {
        AccountId = accountId;
        File = file;
    }


    public int AccountId { get; }
    public FormFile? File { get; }
}