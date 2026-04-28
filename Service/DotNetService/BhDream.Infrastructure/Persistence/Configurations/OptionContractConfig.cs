using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionContractConfig : IEntityTypeConfiguration<OptionContract>
    {
        public void Configure(EntityTypeBuilder<OptionContract> eb)
        {
            eb.HasKey(x => x.Id);

            eb.HasOne(x => x.Underlying)
              .WithMany(u => u.Contracts)
              .HasForeignKey(x => x.UnderlyingId);

            eb.HasIndex(x => new
            {
                x.UnderlyingId,
                x.Expiry,
                x.StrikePrice,
                x.OptionType
            }).IsUnique();

            eb.Property(x => x.StrikePrice).HasPrecision(18, 2);
        }
    }
}
