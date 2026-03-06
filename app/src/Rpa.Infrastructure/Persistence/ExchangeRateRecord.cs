namespace Rpa.Infrastructure.Persistence;

public sealed class ExchangeRateRecord
{
    public Guid Id { get; set; }

    public string BaseCurrency { get; set; } = default!;
    public string QuoteCurrency { get; set; } = default!;
    public decimal Rate { get; set; }
    public DateTimeOffset CollectedAtUtc { get; set; }
    public string Source { get; set; } = default!;
}