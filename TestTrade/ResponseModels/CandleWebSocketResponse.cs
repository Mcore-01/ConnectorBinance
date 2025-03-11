using Newtonsoft.Json;
using TestTrade.Models;

namespace TestTrade.ResponseModels;

public class CandleWebSocketResponse
{
    [JsonProperty("e")]
    public string EventType { get; set; }

    [JsonProperty("E")]
    public long EventTime { get; set; }

    [JsonProperty("s")]
    public string Symbol { get; set; }

    [JsonProperty("k")]
    public CandleDataWebSocketResponse Candle { get; set; }

    public Candle ConvertToCandle()
    {
        return new Candle()
        {
            Pair = Symbol,
            OpenPrice = Candle.OpenPrice,
            HighPrice = Candle.HighPrice,
            LowPrice = Candle.LowPrice,
            ClosePrice = Candle.ClosePrice,
            TotalPrice = Candle.QuoteAssetVolume,
            TotalVolume = Candle.Volume,
            OpenTime = Candle.GetOpenTimeAsDateTimeOffset(),
        };
    }
}