using System.Collections.Generic;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Payments;

public readonly struct FacilityTransactionResponse
{
    public FacilityTransactionResponse(FacilityResponse facility, decimal totalTips)
    {
        Facility = facility;
        TotalTips = totalTips;
    }


    public FacilityResponse Facility { get; }
    public decimal TotalTips { get; }
}