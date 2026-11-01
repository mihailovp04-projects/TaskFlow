using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Tests.Fakes;

public sealed class RecordingNotifier : INotifier
{
    public List<string> Messages { get; } = new();

    public void Notify(string message) => Messages.Add(message);
}
