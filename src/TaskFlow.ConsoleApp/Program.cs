using TaskFlow.ConsoleApp;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

var tasks = new List<TaskItem>();
var nextId = 1;
var projects = new Dictionary<int, Project>();
var nextProjectId = 1;

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
            case "9":
                CreateProject();
                break;
            case "10":
                ListProjects();
                break;
            case "11":
                AssignTaskToProject();
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
    if (dueDate != null)
    {
        task.DueDate = dueDate;
    }
}

    void CreateProject()
    {
        var name = ConsoleInput.ReadText("Project name: ");
        var description = ConsoleInput.ReadText("Project description (empty = none): ");
        foreach (var existing in projects.Values)
        {
            if (string.Equals(existing.Name, name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Project '{name}' already exists.");
                return;
            }
        }
        
        var project = new Project(name, description);

        project.Id = nextProjectId;
         nextProjectId++;
        projects.Add(project.Id, project);

        Console.WriteLine($"Created: #{project.Id} {project.Name}");
    }
void ListProjects()
{
    if(projects.Count == 0)
    {
        Console.WriteLine("(no projects)");
        return;
    }
    foreach(var project in projects.Values)
    {
        Console.WriteLine($"#{project.Id} {project.Name} (tasks: {project.Tasks.Count}, overdue: {project.CountOverdue(DateTime.Now)})");
    }
}
void AssignTaskToProject()
{
    var task = AskForTask();
    if (task is null)
    {
        return;
    }
    var projectId = ConsoleInput.ReadInt("Project id:");
    if (!projects.TryGetValue(projectId, out var project))
    {
        Console.WriteLine($"Project #{projectId} not found.");
        return;
    }
    project.AddTask(task);
    Console.WriteLine($"Task #{task.Id} added to project '{project.Name}'.");
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
