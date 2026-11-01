using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Notifications;

public sealed class ConsoleNotifier : INotifier
{
    public void Notify(string message) => Console.WriteLine($"[уведомление] {message}");
}
