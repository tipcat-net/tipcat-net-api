using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Data
{
    public class AetherDbContext : DbContext
    {
        public AetherDbContext(DbContextOptions<AetherDbContext> options) : base(options)
        { }


        //public virtual DbSet<Company> Companies { get; set; } = null!;
    }
}
