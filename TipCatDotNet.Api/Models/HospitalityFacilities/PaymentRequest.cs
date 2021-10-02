using System;
using System.Text.Json.Serialization;
using HappyTravel.Money.Models;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
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