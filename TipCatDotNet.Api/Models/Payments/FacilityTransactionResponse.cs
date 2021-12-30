using HappyTravel.Money.Models;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Payments;

public readonly struct FacilityTransactionResponse
{
    public FacilityTransactionResponse(FacilityResponse facility, MoneyAmount amount)
    {
        Facility = facility;
        Amount = amount;
    }


    public FacilityResponse Facility { get; }
    public MoneyAmount Amount { get; }
}