using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.Payments;

public readonly struct FacilityTransactionResponse
{
    public FacilityTransactionResponse(int facilityId, string facilityName, decimal totalTips, List<TransactionResponse>? transactions)
    {
        FacilityId = facilityId;
        FacilityName = facilityName;
        TotalTips = totalTips;
        Transactions = transactions;
    }


    public int FacilityId { get; }
    public string FacilityName { get; }
    public decimal TotalTips { get; }
    public List<TransactionResponse>? Transactions { get; }
}