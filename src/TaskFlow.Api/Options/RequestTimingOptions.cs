using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Options;

public sealed class RequestTimingOptions
{
    public const string SectionName = "RequestTiming";

    [Range(1, 60000)]
    public int SlowRequestThresholdMs { get; set; } = 500;

    public bool AddResponseHeader { get; set; } = true;
}
