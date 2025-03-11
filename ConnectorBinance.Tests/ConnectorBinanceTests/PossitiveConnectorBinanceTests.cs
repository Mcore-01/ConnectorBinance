using TestTrade;
using TestTrade.Enums;
using Xunit;

namespace ConnectorBinanceLibrary.Tests.ConnectorBinanceTests;

public class PossitiveConnectorBinanceTests
{
    [Fact]
    public async Task ValidData_ReceivingTrades_ReturnCorrectTradeRecords()
    {
        var connector = new ConnectorBinance();

        var trades = await connector.GetNewTradesAsync("BTCUSDT", 5);

        Assert.NotEmpty(trades);
        Assert.Equal(5, trades.Count());
    }

    [Fact]
    public async Task ValidData_ReceivingCandles_ReturnCorrectCandleRecords()
    {
        var connector = new ConnectorBinance();

        var candles = await connector.GetCandleSeriesAsync("BTCUSDT", TimeInterval.OneHour, DateTimeOffset.UtcNow.AddDays(-1), null, 5);

        Assert.NotEmpty(candles);
        Assert.Equal(5, candles.Count());
    }

    [Fact]
    public async Task CorrectCurrency_ReceivingTickers_ReturnCorrectCurrencyRecords()
    {
        var currency = "BTC";
        var expectedSymbol = currency + "USDT";
        var connector = new ConnectorBinance();

        var tickers = await connector.GetTickers(currency);

        Assert.NotEmpty(tickers);
        Assert.Contains(tickers, ticker => ticker.Symbol == expectedSymbol);
    }
}