namespace TaskFlow.Domain.Abstractions;

public interface INotifier
{
    void Notify(string message);
}
