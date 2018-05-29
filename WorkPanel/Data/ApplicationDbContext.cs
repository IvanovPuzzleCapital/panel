using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Models;

namespace WorkPanel.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Investor> Investors { get; set; }

        public DbSet<Asset> Assets { get; set; }

        public DbSet<AssetHistory> AssetHistories { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        public DbSet<NavHistory> NavHistories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Currency>()
                .Property(b => b.Date)
                .HasDefaultValueSql("getdate()");
        }
    }
}
