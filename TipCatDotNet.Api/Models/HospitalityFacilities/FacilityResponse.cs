using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class FacilityResponse
    {
        public FacilityResponse(int id, string name, int accountId, List<MemberResponse>? members)
        {
            Id = id;
            Name = name;
            AccountId = accountId;
            Members = members;
        }

        public int Id { get; }
        public string Name { get; }
        public int AccountId { get; }
        public List<MemberResponse>? Members { get; }
    }
}