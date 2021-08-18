using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Api.Data
{
    public class ServiceProviderDbContext : DbContext
    {
        public ServiceProviderDbContext(DbContextOptions<ServiceProviderDbContext> options) : base(options)
        { }


        //public virtual DbSet<Company> Companies { get; set; } = null!;
    }
}
