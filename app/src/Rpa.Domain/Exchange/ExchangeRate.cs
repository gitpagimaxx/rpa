namespace Rpa.Domain.Exchange;

public sealed class ExchangeRate
{
    public Guid Id { get; }
    public Currency BaseCurrency { get; }
    public Currency QuoteCurrency { get; }
    public decimal Rate { get; }
    public DateTimeOffset CollectedAtUtc { get; }
    public DataSource Source { get; }

    private ExchangeRate(Guid id, Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTimeOffset collectedAtUtc, DataSource source)
    {
        if (rate <= 0) throw new ArgumentOutOfRangeException(nameof(rate));
        Id = id;
        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
        Rate = rate;
        CollectedAtUtc = collectedAtUtc;
        Source = source;
    }

    public static ExchangeRate Create(Currency baseCurrency, Currency quoteCurrency, decimal rate, DateTimeOffset collectedAtUtc, DataSource source)
        => new(Guid.NewGuid(), baseCurrency, quoteCurrency, rate, collectedAtUtc, source);
}