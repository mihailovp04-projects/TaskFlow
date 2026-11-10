/* 02-seed.sql
   Заполняет TaskFlowLab тестовыми данными.
   Запускать сразу после 01-schema.sql (база пустая, IDENTITY начинается с 1). */

USE TaskFlowLab;
GO

SET NOCOUNT ON;

-- 1. Пользователи: префикс N'' обязателен для Unicode (ș, ț, кириллица)
INSERT INTO dbo.Users (Email, DisplayName, IsActive)
VALUES
    (N'ana.popescu@taskflow.local', N'Ana Popescu', 1),
    (N'stefan.turcanu@taskflow.local', N'Ștefan Țurcanu', 1),
    (N'anna.petrova@taskflow.local', N'Анна Петрова', 1),
    (N'ion.rusu@taskflow.local', N'Ion Rusu', 1),
    (N'maria.ceban@taskflow.local', N'Maria Ceban', 1),
    (N'guest@taskflow.local', N'Guest User', 0);

-- 2. Проекты: у проекта 4 задач не будет (пригодится для LEFT JOIN)
INSERT INTO dbo.Projects (Name, Description, OwnerId)
VALUES
    (N'Website', N'Public website of the company', 1),
    (N'Mobile App', N'iOS and Android client', 2),
    (N'Internal Tools', N'Scripts and admin tools', 3),
    (N'Archive 2025', NULL, 1);

-- 3. Метки
INSERT INTO dbo.Tags (Name)
VALUES (N'bug'), (N'feature'), (N'docs'), (N'urgent'), (N'backend'), (N'frontend');

-- 4. 10 000 задач одним INSERT ... SELECT (set-based, без цикла)
--    CTE Numbers выдаёт числа 1..10000: перекрёстное соединение системного
--    представления с самим собой даёт миллионы строк, TOP оставляет нужное количество
DECLARE @BaseDate DATETIME2(0) = '2026-01-01T09:00:00';

WITH Numbers AS
(
    SELECT TOP (10000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
)
INSERT INTO dbo.Tasks (Title, Description, Priority, Status, DueDate, CompletedAt, ProjectId, AssigneeId, CreatedAt)
SELECT
    CONCAT(CHOOSE(n % 4 + 1, N'Fix bug', N'Write docs', N'Review PR', N'Deploy build'), N' #', n),
    -- у каждой пятой задачи нет описания
    CASE WHEN n % 5 = 0 THEN NULL ELSE CONCAT(N'Auto-generated task number ', n) END,
    -- приоритет 0..3
    n % 4,
    -- статус 0..2
    n % 3,
    -- у каждой шестой задачи нет срока, остальные распределены по 2026 году
    CASE WHEN n % 6 = 0 THEN NULL ELSE DATEADD(DAY, n % 365, @BaseDate) END,
    -- CompletedAt только для статуса Done, иначе сработает CK_Tasks_CompletedAt
    CASE WHEN n % 3 = 2 THEN DATEADD(DAY, n % 365, @BaseDate) ELSE NULL END,
    -- каждая десятая задача без проекта, остальные в проектах 1..3
    CASE WHEN n % 10 = 0 THEN NULL ELSE (n / 3) % 3 + 1 END,
    -- каждая седьмая задача без исполнителя; нагрузка неравномерная:
    -- пользователь 1 около 40%, пользователи 2 и 3 по 20%, пользователи 4 и 5 по 10%
    CASE
        WHEN n % 7 = 0 THEN NULL
        WHEN n % 10 < 4 THEN 1
        WHEN n % 10 < 6 THEN 2
        WHEN n % 10 < 8 THEN 3
        WHEN n % 10 = 8 THEN 4
        ELSE 5
    END,
    DATEADD(MINUTE, n, @BaseDate)
FROM Numbers;

-- 5. Связи N:M: у каждой задачи одна метка, у каждой четвёртой две
INSERT INTO dbo.TaskTags (TaskId, TagId)
SELECT t.Id, t.Id % 6 + 1
FROM dbo.Tasks AS t
UNION ALL
SELECT t.Id, (t.Id + 2) % 6 + 1
FROM dbo.Tasks AS t
WHERE t.Id % 4 = 0;

-- 6. Комментарии: по одному у каждой 50-й задачи, второй у каждой 500-й
INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
SELECT t.Id, t.Id % 5 + 1, CONCAT(N'First comment for task ', t.Id)
FROM dbo.Tasks AS t
WHERE t.Id % 50 = 0;

INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
SELECT t.Id, 3, N'Follow-up: please check again'
FROM dbo.Tasks AS t
WHERE t.Id % 500 = 0;

-- проверка количества строк
SELECT CAST(x.TableName AS VARCHAR(15)) AS TableName, x.RowsCount
FROM
(
    SELECT 'Users' AS TableName, COUNT(*) AS RowsCount, 1 AS SortOrder FROM dbo.Users
    UNION ALL SELECT 'Projects', COUNT(*), 2 FROM dbo.Projects
    UNION ALL SELECT 'Tags', COUNT(*), 3 FROM dbo.Tags
    UNION ALL SELECT 'Tasks', COUNT(*), 4 FROM dbo.Tasks
    UNION ALL SELECT 'TaskTags', COUNT(*), 5 FROM dbo.TaskTags
    UNION ALL SELECT 'TaskComments', COUNT(*), 6 FROM dbo.TaskComments
) AS x
ORDER BY x.SortOrder;
GO
