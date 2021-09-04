using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    [Table("accounts")]
    public class Account
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        public string Name { get; set; } = null!;
        
        [Column("address")]
        public string Address { get; set; } = null!;
        
        [Column("commercial_name")]
        [StringLength(256)]
        public string? CommercialName { get; set; }

        [Column("email")]
        [StringLength(128)]
        public string Email { get; set; } = null!;
        
        [Column("phone")]
        [StringLength(32)]
        public string? Phone { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }
        
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}