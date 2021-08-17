using Microsoft.EntityFrameworkCore;

namespace TipCatDotNet.Data
{
    public class CustomerDbContext : DbContext
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
        { }


        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    Email = "sophie.hall@example.com",
                    Name = "Sophie Hall"
                },
                new Customer
                {
                    Id = 2,
                    Email = "leona.carter@example.com",
                    Name = "Leona Carter"
                },
                new Customer
                {
                    Id = 3,
                    Email = "leroy.robertson@example.com",
                    Name = "Leroy Robertson"
                }
            );

            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = 1,
                    Name = "Zoe"
                },
                new Company
                {
                    Id = 2,
                    Name = "Медные трубы"
                }
            );
        }
    }
}
