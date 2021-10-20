using System.Collections.Generic;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class SlimFacilityResponse
    {
        public SlimFacilityResponse(int id, string name, List<MemberResponse> members)
        {
            Id = id;
            Name = name;
            Members = members;
        }


        public int Id { get; }

        public string Name { get; }

        public List<MemberResponse> Members { get; }
    }
}