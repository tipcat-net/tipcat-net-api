using System;
using System.ComponentModel.DataAnnotations;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    public class Account
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = null!;
        
        public string Address { get; set; } = null!;

        [StringLength(256)]
        public string OperatingName { get; set; } = null!;

        [StringLength(128)]
        public string Email { get; set; } = null!;
        
        [StringLength(32)]
        public string Phone { get; set; } = null!;
        
        public ModelStates State { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime Modified { get; set; }
    }
}