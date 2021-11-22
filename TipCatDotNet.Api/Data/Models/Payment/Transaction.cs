using System;
using HappyTravel.Money.Enums;

namespace TipCatDotNet.Api.Data.Models.Payment
{
    public class Transaction
    {
        public int Id { get; set; }
        public long Amount { get; set; }
        public Currencies Currency { get; set; }
        public int MemberId { get; set; }
        public string PaymentIntentId { get; set; } = null!;
        public string State { get; set; } = null!;
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}