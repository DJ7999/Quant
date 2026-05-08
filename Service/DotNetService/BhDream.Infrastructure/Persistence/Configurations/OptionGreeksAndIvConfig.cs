using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionGreeksAndIvConfig : IEntityTypeConfiguration<OptionGreeksAndIv>
    {
        public void Configure(EntityTypeBuilder<OptionGreeksAndIv> builder)
        {
            builder.HasKey(x => new { x.ContractId, x.OptionHistoryId, x.RfrMarket, x.RfrTenor });
            builder.HasOne(x=>x.OptionHistory)
                .WithMany()
                .HasForeignKey(x=>x.OptionHistoryId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x=>x.Contract)
                .WithMany()
                .HasForeignKey(x=>x.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.RfrMarket).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RfrTenor).HasMaxLength(20).IsRequired();

            builder.HasIndex(x => new { x.OptionHistoryId, x.ContractId });
        }
    }
}
