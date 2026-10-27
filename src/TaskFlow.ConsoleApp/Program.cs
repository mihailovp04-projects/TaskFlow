using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

var tasks = new List<TaskItem>();
var nextId = 1;

AddTask(new TaskItem("Install .NET 10 SDK", TaskPriority.High, DateTime.Today.AddDays(-2)));
AddTask(new TaskItem("Read about value and reference types", TaskPriority.Medium, DateTime.Today.AddDays(3)));
AddTask(new TaskItem("Push the first commit", TaskPriority.Low));

Console.WriteLine("TaskFlow console, week 01");

while (true)
{
    PrintMenu();
    var choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1":
                AddFromInput();
                break;
            case "2":
                PrintTasks(tasks, "All tasks");
                break;
            case "3":
                StartTask();
                break;
            case "4":
                CompleteTask();
                break;
            case "5":
                PrintTasks(FindOverdue(DateTime.Now), "Overdue tasks");
                break;
            case "6":
                ReopenTask();
                break;
            case "7":
                ShowTaskDetails();
                break;
            case "8":
                EditTask();
                break;
            case "0":
            case null:
                Console.WriteLine("Bye!");
                return;
            default:
                Console.WriteLine($"Unknown option '{choice}'.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Not allowed: {ex.Message}");
    }
}

void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("1) Add task");
    Console.WriteLine("2) List tasks");
    Console.WriteLine("3) Start task");
    Console.WriteLine("4) Complete task");
    Console.WriteLine("5) Show overdue tasks");
    Console.WriteLine("6) Reopen task");
    Console.WriteLine("7) Show task details");
    Console.WriteLine("8) Edit task");
    Console.WriteLine("0) Exit");
    Console.Write("Choose an option: ");
}

void AddTask(TaskItem task)
{
    task.Id = nextId;
    nextId++;
    tasks.Add(task);
}

void AddFromInput()
{
    var title = ConsoleInput.ReadText("Title: ");
    var priority = ConsoleInput.ReadPriority();
    var dueDate = ConsoleInput.ReadOptionalDate("Due date (yyyy-MM-dd, empty = none): ");
    var description = ConsoleInput.ReadText("Description: ");

    var task = new TaskItem(title, priority, dueDate, description);
    AddTask(task);
    Console.WriteLine($"Added: {task}");
}

void StartTask()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }

    task.Start();
    Console.WriteLine($"Started: {task}");
}

void ReopenTask()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }

    task.Reopen();
    Console.WriteLine($"Reopened: {task}");
}
void EditTask()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }
    var title = ConsoleInput.ReadText("New title (empty = keep) : ");
    if (!string.IsNullOrWhiteSpace(title))
    {
        task.Title = title;
    }
    var description = ConsoleInput.ReadText("New description (empty = keep) : ");
    if (!string.IsNullOrWhiteSpace(description))
    {
        task.Description = description;
    }
    var priority = ConsoleInput.ReadOptionalPriority();
    if (priority != null)
    {
        task.Priority = priority.Value;
    }
    var dueDate = ConsoleInput.ReadOptionalDate("New due date (empty = keep) : ");
    if(dueDate !=null)
    {
        task.DueDate = dueDate;
    }

}

void ShowTaskDetails()
{
    var task = AskForTask();
    if (task is null) {
        return;
    }
    Console.WriteLine($"Id : {task.Id}");
    Console.WriteLine($"Title: {task.Title}");
    Console.WriteLine($"Description: {task.Description ?? "-"}");
    Console.WriteLine($"Priority: {task.Priority}");
    Console.WriteLine($"Status: {task.Status}");
    Console.WriteLine($"Due date: {task.DueDate?.ToString("yyyy-MM-dd") ?? "-"}");
    Console.WriteLine($"Created At : {task.CreatedAt.ToString("yyyy-MM-dd HH:mm")}");
    Console.WriteLine($"Completed At: {task.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-"}");
    Console.WriteLine($"Project: {task.Project?.Name ?? "-"}");
}

void CompleteTask()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }

    task.Complete(DateTime.Now);
    Console.WriteLine($"Completed: {task}");
}

TaskItem? AskForTask()
{
    var id = ConsoleInput.ReadInt("Task id: ");
    foreach (var task in tasks)
    {
        if (task.Id == id)
        {
            return task;
        }
    }

    Console.WriteLine($"Task #{id} not found.");
    return null;
}

List<TaskItem> FindOverdue(DateTime now)
{
    var result = new List<TaskItem>();
    foreach (var task in tasks)
    {
        if (task.IsOverdue(now))
        {
            result.Add(task);
        }
    }

    return result;
}

void PrintTasks(List<TaskItem> items, string header)
{
    Console.WriteLine($"--- {header}: {items.Count} ---");
    if (items.Count == 0)
    {
        Console.WriteLine("(empty)");
        return;
    }

    foreach (var task in items)
    {
        Console.WriteLine(task);
    }
}
