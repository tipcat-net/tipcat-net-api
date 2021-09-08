using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountRequest
    {
        public AccountRequest(string address, string? commercialName, string? email, string name, string phone)
        {
            Address = address;
            CommercialName = commercialName;
            Email = email;
            Name = name;
            Phone = phone;
        }


        [Required]
        public string Address { get; }
        public string? CommercialName { get; }
        public string? Email { get; }
        [Required]
        public string Name { get; }
        public string Phone { get; }
    }
}
