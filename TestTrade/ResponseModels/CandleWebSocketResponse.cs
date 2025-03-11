using Newtonsoft.Json;

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
    public CandleData Candle { get; set; }
}

public class CandleData
{
    [JsonProperty("t")]
    public long OpenTime { get; set; }

    [JsonProperty("T")]
    public long CloseTime { get; set; }

    [JsonProperty("s")]
    public string Symbol { get; set; }

    [JsonProperty("i")]
    public string Interval { get; set; }

    [JsonProperty("f")]
    public long FirstTradeId { get; set; }

    [JsonProperty("L")]
    public long LastTradeId { get; set; }

    [JsonProperty("o")]
    public decimal OpenPrice { get; set; }

    [JsonProperty("c")]
    public decimal ClosePrice { get; set; }

    [JsonProperty("h")]
    public decimal HighPrice { get; set; }

    [JsonProperty("l")]
    public decimal LowPrice { get; set; }

    [JsonProperty("v")]
    public decimal Volume { get; set; }

    [JsonProperty("n")]
    public int NumberOfTrades { get; set; }

    [JsonProperty("x")]
    public string IsFinal { get; set; }

    [JsonProperty("q")]
    public decimal QuoteAssetVolume { get; set; }

    [JsonProperty("V")]
    public decimal TakerBuyBaseAssetVolume { get; set; }

    [JsonProperty("Q")]
    public decimal TakerBuyQuoteAssetVolume { get; set; }

    [JsonProperty("B")]
    public string Ignore { get; set; }
}