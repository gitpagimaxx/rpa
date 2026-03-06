using Rpa.Domain.Exchange;

namespace Rpa.Domain.Abstractions;

public interface IExchangeRateRepository
{
    Task AddAsync(ExchangeRate rate, CancellationToken ct);
}