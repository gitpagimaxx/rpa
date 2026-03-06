namespace Rpa.Domain.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}