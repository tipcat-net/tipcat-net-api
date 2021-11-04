using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public readonly struct AccountResponse
    {
        public AccountResponse(int id, string name, string operatingName, string address, string email, 
            string phone, bool isActive, List<FacilityResponse>? facilities)
        {
            Id = id;
            Address = address;
            Email = email;
            IsActive = isActive;
            Name = name;
            OperatingName = operatingName;
            Phone = phone;
            Facilities = facilities;
        }


        public int Id { get; }
        public string Address { get; }
        public string Email { get; }
        public bool IsActive { get; }
        public string Name { get; }
        public string OperatingName { get; }
        public string Phone { get; }
        public List<FacilityResponse>? Facilities { get; }
    }
}
