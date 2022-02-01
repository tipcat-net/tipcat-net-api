using System.Collections.Generic;
using HappyTravel.Money.Models;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.Analitics;

public readonly struct FacilityStatsResponse
{
    public FacilityStatsResponse(int id, List<MoneyAmount> amounts)
    {
        Id = id;
        Amounts = amounts;
    }


    public FacilityStatsResponse(int id, List<MemberStatsResponse>? members, List<MoneyAmount> amounts)
    {
        Id = id;
        Members = members;
        Amounts = amounts;
    }


    public int Id { get; }
    public List<MemberStatsResponse>? Members { get; } = null!;
    public List<MoneyAmount> Amounts { get; }
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