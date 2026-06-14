using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionGreeksAndIvConfig : IEntityTypeConfiguration<OptionGreeksAndIv>
    {
        public void Configure(EntityTypeBuilder<OptionGreeksAndIv> builder)
        {
            builder.HasKey(x => new { x.ContractId, x.OptionHistoryId, x.RfrMarket, x.RfrTenor });

            builder.HasOne(x => x.OptionHistory)
                .WithMany()
                .HasForeignKey(x => x.OptionHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Contract)
                .WithMany()
                .HasForeignKey(x => x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.RfrMarket).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RfrTenor).HasMaxLength(20).IsRequired();

            // 🧠 Instead of HasColumnType("decimal(18,6)"), we use standard precision configuration.
            // If these fields are C# 'double' or 'decimal' types, EF Core translates this perfectly for Postgres.
            builder.Property(x => x.Delta).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.Theta).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.Gamma).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.Vega).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.Rho).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.Vomma).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.ImpliedVolatility).HasPrecision(18, 6).IsRequired();

            builder.Property(x => x.BenchMarkDelta).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.BenchMarkTheta).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.BenchMarkGamma).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.BenchMarkVega).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.BenchMarkRho).HasPrecision(18, 6).IsRequired();
            builder.Property(x => x.BenchMarkVomma).HasPrecision(18, 6);
            builder.Property(x => x.BenchMarkImpliedVolatility).HasPrecision(18, 6).IsRequired();

            // 🚀 Dropping HasColumnType("datetime2") lets EF Core auto-target PostgreSQL's native timestamp!
            builder.Property(x => x.CalculatedAt).IsRequired();

            builder.HasIndex(x => new { x.OptionHistoryId, x.ContractId });
        }
    }
}