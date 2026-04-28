using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class OptionHistoryConfig : IEntityTypeConfiguration<OptionHistory>
    {
        public void Configure(EntityTypeBuilder<OptionHistory> eb)
        {
            eb.HasKey(x => x.Id);

            eb.HasOne(x => x.Contract)
              .WithMany(c => c.Histories)
              .HasForeignKey(x => x.ContractId);

            eb.HasIndex(x => new { x.ContractId, x.Date }).IsUnique();

            eb.Property(x => x.Open).HasPrecision(18, 2);
            eb.Property(x => x.High).HasPrecision(18, 2);
            eb.Property(x => x.Low).HasPrecision(18, 2);
            eb.Property(x => x.Close).HasPrecision(18, 2);

            eb.Property(x => x.UnderlyingValue).HasPrecision(18, 2);
        }
    }
}
