using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class PaymentRequest
    {
        public PaymentRequest(string memberCode)
        {
            MemberCode = memberCode;
        }

        [Required]
        public string MemberCode { get; }
    }
}