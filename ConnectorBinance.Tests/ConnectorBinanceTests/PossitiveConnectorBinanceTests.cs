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
        var from = DateTime.UnixEpoch.AddMicroseconds(1741352922000);

        var candles = await connector.GetCandleSeriesAsync("BTCUSDT", TimeInterval.OneHour, from, null, 5);

        Assert.NotEmpty(candles);
        Assert.Equal(5, candles.Count());
    }
}