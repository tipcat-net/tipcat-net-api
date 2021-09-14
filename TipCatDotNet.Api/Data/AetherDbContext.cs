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


        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Member> Members { get; set; } = null!;
        public virtual DbSet<AccountMember> AccountMembers { get; set; } = null!;
        public virtual DbSet<Facility> Facilities { get; set; } = null!;
    }
}
