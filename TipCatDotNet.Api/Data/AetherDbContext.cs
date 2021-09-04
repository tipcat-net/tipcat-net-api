using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;

namespace TipCatDotNet.Api.Data
{
    public class AetherDbContext : DbContext
    {
        public AetherDbContext(DbContextOptions<AetherDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        { }

        public virtual DbSet<Account> Accounts { get; set; } 
        
        public virtual DbSet<Member> Members { get; set; }
        
        public virtual DbSet<Facility> Facilities { get; set; }
        
    }
}
