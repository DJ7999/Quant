using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class UnderlyingConfig : IEntityTypeConfiguration<Underlying>
    {
        public void Configure(EntityTypeBuilder<Underlying> eb)
        {
            eb.HasKey(x => x.Id);

            eb.Property(x => x.Symbol)
                .IsRequired()
                .HasMaxLength(20);

            eb.HasIndex(x => x.Symbol).IsUnique();
        }
    }
}
