using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BhDream.Infrastructure.Persistence
{
    public class QuantDbContext(DbContextOptions<QuantDbContext> options) : DbContext(options)
    {
        public DbSet<Underlying> Underlyings => Set<Underlying>();
        public DbSet<OptionContract> OptionContracts => Set<OptionContract>();
        public DbSet<OptionHistory> OptionHistories => Set<OptionHistory>();
        public DbSet<RiskFreeRate> RiskFreeRate => Set<RiskFreeRate>();
        public DbSet<OptionPricingParameterSnapshot> OptionPricingParameterSnapshots => Set<OptionPricingParameterSnapshot>();

        public DbSet<OptionGreeksAndIv> OptionGreeksAndIvs => Set<OptionGreeksAndIv>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🧠 Automatically loads all IEntityTypeConfiguration<T>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuantDbContext).Assembly);
        }
    }
}
