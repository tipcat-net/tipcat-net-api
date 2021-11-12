using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HappyTravel.Money.Models;

namespace TipCatDotNet.Api.Models.Payments
{
    public class PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(int memberId, MoneyAmount tipsAmount)
        {
            MemberId = memberId;
            TipsAmount = tipsAmount;
        }


        [Required]
        public int MemberId { get; }
        [Required]
        public MoneyAmount TipsAmount { get; }
    }
}