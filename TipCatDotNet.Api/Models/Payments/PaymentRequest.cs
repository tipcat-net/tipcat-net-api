using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Models.Payments
{
    public class PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(int memberId, string? message, MoneyAmount tipsAmount)
        {
            MemberId = memberId;
            Message = message;
            TipsAmount = tipsAmount;
        }


        [Required]
        public int MemberId { get; }
        public string? Message { get; }
        [Required]
        public MoneyAmount TipsAmount { get; }
    }
}