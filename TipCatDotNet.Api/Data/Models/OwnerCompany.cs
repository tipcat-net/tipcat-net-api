using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models
{
    [Table("account_companies")]
    public class OwnerCompany
    {
        public int Id { get; set; }
        
        public int AccountId { get; set; }
        
        public virtual Owner Account { get; set; }
        
        public int? CompanyId { get; set; }
        
        public virtual Company Company { get; set; }
        
        public bool IsActive { get; set; }
    }
}