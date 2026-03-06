using Rpa.Domain.Exchange;

namespace Rpa.Domain.Abstractions;

public interface IExchangeRateRepository
{
    Task AddAsync(ExchangeRate exchangeRate, CancellationToken ct);
}