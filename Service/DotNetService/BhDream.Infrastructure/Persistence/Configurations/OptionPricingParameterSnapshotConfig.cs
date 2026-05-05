using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionPricingParameterSnapshotConfig : IEntityTypeConfiguration<OptionPricingParameterSnapshot>
    {
        public void Configure(EntityTypeBuilder<OptionPricingParameterSnapshot> builder)
        {
            builder.HasNoKey();
            builder.ToView("View_OptionPricingParameterSnapshots");

            builder.Property(x => x.UnderlyingValue).HasPrecision(18, 2);
            builder.Property(x => x.StrikePrice).HasPrecision(18, 2);
            builder.Property(x => x.Close).HasPrecision(18, 2);
            builder.Property(x => x.RiskFreeRateValue).HasPrecision(18, 4);
        }
    }
}
