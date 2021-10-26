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


        public AccountRequest(int? id, in AccountRequest request) : this(id, request.Address, request.CommercialName, request.Email, request.Name, request.Phone)
        { }


        public int? Id { get; }
        [Required]
        public string Address { get; }
        public string? CommercialName { get; }
        public string? Email { get; }
        [Required]
        public string Name { get; }
        [Required]
        public string Phone { get; }


        public static AccountRequest CreateEmpty(int? id) => new(id, string.Empty, null, null, string.Empty, string.Empty);
    }
}
