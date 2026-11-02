using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Reports;

namespace TaskFlow.Domain.Tests;

public class ReportTests
{
    private static readonly DateTime Today = new(2026, 10, 5);

    [Fact]
    public void DueSoonReport_IncludesTaskInThreeDays_ExcludesTaskInTenDays()
    {
        // Arrange
        var soon = new TaskItem("Скоро", TaskPriority.Medium, Today.AddDays(3));
        var later = new TaskItem("Нескоро", TaskPriority.Medium, Today.AddDays(10));
        var report = new DueSoonReport();

        // Act
        string text = report.Render(new[] { soon, later }, Today);

        // Assert
        Assert.Contains("Скоро", text);
        Assert.DoesNotContain("Нескоро", text);
    }
}