namespace TipCatDotNet.Api.Models.HospitalityFacilities;

public class StripeAccountResponse
{
    public StripeAccountResponse(string id)
    {
        Id = id;
    }


    public string Id { get; }
}