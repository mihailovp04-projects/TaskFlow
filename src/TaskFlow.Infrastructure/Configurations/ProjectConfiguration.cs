using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Color)
    .HasMaxLength(Project.ColorMaxLength);


        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Project.NameMaxLength);

        builder.Property(p => p.Description)
            .HasMaxLength(Project.DescriptionMaxLength);

        // UNIQUE INDEX IX_Projects_Name
        builder.HasIndex(p => p.Name).IsUnique();

        // Анонимные объекты: HasData задаёт значения столбцов и не вызывает конструктор Project
        builder.HasData(
            new { Id = 1, Name = "Inbox", Description = (string?)"Tasks without a category", CreatedAt = SeedConstants.SeedDate },
            new { Id = 2, Name = "TaskFlow", Description = (string?)"Course project", CreatedAt = SeedConstants.SeedDate });
    }
}
