USE TaskFlowLab;
GO

DROP TABLE IF EXISTS dbo.TimeEntryActivities;
DROP TABLE IF EXISTS dbo.TimeEntries;
DROP TABLE IF EXISTS dbo.Activities;
GO

CREATE TABLE dbo.TimeEntries
(
    Id INT IDENTITY(1,1) NOT NULL,
    TaskId INT NOT NULL,
    UserId INT NOT NULL,
    WorkDate DATE NOT NULL,
    Hours DECIMAL(4,2) NOT NULL,
    Note NVARCHAR(500) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TimeEntries_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TimeEntries PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_TimeEntries_Hours CHECK (Hours > 0 AND Hours <= 24),
    CONSTRAINT UQ_TimeEntries_Task_User_Date UNIQUE (TaskId, UserId, WorkDate),
    CONSTRAINT FK_TimeEntries_Tasks_TaskId FOREIGN KEY (TaskId)
        REFERENCES dbo.Tasks (Id) ON DELETE CASCADE,
    CONSTRAINT FK_TimeEntries_Users_UserId FOREIGN KEY (UserId)
        REFERENCES dbo.Users (Id) ON DELETE NO ACTION
);
GO

CREATE TABLE dbo.Activities
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Activities PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Activities_Name UNIQUE (Name)
);
GO

CREATE TABLE dbo.TimeEntryActivities
(
    TimeEntryId INT NOT NULL,
    ActivityId INT NOT NULL,
    CONSTRAINT PK_TimeEntryActivities PRIMARY KEY CLUSTERED (TimeEntryId, ActivityId),
    CONSTRAINT FK_TimeEntryActivities_TimeEntries_TimeEntryId FOREIGN KEY (TimeEntryId)
        REFERENCES dbo.TimeEntries (Id) ON DELETE CASCADE,
    CONSTRAINT FK_TimeEntryActivities_Activities_ActivityId FOREIGN KEY (ActivityId)
        REFERENCES dbo.Activities (Id) ON DELETE CASCADE
);
GO
SET NOCOUNT ON;

INSERT INTO dbo.Activities (Name)
VALUES (N'coding'), (N'review'), (N'testing'), (N'design'), (N'documentation'), (N'meeting');

DECLARE @OctoberStart DATE = '2026-10-01';

