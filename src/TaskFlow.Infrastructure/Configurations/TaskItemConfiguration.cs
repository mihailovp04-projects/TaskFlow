using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Класс называется TaskItem (конфликт имён с System.Threading.Tasks.Task), таблица - Tasks
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(TaskItem.TitleMaxLength);

        builder.Property(t => t.Description)
            .HasMaxLength(TaskItem.DescriptionMaxLength);

        // Enum хранится строкой: 'Low', 'Medium', 'High', 'Critical'
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Индексы под частые фильтры списка задач
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DueDate);

        // Один-ко-многим: при удалении проекта задачи остаются без проекта
        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Многие-ко-многим через таблицу TaskItemTags (TaskItemId, TagId)
        builder.HasMany(t => t.Tags)
            .WithMany(tag => tag.Tasks)
            .UsingEntity<Dictionary<string, object>>(
                "TaskItemTags",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                left => left.HasOne<TaskItem>().WithMany().HasForeignKey("TaskItemId"),
                join => join.HasKey("TaskItemId", "TagId"));
    }
}
