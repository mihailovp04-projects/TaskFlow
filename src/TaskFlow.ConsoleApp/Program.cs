using System.Globalization;
using System.Text;
using TaskFlow.Domain.Abstractions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Notifications;
using TaskFlow.Domain.Reports;
using TaskFlow.Domain.Repositories;
using TaskFlow.Domain.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dataFolder = Path.Combine(Environment.CurrentDirectory, "data");
bool useMemory = args.Contains("--memory");

INotifier notifier = new CompositeNotifier(
    new ConsoleNotifier(),
    new FileNotifier(Path.Combine(dataFolder, "notifications.log")));

ITaskRepository repository = useMemory
    ? new InMemoryTaskRepository()
    : new JsonFileTaskRepository(Path.Combine(dataFolder, "tasks.json"));
repository = new LoggingTaskRepository(repository, notifier);

IProjectRepository projectRepository = new InMemoryProjectRepository();

TimeProvider clock = TimeProvider.System;
ITaskService service = new TaskService(repository, notifier, clock);

TaskReport overdueReport = new OverdueReport();
TaskReport priorityReport = new ByPriorityReport();
TaskReport dueSoonReport = new DueSoonReport();

var exporter = new TaskExporter();

Console.WriteLine(useMemory
    ? "TaskFlow, неделя 2. Хранилище: память (данные пропадут после выхода)"
    : $"TaskFlow, неделя 2. Хранилище: {Path.Combine(dataFolder, "tasks.json")}");

if (service.HasOverdue())
{
    Console.WriteLine("Внимание: есть просроченные задачи (пункт 8).");
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Список задач");
    Console.WriteLine("2. Добавить задачу");
    Console.WriteLine("3. Начать задачу");
    Console.WriteLine("4. Завершить задачу");
    Console.WriteLine("5. Вернуть задачу в работу");
    Console.WriteLine("6. Удалить задачу");
    Console.WriteLine("7. Поиск по названию");
    Console.WriteLine("8. Отчёт: просроченные задачи");
    Console.WriteLine("9. Отчёт: по приоритету");
    Console.WriteLine("10. Статистика по статусам");
    Console.WriteLine("11. Отчёт: срок в ближайшие 7 дней");
    Console.WriteLine("12. Фильтр по статусу и приоритету");
    Console.WriteLine("13. Экспорт в JSON");
    Console.WriteLine("14. Создать проект");
    Console.WriteLine("15. Список проектов");
    Console.WriteLine("0. Выход");
    Console.Write("Выберите пункт: ");

    string? choice = Console.ReadLine()?.Trim();
    if (choice is null || choice == "0")
    {
        break;
    }

    try
    {
        switch (choice)
        {
            case "1":
                PrintTasks(service.GetAll());
                break;
            case "2":
                AddTask();
                break;
            case "3":
                service.Start(ReadId());
                break;
            case "4":
                service.Complete(ReadId());
                break;
            case "5":
                service.Reopen(ReadId());
                break;
            case "6":
                int idToDelete = ReadId();
                if (!service.Delete(idToDelete))
                {
                    Console.WriteLine($"Задача #{idToDelete} не найдена.");
                }
                break;
            case "7":
                Console.Write("Текст для поиска: ");
                PrintTasks(service.Search(Console.ReadLine() ?? string.Empty));
                break;
            case "8":
                Console.Write(overdueReport.Render(service.GetAll(), clock.GetLocalNow().DateTime));
                break;
            case "9":
                Console.Write(priorityReport.Render(service.GetAll(), clock.GetLocalNow().DateTime));
                break;
            case "10":
                foreach (KeyValuePair<TaskItemStatus, int> pair in service.CountByStatus())
                {
                    Console.WriteLine($"{pair.Key}: {pair.Value}");
                }
                break;
            case "11":
                Console.Write(dueSoonReport.Render(service.GetAll(), clock.GetLocalNow().DateTime));
                break;
            case "12":
                TaskItemStatus? status = ReadOptionalStatus();
                TaskPriority? priority = ReadOptionalPriority();
                PrintTasks(service.Filter(status, priority));
                break;
            case "13":
                string exportPath = Path.Combine(dataFolder, "export", "tasks-export.json");
                Directory.CreateDirectory(Path.GetDirectoryName(exportPath)!);
                File.WriteAllText(exportPath, exporter.Export(service.GetAll()));
                Console.WriteLine($"Экспорт сохранён: {exportPath}");
                break;
            case "14":
                CreateProject();
                break;
            case "15":
                PrintProjects();
                break;
            default:
                Console.WriteLine("Неизвестный пункт меню.");
                break;
        }
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException or FormatException)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}

