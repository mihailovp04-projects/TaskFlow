# TaskFlow

Учебный менеджер задач (курс C#/.NET junior).

## Требования
- .NET SDK 10.0.x

## Сборка и запуск
dotnet build TaskFlow.slnx
dotnet run --project src/TaskFlow.ConsoleApp

## Структура
- src/TaskFlow.Domain: доменная модель (TaskItem, Project, перечисления)
- src/TaskFlow.ConsoleApp: консольное меню над списком задач в памяти
