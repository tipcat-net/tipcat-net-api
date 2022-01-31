using System.Collections.Generic;
using HappyTravel.Money.Models;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Analitics;

public readonly struct FacilityStatsResponse
{
    public FacilityStatsResponse(int id, List<MoneyAmount> amount)
    {
        Id = id;
        Amount = amount;
    }


    public FacilityStatsResponse(int id, List<MemberStatsResponse>? members, List<MoneyAmount> amount)
    {
        Id = id;
        Members = members;
        Amount = amount;
    }


    public int Id { get; }
    public List<MemberStatsResponse>? Members { get; } = null!;
    public List<MoneyAmount> Amount { get; }
}


public readonly struct MemberStatsResponse
{
    public MemberStatsResponse(int id, List<MoneyAmount> amount)
    {
        Id = id;
        Amount = amount;
    }


    public int Id { get; }
    public List<MoneyAmount> Amount { get; }
}