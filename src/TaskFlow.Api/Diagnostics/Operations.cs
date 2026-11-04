namespace TaskFlow.Api.Diagnostics;

public sealed class TransientOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class ScopedOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class SingletonOperation
{
    public Guid Id { get; } = Guid.NewGuid();
}

public sealed class OperationReporter(
    TransientOperation transient,
    ScopedOperation scoped,
    SingletonOperation singleton)
{
    public OperationIds Report() => new(transient.Id, scoped.Id, singleton.Id);
}

public sealed record OperationIds(Guid Transient, Guid Scoped, Guid Singleton);

public sealed record LifetimesReport(OperationIds Controller, OperationIds Reporter);
