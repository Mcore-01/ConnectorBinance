using Newtonsoft.Json;

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
}