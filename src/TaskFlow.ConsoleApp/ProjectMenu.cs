using TaskFlow.Domain.Entities;

namespace TaskFlow.ConsoleApp;

public static class ProjectMenu
{
    public static void CreateProject(Dictionary<int, Project> projects)
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
        project.Id = projects.Count + 1;
        projects.Add(project.Id, project);

        Console.WriteLine($"Created: #{project.Id} {project.Name}");
    }

    public static void ListProjects(Dictionary<int, Project> projects)
    {
        if (projects.Count == 0)
        {
            Console.WriteLine("(no projects)");
            return;
        }

        foreach (var project in projects.Values)
        {
            Console.WriteLine($"#{project.Id} {project.Name} (tasks: {project.Tasks.Count}, overdue: {project.CountOverdue(DateTime.Now)})");
        }
    }

    public static void AssignTaskToProject(Dictionary<int, Project> projects, TaskItem task)
    {
        var projectId = ConsoleInput.ReadInt("Project id: ");
        if (!projects.TryGetValue(projectId, out var project))
        {
            Console.WriteLine($"Project #{projectId} not found.");
            return;
        }

        project.AddTask(task);
        Console.WriteLine($"Task #{task.Id} added to project '{project.Name}'.");
    }
}