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
            // Update the Primary Key to match the ordering logic of your FK
            builder.HasKey(x => new { x.OptionHistoryId, x.RfrTenor, x.RfrMarket });

            // Correct Relationship: Maps (Date, RfrTenor, RfrMarket)
            builder.HasOne(x => x.RiskFreeRate)
                .WithMany()
                .HasForeignKey(x => new { x.Date, x.RfrTenor, x.RfrMarket })
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship for OptionHistory
            builder.HasOne(x => x.OptionHistory)
                .WithMany()
                .HasForeignKey(x => x.OptionHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
