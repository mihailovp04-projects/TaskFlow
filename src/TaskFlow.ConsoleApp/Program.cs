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

    var task = new TaskItem(title, priority, dueDate);
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