WITH Numbers AS
(
    SELECT TOP (5000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects AS a
    CROSS JOIN sys.all_objects AS b
)
INSERT INTO dbo.TimeEntries (TaskId, UserId, WorkDate, Hours, Note)
SELECT
    n,
    n % 5 + 1,
    DATEADD(DAY, n % 31, @OctoberStart),
    CAST((n % 32 + 1) * 0.25 AS DECIMAL(4,2)),
    CASE WHEN n % 10 = 0 THEN NULL ELSE CONCAT(N'Work on task ', n) END
FROM Numbers;

INSERT INTO dbo.TimeEntryActivities (TimeEntryId, ActivityId)
SELECT e.Id, e.Id % 6 + 1
FROM dbo.TimeEntries AS e
UNION ALL
SELECT e.Id, (e.Id + 3) % 6 + 1
FROM dbo.TimeEntries AS e
WHERE e.Id % 5 = 0;

SELECT
    (SELECT COUNT(*) FROM dbo.Activities) AS Activities,
    (SELECT COUNT(*) FROM dbo.TimeEntries) AS TimeEntries,
    (SELECT COUNT(*) FROM dbo.TimeEntryActivities) AS TimeEntryActivities,
    (SELECT MIN(Hours) FROM dbo.TimeEntries) AS MinHours,
    (SELECT MAX(Hours) FROM dbo.TimeEntries) AS MaxHours,
    (SELECT MIN(WorkDate) FROM dbo.TimeEntries) AS MinDate,
    (SELECT MAX(WorkDate) FROM dbo.TimeEntries) AS MaxDate;
GO

SET NOCOUNT ON;

-- 4.1 Сколько часов списано на каждый проект за октябрь 2026, включая проекты без часов?
PRINT N'--- 4.1 Часы по проектам за октябрь';
SELECT CAST(p.Name AS NVARCHAR(20)) AS Project, ISNULL(SUM(e.Hours), 0) AS TotalHours
FROM dbo.Projects AS p
LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries AS e ON e.TaskId = t.Id
    AND e.WorkDate >= '2026-10-01' AND e.WorkDate < '2026-11-01'
GROUP BY p.Id, p.Name
ORDER BY p.Name;

-- 4.2 Какие три сотрудника списали больше всего часов?
PRINT N'--- 4.2 Топ-3 сотрудника по часам';
SELECT TOP (3) CAST(u.DisplayName AS NVARCHAR(20)) AS UserName, SUM(e.Hours) AS TotalHours
FROM dbo.TimeEntries AS e
JOIN dbo.Users AS u ON u.Id = e.UserId
GROUP BY u.Id, u.DisplayName
ORDER BY TotalHours DESC, UserName;

-- 4.3 Какие задачи в работе ни разу не получали списания времени?
PRINT N'--- 4.3 Задачи в работе без списаний времени';
SELECT t.Id, CAST(t.Title AS NVARCHAR(30)) AS Title
FROM dbo.Tasks AS t
WHERE t.Status = 1
    AND NOT EXISTS (SELECT 1 FROM dbo.TimeEntries AS e WHERE e.TaskId = t.Id)
ORDER BY t.Id
OFFSET 0 ROWS FETCH NEXT 5 ROWS ONLY;

SELECT COUNT(*) AS InProgressWithoutTime
FROM dbo.Tasks AS t
WHERE t.Status = 1
    AND NOT EXISTS (SELECT 1 FROM dbo.TimeEntries AS e WHERE e.TaskId = t.Id);

-- 4.4 У каких сотрудников в среднем больше 6 часов в день?
PRINT N'--- 4.4 Больше 6 часов в день в среднем';
SELECT
    CAST(u.DisplayName AS NVARCHAR(20)) AS UserName,
    CAST(SUM(e.Hours) / COUNT(DISTINCT e.WorkDate) AS DECIMAL(6,2)) AS AvgHoursPerDay
FROM dbo.TimeEntries AS e
JOIN dbo.Users AS u ON u.Id = e.UserId
GROUP BY u.Id, u.DisplayName
HAVING SUM(e.Hours) / COUNT(DISTINCT e.WorkDate) > 6
ORDER BY AvgHoursPerDay DESC;

-- 4.5 Сколько часов списано за каждую неделю октября (неделя с понедельника)?
PRINT N'--- 4.5 Часы по неделям октября';
WITH EntryWeeks AS
(
    SELECT
        DATEADD(DAY, -((DATEPART(WEEKDAY, e.WorkDate) + @@DATEFIRST - 2) % 7), e.WorkDate) AS WeekStart,
        e.Hours
    FROM dbo.TimeEntries AS e
    WHERE e.WorkDate >= '2026-10-01' AND e.WorkDate < '2026-11-01'
)
SELECT w.WeekStart, SUM(w.Hours) AS TotalHours
FROM EntryWeeks AS w
GROUP BY w.WeekStart
ORDER BY w.WeekStart;

-- 4.6 В каких проектах выполнено меньше 40% задач?
PRINT N'--- 4.6 Проекты с долей выполненных задач меньше 40%';
WITH ProjectStats AS
(
    SELECT
        p.Name,
        COUNT(t.Id) AS TotalTasks,
        SUM(CASE WHEN t.Status = 2 THEN 1 ELSE 0 END) AS DoneTasks
    FROM dbo.Projects AS p
    LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
    GROUP BY p.Id, p.Name
)
SELECT
    CAST(s.Name AS NVARCHAR(20)) AS Project,
    s.TotalTasks,
    s.DoneTasks,
    CAST(100.0 * s.DoneTasks / NULLIF(s.TotalTasks, 0) AS DECIMAL(5,2)) AS DonePercent
FROM ProjectStats AS s
WHERE 100.0 * s.DoneTasks / NULLIF(s.TotalTasks, 0) < 40
ORDER BY s.Name;

-- 4.7 У каких пяти задач больше всего комментариев и когда был последний?
PRINT N'--- 4.7 Топ-5 задач по комментариям';
SELECT TOP (5)
    CAST(t.Title AS NVARCHAR(30)) AS Title,
    COUNT(c.Id) AS CommentCount,
    MAX(c.CreatedAt) AS LastCommentAt
FROM dbo.Tasks AS t
JOIN dbo.TaskComments AS c ON c.TaskId = t.Id
GROUP BY t.Id, t.Title
ORDER BY CommentCount DESC, t.Id;

-- 4.8 Какая вторая страница (по 2 строки) у отчёта из запроса 4.1?
PRINT N'--- 4.8 Запрос 4.1, страница 2 по 2 строки';
SELECT CAST(p.Name AS NVARCHAR(20)) AS Project, ISNULL(SUM(e.Hours), 0) AS TotalHours
FROM dbo.Projects AS p
LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries AS e ON e.TaskId = t.Id
    AND e.WorkDate >= '2026-10-01' AND e.WorkDate < '2026-11-01'
GROUP BY p.Id, p.Name
ORDER BY p.Name
OFFSET 2 ROWS FETCH NEXT 2 ROWS ONLY;
GO

CREATE OR ALTER VIEW dbo.vw_UserWorkload
AS
SELECT
    u.Id AS UserId,
    u.DisplayName,
    ISNULL(t.OpenTasks, 0) AS OpenTasks,
    ISNULL(t.OverdueTasks, 0) AS OverdueTasks,
    ISNULL(e.HoursLast30Days, 0) AS HoursLast30Days
FROM dbo.Users AS u
LEFT JOIN
(
    SELECT
        AssigneeId,
        COUNT(*) AS OpenTasks,
        SUM(CASE WHEN DueDate < '2026-10-31' THEN 1 ELSE 0 END) AS OverdueTasks
    FROM dbo.Tasks
    WHERE Status <> 2
    GROUP BY AssigneeId
) AS t ON t.AssigneeId = u.Id
LEFT JOIN
(
    SELECT UserId, SUM(Hours) AS HoursLast30Days
    FROM dbo.TimeEntries
    WHERE WorkDate >= '2026-10-01' AND WorkDate < '2026-10-31'
    GROUP BY UserId
) AS e ON e.UserId = u.Id;
GO

SET NOCOUNT ON;

PRINT N'--- 5. vw_UserWorkload';
SELECT UserId, CAST(DisplayName AS NVARCHAR(20)) AS DisplayName, OpenTasks, OverdueTasks, HoursLast30Days
FROM dbo.vw_UserWorkload
ORDER BY UserId;
GO

CREATE OR ALTER PROCEDURE dbo.usp_LogTime
    @TaskId INT,
    @UserId INT,
    @WorkDate DATE,
    @Hours DECIMAL(4,2),
    @Note NVARCHAR(500) = NULL,
    @NewEntryId INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE Id = @TaskId)
        THROW 50101, N'Task not found.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Tasks WHERE Id = @TaskId AND Status = 2)
        THROW 50102, N'Task is already completed.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @UserId)
        THROW 50103, N'User not found.', 1;

    IF @WorkDate IS NULL
        THROW 50104, N'Work date is required.', 1;

    IF @Hours IS NULL OR @Hours <= 0 OR @Hours > 24
        THROW 50105, N'Hours must be greater than 0 and not greater than 24.', 1;

    INSERT INTO dbo.TimeEntries (TaskId, UserId, WorkDate, Hours, Note)
    VALUES (@TaskId, @UserId, @WorkDate, @Hours, @Note);

    SET @NewEntryId = CAST(SCOPE_IDENTITY() AS INT);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_MoveProjectTasks
    @FromProjectId INT,
    @ToProjectId INT,
    @AuthorId INT,
    @MovedCount INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @FromName NVARCHAR(100) = (SELECT Name FROM dbo.Projects WHERE Id = @FromProjectId);

    IF @FromName IS NULL
        THROW 50111, N'Source project not found.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE Id = @ToProjectId)
        THROW 50112, N'Target project not found.', 1;

    IF @FromProjectId = @ToProjectId
        THROW 50113, N'Source and target projects must be different.', 1;

    DECLARE @Moved TABLE (Id INT PRIMARY KEY);

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Tasks
        SET ProjectId = @ToProjectId
        OUTPUT inserted.Id INTO @Moved (Id)
        WHERE ProjectId = @FromProjectId AND Status <> 2;

        INSERT INTO dbo.TaskComments (TaskId, AuthorId, Body)
        SELECT m.Id, @AuthorId, CONCAT(N'Moved from project ', @FromName)
        FROM @Moved AS m;

        COMMIT TRANSACTION;

        SET @MovedCount = (SELECT COUNT(*) FROM @Moved);
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

