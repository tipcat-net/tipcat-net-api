using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models
{
    [Table("accounts")]
    public class Owner
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("user_id")]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [Column("name")]
        [StringLength(200)]
        public string Name { get; set; }

        [Column("last_name")]
        [StringLength(200)]
        public string LastName { get; set; }
        
        [Column("email")]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Column("created")]
        public DateTime Created { get; set; }
        
        [Column("modified")]
        public DateTime Modified { get; set; }
    }
}