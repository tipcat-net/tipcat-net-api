using System;
using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Models.Payments;

public class TransactionResponse
{
    public TransactionResponse(MoneyAmount amount, int memberId, int facilityId, string message, string state, DateTime created)
    {
        Amount = amount;
        Created = created;
        MemberId = memberId;
        FacilityId = facilityId;
        Message = message;
        State = state;
    }


    public MoneyAmount Amount { get; }
    public DateTime Created { get; }
    public int MemberId { get; }
    public int FacilityId { get; }
    public string Message { get; }
    public string State { get; }
}