Console.WriteLine("До встречи!");

void AddTask()
{
    Console.Write("Название: ");
    string title = Console.ReadLine() ?? string.Empty;

    Console.Write("Приоритет (0 = Low, 1 = Medium, 2 = High, 3 = Critical): ");
    string? priorityText = Console.ReadLine();
    if (!Enum.TryParse(priorityText, out TaskPriority priority) || !Enum.IsDefined(priority))
    {
        throw new FormatException("Приоритет должен быть числом от 0 до 3.");
    }

    Console.Write("Срок (yyyy-MM-dd, пусто = без срока): ");
    string? dueText = Console.ReadLine();
    DateTime? dueDate = null;
    if (!string.IsNullOrWhiteSpace(dueText))
    {
        if (!DateTime.TryParseExact(dueText.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            throw new FormatException("Дата должна быть в формате yyyy-MM-dd.");
        }

        dueDate = parsed;
    }

    int? projectId = ReadOptionalProjectId();

    service.Create(title, priority, dueDate, projectId: projectId);
}

int ReadId()
{
    Console.Write("Id задачи: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    {
        throw new FormatException("Id должен быть целым числом.");
    }

    return id;
}

int? ReadOptionalProjectId()
{
    Console.Write("Id проекта (пусто = без проекта): ");
    string? text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text))
    {
        return null;
    }

    if (!int.TryParse(text.Trim(), out int id))
    {
        throw new FormatException("Id проекта должен быть целым числом.");
    }

    if (projectRepository.GetById(id) is null)
    {
        throw new KeyNotFoundException($"Проект #{id} не найден.");
    }

    return id;
}

TaskItemStatus? ReadOptionalStatus()
{
    Console.Write("Статус (Todo, InProgress, Done, пусто = любой): ");
    string? text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text))
    {
        return null;
    }

    if (!Enum.TryParse(text.Trim(), true, out TaskItemStatus status) || !Enum.IsDefined(status))
    {
        throw new FormatException("Неизвестный статус");
    }

    return status;
}

TaskPriority? ReadOptionalPriority()
{
    Console.Write("Приоритет (Low, Medium, High, Critical), пусто = любой: ");
    string? text = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(text))
    {
        return null;
    }

    if (!Enum.TryParse(text.Trim(), true, out TaskPriority priority) || !Enum.IsDefined(priority))
    {
        throw new FormatException("Неизвестный приоритет");
    }

    return priority;
}

void CreateProject()
{
    Console.Write("Название проекта: ");
    string name = Console.ReadLine() ?? string.Empty;

    Console.Write("Описание (пусто = без описания): ");
    string? description = Console.ReadLine();

    var project = new Project(name, string.IsNullOrWhiteSpace(description) ? null : description.Trim())
    {
        CreatedAt = clock.GetLocalNow().DateTime
    };

    projectRepository.Add(project);
    Console.WriteLine($"Создан проект #{project.Id}: {project.Name}");
}

void PrintProjects()
{
    IReadOnlyList<Project> projects = projectRepository.GetAll();
    if (projects.Count == 0)
    {
        Console.WriteLine("(проектов нет)");
        return;
    }

    foreach (Project project in projects)
    {
        int taskCount = service.GetAll().Count(t => t.ProjectId == project.Id);
        Console.WriteLine($"#{project.Id} {project.Name} (задач: {taskCount})");
    }
}

void PrintTasks(IReadOnlyList<TaskItem> tasks)
{
    if (tasks.Count == 0)
    {
        Console.WriteLine("(задач нет)");
        return;
    }

    foreach (TaskItem task in tasks)
    {
        Console.WriteLine(task);
    }
}