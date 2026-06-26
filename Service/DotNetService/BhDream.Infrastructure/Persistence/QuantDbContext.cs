using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
        public DbSet<OptionHistoryRfrSync> OptionHistoryRfrSync => Set<OptionHistoryRfrSync>();
        public DbSet<MlModel> MlModels => Set<MlModel>();

        // Inside QuantDbContext.cs
        public static TimeZoneInfo IstZone { get; } = OperatingSystem.IsWindows()
            ? TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
            : TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🧠 Automatically loads all IEntityTypeConfiguration<T>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuantDbContext).Assembly);

            // 🇮🇳 IST <-> UTC Database Converter
            // Finds the Indian Standard Time zone info regardless of whether you are hosting on Windows or Linux/Docker
            TimeZoneInfo istZone = OperatingSystem.IsWindows()
                ? TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
                : TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeProperties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                foreach (var property in dateTimeProperties)
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        // 1. When saving to DB: Assume the incoming DateTime is IST, convert it to UTC for Postgres
                        convertToProviderExpression: v => ConvertIstToUtc(v, istZone),

                        // 2. When reading from DB: Convert Postgres' UTC time back into IST for your application
                        convertFromProviderExpression: v => ConvertUtcToIst(v, istZone)
                    ));
                }
            }
        }

        private static DateTime ConvertIstToUtc(DateTime istTime, TimeZoneInfo istZone)
        {
            // If it's already specified as UTC, leave it (or handle if necessary)
            if (istTime.Kind == DateTimeKind.Utc) return istTime;

            // Treat unspecified/local as IST, and convert to UTC
            var unspecifiedIst = DateTime.SpecifyKind(istTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecifiedIst, istZone);
        }

        private static DateTime ConvertUtcToIst(DateTime utcTime, TimeZoneInfo istZone)
        {
            var utcSpecified = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);
            // Convert UTC from database back to Indian Standard Time
            return TimeZoneInfo.ConvertTimeFromUtc(utcSpecified, istZone);
        }
    }
}