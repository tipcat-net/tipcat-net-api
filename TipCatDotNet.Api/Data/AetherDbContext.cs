using System;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TipCatDotNet.Api.Data.Models.Auth;
using TipCatDotNet.Api.Data.Models.HospitalityFacility;
using TipCatDotNet.Api.Data.Models.Payment;
using TipCatDotNet.Api.Data.Models.Stripe;
using TipCatDotNet.Api.Infrastructure.Converters;

namespace TipCatDotNet.Api.Data
{
    public class AetherDbContext : DbContext
    {
        public AetherDbContext(DbContextOptions<AetherDbContext> options) : base(options)
        { }


        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>()
                .HaveConversion<DateTimeKindConverter>();
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Transaction>()
                .HasIndex(t => t.Created)
                .HasFilter(null)
                .HasSortOrder(SortOrder.Descending);
        }


        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Member> Members { get; set; } = null!;
        public virtual DbSet<Facility> Facilities { get; set; } = null!;
        public virtual DbSet<MemberInvitation> MemberInvitations { get; set; } = null!;
        public virtual DbSet<StripeAccount> StripeAccounts { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
    }
}
