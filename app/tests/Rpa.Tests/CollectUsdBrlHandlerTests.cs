using FluentAssertions;
using NSubstitute;
using Rpa.Application.UseCases.CollectUsdBrl;
using Rpa.Domain.Abstractions;
using Rpa.Domain.Exchange;

namespace Rpa.Tests;

public sealed class CollectUsdBrlHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldPersistAndReturnEntity_WhenValid()
    {
        var htmlClient = Substitute.For<IExchangeRateHtmlClient>();
        var parser = Substitute.For<IUsdBrlHtmlParser>();
        var repo = Substitute.For<IExchangeRateRepository>();
        var clock = Substitute.For<IClock>();

        htmlClient.GetUsdBrlPageHtmlAsync(Arg.Any<CancellationToken>())
            .Returns("<html/>");

        parser.TryParseUsdBrl(Arg.Any<string>(), out Arg.Any<decimal>())
            .Returns(x =>
            {
                x[1] = 5.253m;
                return true;
            });

        var now = new DateTimeOffset(2026, 03, 08, 10, 00, 00, TimeSpan.Zero);
        clock.UtcNow.Returns(now);

        var handler = new CollectUsdBrlHandler(htmlClient, parser, repo, clock);

        var entity = await handler.HandleAsync(CancellationToken.None);

        entity.BaseCurrency.Should().Be(Currency.USD);
        entity.QuoteCurrency.Should().Be(Currency.BRL);
        entity.Rate.Should().Be(5.253m);
        entity.CollectedAtUtc.Should().Be(now);
        entity.Source.Should().Be(DataSource.From("wise.com"));

        await repo.Received(1).AddAsync(Arg.Any<ExchangeRate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenParserFails()
    {
        var htmlClient = Substitute.For<IExchangeRateHtmlClient>();
        var parser = Substitute.For<IUsdBrlHtmlParser>();
        var repo = Substitute.For<IExchangeRateRepository>();
        var clock = Substitute.For<IClock>();

        htmlClient.GetUsdBrlPageHtmlAsync(Arg.Any<CancellationToken>())
            .Returns("<html/>");

        parser.TryParseUsdBrl(Arg.Any<string>(), out Arg.Any<decimal>())
            .Returns(false);

        var handler = new CollectUsdBrlHandler(htmlClient, parser, repo, clock);

        var act = () => handler.HandleAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        await repo.DidNotReceive().AddAsync(Arg.Any<ExchangeRate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenRateOutOfRange()
    {
        var htmlClient = Substitute.For<IExchangeRateHtmlClient>();
        var parser = Substitute.For<IUsdBrlHtmlParser>();
        var repo = Substitute.For<IExchangeRateRepository>();
        var clock = Substitute.For<IClock>();

        htmlClient.GetUsdBrlPageHtmlAsync(Arg.Any<CancellationToken>())
            .Returns("<html/>");

        parser.TryParseUsdBrl(Arg.Any<string>(), out Arg.Any<decimal>())
            .Returns(x =>
            {
                x[1] = 999m; // fora do range
                return true;
            });

        var handler = new CollectUsdBrlHandler(htmlClient, parser, repo, clock);

        var act = () => handler.HandleAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        await repo.DidNotReceive().AddAsync(Arg.Any<ExchangeRate>(), Arg.Any<CancellationToken>());
    }
}