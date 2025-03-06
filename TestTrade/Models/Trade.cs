using Newtonsoft.Json;
using TestTrade.Utilities;

namespace TestTrade.Models;

public class Trade
{
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Цена трейда
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Объем трейда
    /// </summary>
    [JsonProperty("qty")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Направление (buy/sell)
    /// </summary>
    [JsonProperty("isBuyerMaker")]
    public string Side { get; set; }

    /// <summary>
    /// Время трейда
    /// </summary>
    [JsonConverter(typeof(UnixTimeToDateTimeOffSetConvertor))]
    public DateTimeOffset Time { get; set; }

    /// <summary>
    /// Id трейда
    /// </summary>
    public string Id { get; set; }
}