SET NOCOUNT ON;

PRINT N'--- 6.1 usp_LogTime: успешный вызов';
DECLARE @EntryId INT;
EXEC dbo.usp_LogTime @TaskId = 5002, @UserId = 4, @WorkDate = '2026-10-30', @Hours = 1.5,
    @Note = N'Investigated the issue', @NewEntryId = @EntryId OUTPUT;
SELECT e.Id, e.TaskId, e.UserId, e.WorkDate, e.Hours
FROM dbo.TimeEntries AS e
WHERE e.Id = @EntryId;

PRINT N'--- 6.2 usp_LogTime: задача уже выполнена';
BEGIN TRY
    EXEC dbo.usp_LogTime @TaskId = 1, @UserId = 1, @WorkDate = '2026-10-30', @Hours = 2;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'Ошибка ', ERROR_NUMBER(), N': ', ERROR_MESSAGE());
END CATCH;

PRINT N'--- 6.3 usp_MoveProjectTasks: успешный перенос из проекта 3 в проект 4';
DECLARE @Count INT;
EXEC dbo.usp_MoveProjectTasks @FromProjectId = 3, @ToProjectId = 4, @AuthorId = 1, @MovedCount = @Count OUTPUT;
SELECT
    @Count AS MovedTasks,
    (SELECT COUNT(*) FROM dbo.Tasks WHERE ProjectId = 3 AND Status <> 2) AS OpenLeftInProject3,
    (SELECT COUNT(*) FROM dbo.TaskComments WHERE Body = N'Moved from project Internal Tools') AS MoveComments;

