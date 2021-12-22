using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.Payments;

public class FacilityTransactionResponse
{
    public FacilityTransactionResponse(int facilityId, string facilityName, decimal totalTips, List<TransactionResponse>? transactions)
    {
        FacilityId = facilityId;
        FacilityName = facilityName;
        TotalTips = totalTips;
        Transactions = transactions;
    }


    public int FacilityId;
    public string FacilityName = null!;
    public decimal TotalTips;
    public List<TransactionResponse>? Transactions;
}