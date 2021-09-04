using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TipCatDotNet.Api.Models.HospitalityFacilities.Enums;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("email")]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Column("password_hash")]
        [StringLength(200)]
        public string PasswordHash { get; set; }
        
        [Column("salt")]
        [StringLength(250)]
        public string Salt { get; set; }
        
        [Column("permission")]
        public HospitalityFacilityPermissions Permission { get; set; }
        
        [Column("status")]
        public ModelStatus Status { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }
        
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}