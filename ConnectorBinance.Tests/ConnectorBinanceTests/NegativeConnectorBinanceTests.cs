using TestTrade;
using TestTrade.Enums;
using Xunit;

namespace ConnectorBinanceLibrary.Tests.ConnectorBinanceTests;

public class NegativeConnectorBinanceTests
{
    [Fact]
    public async Task IncorrectPair_ReceivingTrades_Exception()
    {
        var connector = new ConnectorBinance();

        await Assert.ThrowsAsync<HttpRequestException>(async () => await connector.GetNewTradesAsync("BT1CUSDT", 1));
    }

    [Fact]
    public async Task IncorrectLimit_ReceivingTrades_Exception()
    {
        var connector = new ConnectorBinance();

        await Assert.ThrowsAsync<HttpRequestException>(async () => await connector.GetNewTradesAsync("BTCUSDT", -1));
    }

    [Fact]
    public async Task IncorrectPair_ReceivingCandles_Exception()
    {
        var connector = new ConnectorBinance();
        var from = DateTime.UnixEpoch.AddMicroseconds(1741352922000);

        await Assert.ThrowsAsync<HttpRequestException>(async () => await connector.GetCandleSeriesAsync("BTC1USDT", TimeInterval.OneHour, from, null, 5));
    }

    [Fact]
    public async Task FakeCurrency_ReceivingTickers_ReturnEmptyList()
    {
        var fakeCurrency = "AS132asDS";
        var connector = new ConnectorBinance();

        var tickers = await connector.GetTickers(fakeCurrency);

        Assert.Empty(tickers);
    }
}