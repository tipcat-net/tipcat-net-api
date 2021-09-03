using Microsoft.EntityFrameworkCore;
using TipCatDotNet.Api.Data.Models;

namespace TipCatDotNet.Api.Data
{
    public class AetherDbContext : DbContext
    {
        public AetherDbContext(DbContextOptions<AetherDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        { }

        public virtual DbSet<Owner> Owners { get; set; }
        
        public virtual DbSet<OwnerCompany> AccountCompanies { get; set; }

        public virtual DbSet<Company> Companies { get; set; } 
        
        public virtual DbSet<Employee> Employees { get; set; }
        
        public virtual DbSet<Facility> Facilities { get; set; }

        public virtual DbSet<User> Users { get; set; }
        
    }
}
