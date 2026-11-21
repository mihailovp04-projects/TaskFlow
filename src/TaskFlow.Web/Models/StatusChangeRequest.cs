using TaskFlow.Domain.Enums;

namespace TaskFlow.Web.Models;

public sealed record StatusChangeRequest(int TaskId, TaskItemStatus NewStatus);