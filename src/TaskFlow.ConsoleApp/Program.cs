using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

var tasks = new List<TaskItem>();
var nextId = 1;
var projects = new Dictionary<int, Project>();

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
                PrintTasks(TaskQueries.FindOverdue(tasks, DateTime.Now), "Overdue tasks");
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
            case "9":
                ProjectMenu.CreateProject(projects);
                break;
            case "10":
                ProjectMenu.ListProjects(projects);
                break;
            case "11":
                AssignTaskToProject();
                break;
            case "12":
                FilterByPriority();
                break;
            case "13":
                FilterByStatus();
                break;
            case "14":
                ShowStatistics();
                break;
            case "15":
                PrintTasks(TaskQueries.SortByDueDateThenPriority(tasks), "Tasks sorted by due date and priority");
                break;
            case "16":
                SearchTasks();
                break;
            case "99":
                TypesDemo.Run();
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
    Console.WriteLine("9) Create project");
    Console.WriteLine("10) List projects");
    Console.WriteLine("11) Add task to project");
    Console.WriteLine("12) Filter by priority");
    Console.WriteLine("13) Filter by status");
    Console.WriteLine("14) Show statistics");
    Console.WriteLine("15) Sort tasks by due date");
    Console.WriteLine("16) Search tasks");
    Console.WriteLine("99) Types demo");
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
    var description = ConsoleInput.ReadText("Description (empty = none): ");

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

void ShowTaskDetails()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }
    Console.WriteLine($"Id: {task.Id}");
    Console.WriteLine($"Title: {task.Title}");
    Console.WriteLine($"Description: {task.Description ?? "-"}");
    Console.WriteLine($"Priority: {task.Priority}");
    Console.WriteLine($"Status: {task.Status}");
    Console.WriteLine($"Due date: {task.DueDate?.ToString("yyyy-MM-dd") ?? "-"}");
    Console.WriteLine($"Created at: {task.CreatedAt:yyyy-MM-dd HH:mm}");
    Console.WriteLine($"Completed at: {task.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-"}");
    Console.WriteLine($"Project: {task.Project?.Name ?? "-"}");
}

void EditTask()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }

    var title = ConsoleInput.ReadText("New title (empty = keep): ");
    if (!string.IsNullOrWhiteSpace(title))
    {
        task.Title = title;
    }

    var description = ConsoleInput.ReadText("New description (empty = keep): ");
    if (!string.IsNullOrWhiteSpace(description))
    {
        task.Description = description;
    }

    var priority = ConsoleInput.ReadOptionalPriority();
    if (priority is not null)
    {
        task.Priority = priority.Value;
    }

    var dueDate = ConsoleInput.ReadOptionalDate("New due date (yyyy-MM-dd, empty = keep): ");
    if (dueDate is not null)
    {
        task.DueDate = dueDate;
    }

    Console.WriteLine($"Updated: {task}");
}

void AssignTaskToProject()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }

    ProjectMenu.AssignTaskToProject(projects, task);
}

void FilterByPriority()
{
    var priority = ConsoleInput.ReadPriority();
    PrintTasks(TaskQueries.FindByPriority(tasks, priority), $"Tasks with priority {priority}");
}

void FilterByStatus()
{
    var status = ConsoleInput.ReadStatus();
    PrintTasks(TaskQueries.FindByStatus(tasks, status), $"Tasks with status {status}");
}

void SearchTasks()
{
    var text = ConsoleInput.ReadText("Search text: ").Trim();
    if (string.IsNullOrWhiteSpace(text))
    {
        Console.WriteLine("Search text is empty.");
        return;
    }

    PrintTasks(TaskQueries.FindByText(tasks, text), $"Tasks containing '{text}'");
}
void ShowStatistics()
{
    var counts = TaskQueries.CountByStatus(tasks);

    Console.WriteLine("--- Statistics ---");
    foreach (var pair in counts)
    {
        Console.WriteLine($"{pair.Key}: {pair.Value}");
    }

    Console.WriteLine($"Overdue: {TaskQueries.FindOverdue(tasks, DateTime.Now).Count}");
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