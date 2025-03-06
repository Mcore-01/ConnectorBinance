using TestTrade;
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
}