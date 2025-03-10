using TestTrade.Enums;
using TestTrade.Models;

namespace TestTrade.Interfaces;

interface ITestConnector
{
    #region Rest

    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
    Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, TimeInterval interval, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);

    #endregion

    #region Socket


    event Action<Trade> NewBuyTrade;
    event Action<Trade> NewSellTrade;
    void SubscribeTrades(string pair);
    void UnsubscribeTrades(string pair);

    event Action<Candle> CandleSeriesProcessing;
    void SubscribeCandles(string pair, TimeInterval interval);
    void UnsubscribeCandles(string pair, TimeInterval interval);

    #endregion
}