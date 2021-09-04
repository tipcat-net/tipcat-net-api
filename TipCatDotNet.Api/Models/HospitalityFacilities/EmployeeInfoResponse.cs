using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class EmployeeInfoResponse
    {
        public EmployeeInfoResponse(string name, string lastName, string email, HospitalityFacilityPermissions permission)
        {
            Name = name;
            LastName = lastName;
            Email = email;
            Permission = permission;
        }
        
        public string Name { get; init; }
        
        public string LastName { get; init; }
        
        public string Email { get; init; }
        
        public HospitalityFacilityPermissions Permission { get; init; }
    }
}