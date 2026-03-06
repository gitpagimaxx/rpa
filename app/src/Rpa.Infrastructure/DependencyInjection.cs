using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Rpa.Domain.Abstractions;
using Rpa.Infrastructure.Persistence;
using Rpa.Infrastructure.Scraping.Wise;
using Rpa.Infrastructure.Time;

namespace Rpa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();

        services.AddOptions<PostgresOptions>()
            .BindConfiguration(PostgresOptions.SectionName)
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Postgres connection string is required.");

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddSingleton<DatabaseInitializer>();

        services.AddOptions<WiseOptions>().BindConfiguration(WiseOptions.SectionName);

        services.AddHttpClient("wise", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");
        })
            .AddPolicyHandler((request) =>
            {
                var jitter = new Random();
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => (int)msg.StatusCode == 429)
                    .WaitAndRetryAsync(
                        retryCount: 4,
                        sleepDurationProvider: attempt =>
                        {
                            var baseDelay = TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt - 1));
                            return baseDelay + TimeSpan.FromMilliseconds(jitter.Next(0, 250));
                        });
            });

        services.AddSingleton<IExchangeRateHtmlClient, WiseHtmlClient>();
        services.AddSingleton<IUsdBrlHtmlParser, WiseUsdBrlHtmlParser>();

        return services;
    }
}