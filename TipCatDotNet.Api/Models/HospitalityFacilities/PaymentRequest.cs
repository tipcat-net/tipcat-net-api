using System;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class PaymentRequest
    {
        public PaymentRequest(int receiverId, int tipeSize, string cardNumber, DateTime cardCreationDate, DateTime cardExpirationDate, 
            string cardOwner, int cvv)
        {
            ReceiverId = receiverId;
            TipSize = tipeSize;
            CardNumber = cardNumber;
            CardCreationDate = cardCreationDate;
            CardExpirationDate = cardExpirationDate;
            CardOwner = cardOwner;
        }

        [Required]
        public int ReceiverId { get; }
        [Required]
        public int TipSize { get; }
        [Required]
        public string CardNumber { get; }
        [Required]
        public string CardOwner { get; }
        [Required]  
        public DateTime CardCreationDate { get; }
        [Required]
        public DateTime CardExpirationDate { get; }
        [Required]
        public int CVV { get; }
    }
}