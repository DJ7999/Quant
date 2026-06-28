using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionPricingParameterSnapshotConfig : IEntityTypeConfiguration<OptionPricingParameterSnapshot>
    {
        public void Configure(EntityTypeBuilder<OptionPricingParameterSnapshot> builder)
        {
            builder.HasNoKey();

            // 1. Points to your lowercase database view
            builder.ToView("view_optionpricingparametersnapshots");

            // 2. Map properties to the exact casing Postgres used for unaliased/aliased columns
            // Unquoted aliases in your SQL migrated as completely lowercase:
            builder.Property(x => x.OptionContractId).HasColumnName("optioncontractid");
            builder.Property(x => x.OptionHistoryId).HasColumnName("optionhistoryid");
            builder.Property(x => x.RfrMarket).HasColumnName("rfrmarket");
            builder.Property(x => x.RfrTenor).HasColumnName("rfrtenor");
            builder.Property(x => x.RfrTenorDays).HasColumnName("rfrtenordays");
            builder.Property(x => x.RiskFreeRateValue).HasColumnName("riskfreeratevalue").HasPrecision(18, 4);

            // Columns pulled directly from double-quoted source tables kept their PascalCase:
            builder.Property(x => x.UnderlyingValue).HasColumnName("UnderlyingValue").HasPrecision(18, 2);
            builder.Property(x => x.StrikePrice).HasColumnName("StrikePrice").HasPrecision(18, 2);
            builder.Property(x => x.Close).HasColumnName("Close").HasPrecision(18, 2);
            builder.Property(x => x.Date).HasColumnName("Date");
            builder.Property(x => x.Expiry).HasColumnName("Expiry");
            builder.Property(x => x.OptionType).HasColumnName("OptionType");
        }
    }
}