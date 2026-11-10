/* 01-schema.sql
   Создаёт учебную базу TaskFlowLab с нуля.
   Скрипт можно запускать повторно: база удаляется и создаётся заново. */

USE master;
GO

IF DB_ID(N'TaskFlowLab') IS NOT NULL
BEGIN
    -- закрываем чужие сессии (например, открытое окно SSMS), иначе DROP DATABASE упадёт
    ALTER DATABASE TaskFlowLab SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TaskFlowLab;
END
GO

CREATE DATABASE TaskFlowLab;
GO

USE TaskFlowLab;
GO

-- 1. Пользователи: кто создаёт проекты, кому назначены задачи, кто пишет комментарии
CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Email CHECK (Email LIKE N'%_@_%._%')
);
GO

-- 2. Проекты: у каждого проекта ровно один владелец (связь 1:N Users -> Projects)
CREATE TABLE dbo.Projects
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL,
    OwnerId INT NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Projects_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Projects PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Projects_Name UNIQUE (Name),
    -- NO ACTION: пользователя, который владеет проектами, удалить нельзя
    CONSTRAINT FK_Projects_Users_OwnerId FOREIGN KEY (OwnerId)
        REFERENCES dbo.Users (Id) ON DELETE NO ACTION
);
GO

-- 3. Задачи: связь 1:N Projects -> Tasks и 1:N Users -> Tasks (исполнитель)
CREATE TABLE dbo.Tasks
(
    Id INT IDENTITY(1,1) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(2000) NULL,
    Priority TINYINT NOT NULL CONSTRAINT DF_Tasks_Priority DEFAULT (1),
    Status TINYINT NOT NULL CONSTRAINT DF_Tasks_Status DEFAULT (0),
    DueDate DATETIME2(0) NULL,
    CompletedAt DATETIME2(0) NULL,
    ProjectId INT NULL,
    AssigneeId INT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Tasks PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Tasks_Title CHECK (LEN(Title) > 0),
    -- 0 Low, 1 Medium, 2 High, 3 Critical (как enum TaskPriority в C#)
    CONSTRAINT CK_Tasks_Priority CHECK (Priority BETWEEN 0 AND 3),
    -- 0 Todo, 1 InProgress, 2 Done (как enum TaskItemStatus в C#)
    CONSTRAINT CK_Tasks_Status CHECK (Status BETWEEN 0 AND 2),
    -- дата завершения заполнена только у выполненных задач
    CONSTRAINT CK_Tasks_CompletedAt CHECK
        ((Status = 2 AND CompletedAt IS NOT NULL) OR (Status <> 2 AND CompletedAt IS NULL)),
    -- SET NULL: при удалении проекта задачи остаются, но без проекта
    CONSTRAINT FK_Tasks_Projects_ProjectId FOREIGN KEY (ProjectId)
        REFERENCES dbo.Projects (Id) ON DELETE SET NULL,
    -- SET NULL: при удалении пользователя задачи остаются без исполнителя
    CONSTRAINT FK_Tasks_Users_AssigneeId FOREIGN KEY (AssigneeId)
        REFERENCES dbo.Users (Id) ON DELETE SET NULL
);
GO

-- 4. Метки (справочник)
CREATE TABLE dbo.Tags
(
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Tags_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Tags PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Tags_Name UNIQUE (Name)
);
GO

-- 5. Связующая таблица N:M Tasks <-> Tags, составной первичный ключ
CREATE TABLE dbo.TaskTags
(
    TaskId INT NOT NULL,
    TagId INT NOT NULL,
    CONSTRAINT PK_TaskTags PRIMARY KEY CLUSTERED (TaskId, TagId),
    -- CASCADE: удалили задачу или метку, связь исчезает вместе с ней
    CONSTRAINT FK_TaskTags_Tasks_TaskId FOREIGN KEY (TaskId)
        REFERENCES dbo.Tasks (Id) ON DELETE CASCADE,
    CONSTRAINT FK_TaskTags_Tags_TagId FOREIGN KEY (TagId)
        REFERENCES dbo.Tags (Id) ON DELETE CASCADE
);
GO

-- 6. Комментарии к задачам: 1:N Tasks -> TaskComments
CREATE TABLE dbo.TaskComments
(
    Id INT IDENTITY(1,1) NOT NULL,
    TaskId INT NOT NULL,
    AuthorId INT NOT NULL,
    Body NVARCHAR(2000) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_TaskComments_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_TaskComments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_TaskComments_Body CHECK (LEN(Body) > 0),
    -- CASCADE: комментарии не имеют смысла без задачи
    CONSTRAINT FK_TaskComments_Tasks_TaskId FOREIGN KEY (TaskId)
        REFERENCES dbo.Tasks (Id) ON DELETE CASCADE,
    -- NO ACTION: автора с комментариями удалить нельзя
    CONSTRAINT FK_TaskComments_Users_AuthorId FOREIGN KEY (AuthorId)
        REFERENCES dbo.Users (Id) ON DELETE NO ACTION
);
GO

-- проверка: должно быть 6 таблиц
SELECT CAST(s.name + N'.' + t.name AS NVARCHAR(40)) AS TableName
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
ORDER BY t.name;
GO
