using Newtonsoft.Json;
using TestTrade.Models;

namespace TestTrade.ResponseModels;

public class TradeWebSocketResponse
{
    [JsonProperty("e")]
    public string EventType { get; set; }

    [JsonProperty("E")]
    public long EventTime { get; set; }

    [JsonProperty("s")]
    public string Symbol { get; set; }

    [JsonProperty("t")]
    public string TradeId { get; set; }

    [JsonProperty("p")]
    public decimal Price { get; set; }

    [JsonProperty("q")]
    public decimal Quantity { get; set; }

    [JsonProperty("T")]
    public long TradeTime { get; set; }

    [JsonProperty("m")]
    public string IsBuyerMaker { get; set; }

    [JsonProperty("M")]
    public string Ignore { get; set; }

    public DateTimeOffset GetEventTimeAsDateTimeOffset()
    {
        return DateTime.UnixEpoch.AddMilliseconds(EventTime);
    }

    public Trade ConvertToTrade()
    {
        return new Trade()
        {
            Id = TradeId,
            Pair = Symbol,
            Price = Price,
            Amount = Quantity,
            Side = IsBuyerMaker,
            Time = GetEventTimeAsDateTimeOffset(),
        };
    }
}