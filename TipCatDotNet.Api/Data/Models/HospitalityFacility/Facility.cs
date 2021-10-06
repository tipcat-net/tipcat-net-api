using System;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    public class Facility
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public int? AccountId { get; set; }

        public ModelStates State { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }
    }
}