namespace Rpa.Worker.Options;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";
    public int IntervalSeconds { get; init; } = 60;
}