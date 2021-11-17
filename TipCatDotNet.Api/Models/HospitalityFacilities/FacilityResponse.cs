using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class FacilityResponse
    {
        public FacilityResponse(int id, string name, string address, int accountId, List<MemberResponse>? members)
        {
            Id = id;
            Name = name;
            Address = address;
            AccountId = accountId;
            Members = members ?? new List<MemberResponse>();
        }

        public int Id { get; }
        public string Name { get; }
        public string Address { get; }
        public int AccountId { get; }
        public List<MemberResponse> Members { get; }
    }
}