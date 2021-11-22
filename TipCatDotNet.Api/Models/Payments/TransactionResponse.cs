using System;
using HappyTravel.Money.Enums;

namespace TipCatDotNet.Api.Models.Payments
{
    public class TransactionResponse
    {
        public TransactionResponse(long amount, Currencies currency, int memberId, string state, DateTime created)
        {
            Amount = amount;
            Currency = currency;
            MemberId = memberId;
            State = state;
            Created = created;
        }


        public long Amount { get; }
        public Currencies Currency { get; }
        public int MemberId { get; }
        public string State { get; }
        public DateTime Created { get; }
    }
}