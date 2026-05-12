using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionHistoryRfrSyncConfig : IEntityTypeConfiguration<OptionHistoryRfrSync>
    {
        public void Configure(EntityTypeBuilder<OptionHistoryRfrSync> builder)
        {
            builder.HasKey(x => new { x.OptionHistoryId, x.RfrMarket, x.RfrTenor });
            builder.HasOne(x => x.OptionHistory)
                .WithMany()
                .HasForeignKey(x => x.OptionHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.RiskFreeRate)
                .WithMany()
                .HasForeignKey(x => new { x.Date, x.RfrMarket, x.RfrTenor })
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
