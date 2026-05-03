using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class RiskFreeRateConfig : IEntityTypeConfiguration<RiskFreeRate>
    {
        public void Configure(EntityTypeBuilder<RiskFreeRate> builder)
        {
            builder.HasKey(x => new { x.Date, x.Tenor });
            builder.ToTable("RiskFreeRates");
            builder.Property(x => x.Rate).HasPrecision(18, 4).IsRequired();
            builder.Property(x => x.Tenor).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Date).IsRequired();
            
            builder.HasIndex(r => r.Date);
        }
    }
}
