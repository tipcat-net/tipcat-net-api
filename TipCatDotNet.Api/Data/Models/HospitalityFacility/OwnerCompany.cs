using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    [Table("owner_companies")]
    public class OwnerCompany
    {
        public int Id { get; set; }
        
        public int OwnerId { get; set; }
        
        public virtual Owner Owner { get; set; }
        
        public int? AccountId { get; set; }
        
        public virtual Account Account { get; set; }
         
        public ModelStatus Status { get; set; }
    }
}