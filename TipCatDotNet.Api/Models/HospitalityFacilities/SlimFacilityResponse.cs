using System.Collections.Generic;
using TipCatDotNet.Api.Models.HospitalityFacilities;

namespace TipCatDotNet.Api.Models.HospitalityFacilities
{
    public class SlimFacilityResponse
    {
        public SlimFacilityResponse(int id, string name)
        {
            Id = id;
            Name = name;
        }


        public SlimFacilityResponse(int id, string name, IEnumerable<MemberResponse> members)
        {
            Id = id;
            Name = name;
            Members = members;
        }

        public int Id { get; }
        public string Name { get; }
        public IEnumerable<MemberResponse> Members { get; } = new List<MemberResponse>();
    }
}