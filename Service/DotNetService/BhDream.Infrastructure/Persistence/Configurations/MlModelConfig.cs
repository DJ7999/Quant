using BhDream.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;

namespace BhDream.Infrastructure.Persistence.Configurations
{
    public class MlModelConfig : IEntityTypeConfiguration<MlModel>
    {
        public void Configure(EntityTypeBuilder<MlModel> builder)
        {
            // Explicitly set the table name in PostgreSQL (using clean snake_case convention)
            builder.ToTable("ml_models");

            // Configure ModelName with a defined size limit for index optimization
            builder.Property(e => e.ModelName)
                .IsRequired()
                .HasMaxLength(150);

            // Define the Primary Key
            builder.HasKey(e => e.Id);

            // Configure standard tracking columns
            builder.Property(e => e.StartDateTime)
                .IsRequired();

            builder.Property(e => e.EndDateTime)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnType("integer")
                .IsRequired();

            // Tracks the crash dump logs, exceptions, or error messages if training aborts
            builder.Property(e => e.FailureReason)
                .HasColumnType("text") // Using "text" to accommodate full C# stack traces without truncation
                .IsRequired(false);

            
            builder.Property(e => e.LastUpdatedAt)
                .IsRequired();

            // Optional string identifier configuration
            builder.Property(e => e.ModelReference)
                .HasMaxLength(255);

            // Enforce Native PostgreSQL JSONB data type for string properties
            builder.Property(e => e.Features)
                .HasColumnType("jsonb");

            builder.Property(e => e.Parameters)
                .HasColumnType("jsonb");

            builder.Property(e => e.ModelMetrics)
                .HasColumnType("jsonb");

            // Optimizes Watcher lookups filtering by name, status tracking, and running windows
            builder.HasIndex(e => new { e.ModelName, e.Status, e.StartDateTime, e.EndDateTime });
        }
    }
}