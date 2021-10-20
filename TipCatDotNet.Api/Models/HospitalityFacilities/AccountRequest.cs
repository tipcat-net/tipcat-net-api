using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountRequest
    {
        [JsonConstructor]
        public AccountRequest(int? id, string address, string? commercialName, string? email, string name, string phone)
        {
            Id = id;
            Address = address;
            CommercialName = commercialName;
            Email = email;
            Name = name;
            Phone = phone;
        }


        public AccountRequest(int? id)
        {
            Id = id;
            Address = string.Empty;
            CommercialName = null;
            Email = null;
            Name = string.Empty;
            Phone = string.Empty;
        }


        public AccountRequest(int? id, AccountRequest request)
        {
            Id = id;
            Address = request.Address;
            CommercialName = request.CommercialName;
            Email = request.Email;
            Name = request.Name;
            Phone = request.Phone;
        }


        public int? Id { get; }
        [Required]
        public string Address { get; }
        public string? CommercialName { get; }
        public string? Email { get; }
        [Required]
        public string Name { get; }
        [Required]
        public string Phone { get; }
    }
}
