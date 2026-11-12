# Нормализация выгрузки учёта времени

## Исходная таблица

| EntryDate | UserEmail | UserName | TaskId | TaskTitle | ProjectName | ProjectOwnerEmail | Hours | Activities |
|---|---|---|---|---|---|---|---|---|
| 2026-10-12 | ion.rusu@taskflow.local | Ion Rusu | 3 | Deploy build #3 | Mobile App | stefan.turcanu@taskflow.local | 2.5 | coding, review |
| 2026-10-12 | ion.rusu@taskflow.local | Ion Rusu | 8 | Fix bug #8 | Website | ana.popescu@taskflow.local | 1.0 | testing |
| 2026-10-13 | maria.ceban@taskflow.local | Maria Ceban | 3 | Deploy build #3 | Mobile App | stefan.turcanu@taskflow.local | 4.0 | coding |

Аномалии такой таблицы:
- обновления: если Ion Rusu сменит имя или у проекта Mobile App сменится владелец, придётся исправлять все строки с ними, а пропуск хотя бы одной строки даст противоречие;
- вставки: нельзя добавить нового сотрудника, задачу или вид работы, пока по ним не списано время;
- удаления: если удалить единственную запись Maria Ceban, пропадёт и информация о самом сотруднике.

## 1НФ

Правило: каждая ячейка содержит одно значение, нет повторяющихся групп.

Нарушение: колонка `Activities` хранит список (`coding, review`). По ней нельзя нормально искать и нельзя сослаться на справочник видов работ.

Решение: разносим виды работ по строкам, колонка называется `Activity`.

| EntryDate | UserEmail | UserName | TaskId | TaskTitle | ProjectName | ProjectOwnerEmail | Hours | Activity |
|---|---|---|---|---|---|---|---|---|
| 2026-10-12 | ion.rusu@... | Ion Rusu | 3 | Deploy build #3 | Mobile App | stefan.turcanu@... | 2.5 | coding |
| 2026-10-12 | ion.rusu@... | Ion Rusu | 3 | Deploy build #3 | Mobile App | stefan.turcanu@... | 2.5 | review |
| 2026-10-12 | ion.rusu@... | Ion Rusu | 8 | Fix bug #8 | Website | ana.popescu@... | 1.0 | testing |
| 2026-10-13 | maria.ceban@... | Maria Ceban | 3 | Deploy build #3 | Mobile App | stefan.turcanu@... | 4.0 | coding |

Ключ: `(EntryDate, UserEmail, TaskId, Activity)`.

Теперь видно новое дублирование: часы 2.5 записаны дважды, хотя это одна запись времени.

## 2НФ

Правило: таблица в 1НФ, и каждая неключевая колонка зависит от всего ключа, а не от его части.

Функциональные зависимости:
- `UserEmail -> UserName`
- `TaskId -> TaskTitle, ProjectName, ProjectOwnerEmail`
- `(EntryDate, UserEmail, TaskId) -> Hours`

Нарушение: все три зависимости частичные, ни одна неключевая колонка не зависит от `Activity`. Например, `UserName` определяется одним `UserEmail`, а `Hours` не зависит от вида работы.

Решение: делим таблицу по зависимостям.
- `Users (UserEmail PK, UserName)`
- `Tasks (TaskId PK, TaskTitle, ProjectName, ProjectOwnerEmail)`
- `TimeEntries (EntryDate, UserEmail, TaskId, Hours)`, PK `(EntryDate, UserEmail, TaskId)`
- `Activities (Activity PK)`
- `TimeEntryActivities (EntryDate, UserEmail, TaskId, Activity)`, PK из всех четырёх колонок

Запись времени и вид работы связаны как N:M: в одной записи может быть несколько видов работы, а один вид работы встречается во многих записях. Поэтому нужна связующая таблица.

## 3НФ

Правило: таблица во 2НФ, и нет транзитивных зависимостей (неключевая колонка не зависит от другой неключевой).

Функциональные зависимости в `Tasks`:
- `TaskId -> ProjectName`
- `ProjectName -> ProjectOwnerEmail`

Нарушение: `ProjectOwnerEmail` зависит от `TaskId` только через `ProjectName`. Владелец описывает проект, а не задачу.

Решение: выносим проекты в отдельную таблицу. Владелец проекта это пользователь, поэтому вместо email храним ссылку на `Users`.
- `Projects (ProjectName PK, OwnerEmail FK -> Users)`
- `Tasks (TaskId PK, TaskTitle, ProjectName FK -> Projects)`

Остальные таблицы транзитивных зависимостей не имеют.

## Итоговые таблицы

Естественные ключи (email, название проекта, название вида работы, тройка «задача, пользователь, дата») заменены суррогатными `INT IDENTITY`, а их уникальность сохранена через `UNIQUE`. Так переименование проекта или смена email это `UPDATE` одной строки.

Таблицы `Users`, `Projects`, `Tasks` уже есть в базе TaskFlowLab (01-schema.sql) и переиспользуются. Новые таблицы создаются в `06-homework.sql`.

| Таблица | Колонки | Ключи |
|---|---|---|
| Users (уже есть) | Id, Email, DisplayName, IsActive, CreatedAt | PK Id, UNIQUE Email |
| Projects (уже есть) | Id, Name, Description, OwnerId, CreatedAt | PK Id, UNIQUE Name, FK OwnerId -> Users |
| Tasks (уже есть) | Id, Title, ..., ProjectId, AssigneeId | PK Id, FK ProjectId -> Projects |
| TimeEntries | Id, TaskId, UserId, WorkDate, Hours, Note, CreatedAt | PK Id, FK TaskId -> Tasks, FK UserId -> Users, UNIQUE (TaskId, UserId, WorkDate) |
| Activities | Id, Name | PK Id, UNIQUE Name |
| TimeEntryActivities | TimeEntryId, ActivityId | PK (TimeEntryId, ActivityId), FK TimeEntryId -> TimeEntries, FK ActivityId -> Activities |

Соответствие колонок выгрузки: `EntryDate -> TimeEntries.WorkDate`, `UserEmail -> Users.Email` (через `TimeEntries.UserId`), `UserName -> Users.DisplayName`, `TaskId -> TimeEntries.TaskId`, `TaskTitle -> Tasks.Title`, `ProjectName -> Projects.Name`, `ProjectOwnerEmail -> Users.Email` (через `Projects.OwnerId`), `Hours -> TimeEntries.Hours`, `Activities -> Activities.Name` (через `TimeEntryActivities`).