/* 04-indexes.sql
   Индексы TaskFlowLab: индексы на внешние ключи, сравнение Scan и Seek, покрывающий индекс.
   Запускайте целиком через sqlcmd (смотрим logical reads) и по блокам в SSMS
   с включённым планом выполнения (Ctrl+M), чтобы увидеть операторы плана. */

USE TaskFlowLab;
GO

SET NOCOUNT ON;

-- 0. Начинаем с чистого состояния, чтобы скрипт можно было запускать повторно
DROP INDEX IF EXISTS IX_Projects_OwnerId ON dbo.Projects;
DROP INDEX IF EXISTS IX_Tasks_ProjectId ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_Title ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_AssigneeId ON dbo.Tasks;
DROP INDEX IF EXISTS IX_Tasks_AssigneeId_Status_DueDate ON dbo.Tasks;
DROP INDEX IF EXISTS IX_TaskTags_TagId ON dbo.TaskTags;
DROP INDEX IF EXISTS IX_TaskComments_TaskId ON dbo.TaskComments;
DROP INDEX IF EXISTS IX_TaskComments_AuthorId ON dbo.TaskComments;
GO

-- 1. Индексы на внешние ключи.
--    SQL Server НЕ создаёт их автоматически (MySQL InnoDB создаёт).
--    Без них JOIN по FK и удаление родителя (проверка ссылок) читают всю дочернюю таблицу.
--    TaskTags.TaskId отдельный индекс не нужен: это первая колонка PK_TaskTags.
CREATE NONCLUSTERED INDEX IX_Projects_OwnerId ON dbo.Projects (OwnerId);
CREATE NONCLUSTERED INDEX IX_Tasks_ProjectId ON dbo.Tasks (ProjectId);
CREATE NONCLUSTERED INDEX IX_TaskTags_TagId ON dbo.TaskTags (TagId);
CREATE NONCLUSTERED INDEX IX_TaskComments_TaskId ON dbo.TaskComments (TaskId);
CREATE NONCLUSTERED INDEX IX_TaskComments_AuthorId ON dbo.TaskComments (AuthorId);
GO

SET STATISTICS IO ON;
GO

-- 2. Поиск по названию без индекса: Clustered Index Scan (читается вся таблица)
PRINT N'--- 2. Title = ... без индекса';
SELECT Id, Title, Status
FROM dbo.Tasks
WHERE Title = N'Review PR #5002';
GO

-- 3. Тот же запрос с индексом по Title: Index Seek + Key Lookup
CREATE NONCLUSTERED INDEX IX_Tasks_Title ON dbo.Tasks (Title);
GO

PRINT N'--- 3. Title = ... с индексом IX_Tasks_Title';
SELECT Id, Title, Status
FROM dbo.Tasks
WHERE Title = N'Review PR #5002';
GO

-- 4. Индекс есть, но условие "не sargable": ведущий % не позволяет искать по дереву.
--    Оптимизатор читает весь индекс или всю таблицу (Scan).
PRINT N'--- 4. LIKE с ведущим % (Scan, индекс не помогает)';
SELECT Id, Title
FROM dbo.Tasks
WHERE Title LIKE N'%#5002';
GO

-- 5. Префиксный поиск sargable: Index Seek по диапазону
PRINT N'--- 5. LIKE по префиксу (Seek)';
SELECT Id, Title
FROM dbo.Tasks
WHERE Title LIKE N'Review PR #500%';
GO

-- 6. "Мои задачи в работе со сроком на этой неделе": без индекса по AssigneeId это Scan
PRINT N'--- 6. AssigneeId + Status + DueDate без индекса';
SELECT Id, Title, DueDate
FROM dbo.Tasks
WHERE AssigneeId = 4 AND Status = 1
    AND DueDate >= '2026-10-12' AND DueDate < '2026-10-19';
GO

-- 7. Индекс только по AssigneeId: у пользователя 4 около 900 задач,
--    на каждую понадобился бы Key Lookup, это дороже, чем прочитать таблицу.
--    Оптимизатор ИГНОРИРУЕТ такой индекс: в плане по-прежнему Clustered Index Scan.
CREATE NONCLUSTERED INDEX IX_Tasks_AssigneeId ON dbo.Tasks (AssigneeId);
GO

PRINT N'--- 7. С индексом только по AssigneeId (индекс не используется)';
SELECT Id, Title, DueDate
FROM dbo.Tasks
WHERE AssigneeId = 4 AND Status = 1
    AND DueDate >= '2026-10-12' AND DueDate < '2026-10-19';
GO

-- 8. Покрывающий индекс: все условия WHERE в ключе (AssigneeId, Status, DueDate),
--    Title хранится в листьях через INCLUDE, поэтому Key Lookup не нужен:
--    в плане остаётся один Index Seek
DROP INDEX IX_Tasks_AssigneeId ON dbo.Tasks;
CREATE NONCLUSTERED INDEX IX_Tasks_AssigneeId_Status_DueDate
    ON dbo.Tasks (AssigneeId, Status, DueDate)
    INCLUDE (Title);
GO

PRINT N'--- 8. С покрывающим индексом';
SELECT Id, Title, DueDate
FROM dbo.Tasks
WHERE AssigneeId = 4 AND Status = 1
    AND DueDate >= '2026-10-12' AND DueDate < '2026-10-19';
GO

SET STATISTICS IO OFF;
GO

-- 9. Итоговый список индексов
PRINT N'--- 9. Индексы в базе';
SELECT
    CAST(OBJECT_NAME(i.object_id) AS NVARCHAR(15)) AS TableName,
    CAST(i.name AS NVARCHAR(40)) AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
WHERE i.name IS NOT NULL
ORDER BY TableName, i.type_desc, IndexName;
GO
