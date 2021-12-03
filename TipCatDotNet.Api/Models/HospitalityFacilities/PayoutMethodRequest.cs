using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public class PayoutMethodRequest
{
    [JsonConstructor]
    public PayoutMethodRequest(int memberId, string token)
    {
        MemberId = memberId;
        Token = token;
    }

    public int MemberId { get; }
    public string Token { get; } // Token from front-end Stripe.js library
}