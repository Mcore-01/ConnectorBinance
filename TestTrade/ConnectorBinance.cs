using Newtonsoft.Json;
using TestTrade.Interfaces;
using TestTrade.Models;

namespace TestTrade;

public class ConnectorBinance : ITestConnector
{
    public event Action<Trade> NewBuyTrade = null!;
    public event Action<Trade> NewSellTrade = null!;
    public event Action<Candle> CandleSeriesProcessing = null!;
   

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync($"https://api.binance.com/api/v3/trades?symbol={pair}&limit={maxCount}");

            var content = await response.Content.ReadAsStringAsync();

            var trades = JsonConvert.DeserializeObject<List<Trade>>(content);

            foreach (var trade in trades)
            {
                trade.Pair = pair;
            }

            return trades;
        }
    }

    public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        throw new NotImplementedException();
    }

    public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
    {
        throw new NotImplementedException();
    }

    public void SubscribeTrades(string pair, int maxCount = 100)
    {
        throw new NotImplementedException();
    }

    public void UnsubscribeCandles(string pair)
    {
        throw new NotImplementedException();
    }

    public void UnsubscribeTrades(string pair)
    {
        throw new NotImplementedException();
    }
}