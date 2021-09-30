using System;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct PaymentDetailsResponse
    {
        public PaymentDetailsResponse(int receiverId, string receiverFirstName, string receiverLastName, string? receiverAvatarUrl)
        {
            ReceiverId = receiverId;
            ReceiverFirstName = receiverFirstName;
            ReceiverLastName = receiverLastName;
            ReceiverAvatarUrl = receiverAvatarUrl;
        }


        [Required]
        public int ReceiverId { get; }
        [Required]
        public string ReceiverFirstName { get; }
        [Required]
        public string ReceiverLastName { get; }
        public string? ReceiverAvatarUrl { get; }


        public override bool Equals(object? obj) => obj is PaymentDetailsResponse other && Equals(other);


        public bool Equals(in PaymentDetailsResponse other)
            => (ReceiverId, ReceiverFirstName, ReceiverLastName, ReceiverAvatarUrl) == (other.ReceiverId, other.ReceiverFirstName, other.ReceiverLastName, other.ReceiverAvatarUrl);


        public override int GetHashCode()
            => HashCode.Combine(ReceiverId, ReceiverFirstName, ReceiverLastName, ReceiverAvatarUrl);
    }
}