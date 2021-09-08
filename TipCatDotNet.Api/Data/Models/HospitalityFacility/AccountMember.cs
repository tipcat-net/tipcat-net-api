using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models.HospitalityFacility
{
    [Table("account_members")]
    public class AccountMember
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("account_id")]
        public int AccountId { get; set; }
        [Column("member_id")]
        public int MemberId { get; set; }
    }
}
