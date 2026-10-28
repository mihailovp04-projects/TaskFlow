using System.Globalization;
using TaskFlow.Domain.Enums;

namespace TaskFlow.ConsoleApp;

public static class ConsoleInput
{
    public static string ReadText(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (int.TryParse(input, out var value))
            {
                return value;
            }

            Console.WriteLine("Please enter a whole number.");
        }
    }

    public static TaskPriority ReadPriority()
    {
        while (true)
        {
            Console.Write("Priority (Low, Medium, High, Critical) [Medium]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return TaskPriority.Medium;
            }

            if (Enum.TryParse<TaskPriority>(input.Trim(), ignoreCase: true, out var priority) && Enum.IsDefined(priority))
            {
                return priority;
            }

            Console.WriteLine("Unknown priority. Try again.");
        }
    }

    public static TaskItemStatus ReadStatus()
    {
        while (true)
        {
            Console.Write("Status (Todo, InProgress, Done) [Todo]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return TaskItemStatus.Todo;
            }

            if (Enum.TryParse<TaskItemStatus>(input.Trim(), ignoreCase: true, out var status) && Enum.IsDefined(status))
            {
                return status;
            }

            Console.WriteLine("Unknown status. Try again.");
        }
    }

    public static TaskPriority? ReadOptionalPriority()
    {
        while (true)
        {
            Console.Write("Priority (Low, Medium, High, Critical) (empty = keep): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (Enum.TryParse<TaskPriority>(input.Trim(), ignoreCase: true, out var priority) && Enum.IsDefined(priority))
            {
                return priority;
            }

            Console.WriteLine("Unknown priority. Try again.");
        }
    }

    public static DateTime? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            Console.WriteLine("Use the format yyyy-MM-dd, for example 2026-11-30.");
        }
    }
}
