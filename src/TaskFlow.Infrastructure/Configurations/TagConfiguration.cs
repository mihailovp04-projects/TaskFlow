using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(Tag.NameMaxLength);

        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasData(
            new { Id = 1, Name = "backend", CreatedAt = SeedConstants.SeedDate },
            new { Id = 2, Name = "frontend", CreatedAt = SeedConstants.SeedDate },
            new { Id = 3, Name = "urgent", CreatedAt = SeedConstants.SeedDate });
    }
}
