using System;
using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Models.Payments
{
    public class TransactionResponse
    {
        public TransactionResponse(MoneyAmount amount, int memberId, string message, string state, DateTime created)
        {
            Amount = amount;
            MemberId = memberId;
            Message = message;
            State = state;
            Created = created;
        }


        public MoneyAmount Amount { get; }
        public int MemberId { get; }
        public string Message { get; }
        public string State { get; }
        public DateTime Created { get; }
    }
}