PRINT N'--- 6.4 usp_MoveProjectTasks: несуществующий автор, откат';
DECLARE @Before INT = (SELECT COUNT(*) FROM dbo.Tasks WHERE ProjectId = 2);
BEGIN TRY
    EXEC dbo.usp_MoveProjectTasks @FromProjectId = 2, @ToProjectId = 3, @AuthorId = 999;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'Ошибка ', ERROR_NUMBER(), N': откат выполнен');
END CATCH;
SELECT @Before AS TasksBefore, (SELECT COUNT(*) FROM dbo.Tasks WHERE ProjectId = 2) AS TasksAfter;
GO

SET NOCOUNT ON;
DROP INDEX IF EXISTS IX_Tasks_Status ON dbo.Tasks;
GO

SET STATISTICS IO ON;
GO

PRINT N'--- 7.1 До индексов: запрос 4.1';
SELECT CAST(p.Name AS NVARCHAR(20)) AS Project, ISNULL(SUM(e.Hours), 0) AS TotalHours
FROM dbo.Projects AS p
LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries AS e ON e.TaskId = t.Id
    AND e.WorkDate >= '2026-10-01' AND e.WorkDate < '2026-11-01'
GROUP BY p.Id, p.Name
ORDER BY p.Name;

PRINT N'--- 7.2 До индексов: запрос 4.3';
SELECT COUNT(*) AS InProgressWithoutTime
FROM dbo.Tasks AS t
WHERE t.Status = 1
    AND NOT EXISTS (SELECT 1 FROM dbo.TimeEntries AS e WHERE e.TaskId = t.Id);
GO

SET STATISTICS IO OFF;
GO

CREATE NONCLUSTERED INDEX IX_TimeEntries_UserId ON dbo.TimeEntries (UserId);
CREATE NONCLUSTERED INDEX IX_TimeEntryActivities_ActivityId ON dbo.TimeEntryActivities (ActivityId);
CREATE NONCLUSTERED INDEX IX_TimeEntries_WorkDate ON dbo.TimeEntries (WorkDate) INCLUDE (TaskId, Hours);
CREATE NONCLUSTERED INDEX IX_Tasks_Status ON dbo.Tasks (Status) INCLUDE (Title);
GO

SET STATISTICS IO ON;
GO

PRINT N'--- 7.3 После индексов: запрос 4.1';
SELECT CAST(p.Name AS NVARCHAR(20)) AS Project, ISNULL(SUM(e.Hours), 0) AS TotalHours
FROM dbo.Projects AS p
LEFT JOIN dbo.Tasks AS t ON t.ProjectId = p.Id
LEFT JOIN dbo.TimeEntries AS e ON e.TaskId = t.Id
    AND e.WorkDate >= '2026-10-01' AND e.WorkDate < '2026-11-01'
GROUP BY p.Id, p.Name
ORDER BY p.Name;

PRINT N'--- 7.4 После индексов: запрос 4.3';
SELECT COUNT(*) AS InProgressWithoutTime
FROM dbo.Tasks AS t
WHERE t.Status = 1
    AND NOT EXISTS (SELECT 1 FROM dbo.TimeEntries AS e WHERE e.TaskId = t.Id);
GO

SET STATISTICS IO OFF;
GO

GRANT EXECUTE ON OBJECT::dbo.usp_LogTime TO TaskFlowApp;
GRANT EXECUTE ON OBJECT::dbo.usp_MoveProjectTasks TO TaskFlowApp;
GRANT SELECT ON OBJECT::dbo.vw_UserWorkload TO TaskFlowApp;
GO

SET NOCOUNT ON;

PRINT N'--- 8. Проверка прав от имени TaskFlowApp';
EXECUTE AS USER = N'TaskFlowApp';

BEGIN TRY
    SELECT TOP (1) Id FROM dbo.TimeEntries;
END TRY
BEGIN CATCH
    PRINT CONCAT(N'Прямой SELECT из TimeEntries: ошибка ', ERROR_NUMBER());
END CATCH;

DECLARE @AppEntryId INT;
EXEC dbo.usp_LogTime @TaskId = 5005, @UserId = 5, @WorkDate = '2026-10-30', @Hours = 2,
    @NewEntryId = @AppEntryId OUTPUT;
SELECT CASE WHEN @AppEntryId IS NOT NULL THEN 'yes' ELSE 'no' END AS LoggedViaProcedure;

SELECT COUNT(*) AS UsersInWorkload FROM dbo.vw_UserWorkload;

REVERT;

SELECT CAST(USER_NAME() AS NVARCHAR(20)) AS CurrentUserAfterRevert;
GO