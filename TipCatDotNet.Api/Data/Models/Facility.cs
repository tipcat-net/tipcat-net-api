using System.ComponentModel.DataAnnotations.Schema;

namespace TipCatDotNet.Api.Data.Models
{
    [Table("facilities")]
    public class Facility
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
    }
}