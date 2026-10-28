using TaskFlow.Domain.Entities;

namespace TaskFlow.ConsoleApp;

public static class TypesDemo
{
    public static void Run()
    {
        Console.WriteLine("--- Value types vs reference types ---");

        int a = 10;
        int b = a;
        b = 20;
        Console.WriteLine($"1) int: a = {a}, b = {b}");
        Console.WriteLine("   int is a value type: b got a copy, so changing b did not change a.");

        var start = new DateTime(2026, 11, 1);
        var copy = start;
        copy = copy.AddDays(1);
        Console.WriteLine($"2) DateTime: start = {start:yyyy-MM-dd}, copy = {copy:yyyy-MM-dd}");
        Console.WriteLine("   DateTime is a struct: copy is independent, start stayed the same.");

        var first = new List<string> { "A" };
        var second = first;
        second.Add("B");
        Console.WriteLine($"3) List: first.Count = {first.Count}, second.Count = {second.Count}");
        Console.WriteLine("   List is a reference type: both variables point to the same list.");

        var task = new TaskItem("Original title");
        var sameTask = task;
        sameTask.Title = "Changed title";
        Console.WriteLine($"4) TaskItem: task.Title = '{task.Title}'");
        Console.WriteLine("   TaskItem is a class: sameTask and task are the same object.");
    }
}