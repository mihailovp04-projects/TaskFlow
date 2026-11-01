using TaskFlow.Domain.Abstractions;

namespace TaskFlow.Domain.Notifications;

public sealed class FileNotifier : INotifier
{
    private readonly string _filePath;

    public FileNotifier(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Путь к файлу обязателен.", nameof(filePath));
        }

        _filePath = filePath;
    }

    public void Notify(string message)
    {
        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(_filePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
    }
}
