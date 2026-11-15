namespace TaskFlow.Infrastructure.Configurations;

internal static class SeedConstants
{
    // Только константы: значение HasData попадает в код миграции
    public static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
