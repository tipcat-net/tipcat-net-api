using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models
{
    [Table("employees")]
    public class Employee
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
        
        [Column("photo")]
        public string Photo { get; set; }
        
        [Column("alpha_digital_code")]
        [StringLength(50)]
        public string AlphaDigitalCode { get; set; }

        [Column("qr_code")]
        public string QrCode { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
                
        [Column("modified")]
        public DateTime Modified { get; set; }
        
        [Column("company_id")]
        public int CompanyId { get; set; }
        
        [ForeignKey("CompanyId")]
        public Account Account { get; set; }
    }
}