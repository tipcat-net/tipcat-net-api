using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models
{
    [Table("companies")]
    public class Company
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        [StringLength(250)]
        public string Name { get; set; }
        
        [Column("address")]
        public string Address { get; set; }
        
        [Column("commercial_name")]
        [StringLength(250)]
        public string CommercialName { get; set; }
        
        [Column("email")]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Column("phone")]
        [StringLength(20)]
        public string Phone { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }
        
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}