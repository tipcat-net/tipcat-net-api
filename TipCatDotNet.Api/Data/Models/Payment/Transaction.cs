using System;
using HappyTravel.Money.Enums;

namespace TipCatDotNet.Api.Data.Models.Payment
{
    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public int MemberId { get; set; }
        public string PaymentIntentId { get; set; } = null!;
        public string State { get; set; } = null!;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}