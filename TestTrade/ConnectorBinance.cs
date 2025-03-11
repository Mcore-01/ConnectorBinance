using Newtonsoft.Json;
using WebSocketSharp;
using TestTrade.Enums;
using TestTrade.Interfaces;
using TestTrade.Models;
using TestTrade.ResponseModels;
using TestTrade.Extensions;
using System.Text;

namespace TestTrade;

public class ConnectorBinance : ITestConnector
{
    private readonly HttpClient _httpClient;
    private readonly WebSocket _webSocket;

    private int messageId = 1;

    public ConnectorBinance()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://api.binance.com/api/v3/");

        _webSocket = new WebSocket("wss://stream.binance.com:443/ws");
        _webSocket.OnMessage += OnMessage; 
    }

    /// /// <summary>
    /// Retrieves the latest trade data for a currency pair.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    /// <param name="maxCount">The number of trades.</param>
    /// <returns>A list of the latest trades.</returns>
    /// <exception cref="HttpRequestException">Thrown if invalid parameters are provided or the API is unresponsive.</exception>
    /// <exception cref="JsonException">Thrown if the API returns null.</exception>
    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var response = await _httpClient.GetAsync($"trades?symbol={pair}&limit={maxCount}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var tradesResponse = JsonConvert.DeserializeObject<List<TradeRestResponse>>(content)
            ?? throw new JsonException("Failed to deserialize tradesResponse");

        return tradesResponse.Select(trade => trade.ConvertToTrade(pair));
    }

    /// <summary>
    /// Retrieves candlestick data based on the specified parameters.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    /// <param name="interval">The candlestick interval.</param>
    /// <param name="from">The start time of the candlesticks.</param>
    /// <param name="to">The end time of the candlesticks.</param>
    /// <param name="count">The number of candlesticks.</param>
    /// <returns>A list of candlesticks based on the specified parameters.</returns>
    /// <exception cref="HttpRequestException">Thrown if invalid parameters are provided or the API is unresponsive.</exception>
    /// <exception cref="JsonException">Thrown if the API returns null.</exception>
    /// /// <exception cref="InvalidCastException">Thrown if the API returns invalid data.</exception>
    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, TimeInterval interval, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        var uri = new StringBuilder();
        uri.Append($"klines?symbol={pair}&interval={interval.ToStringInterval()}&limit={count}");
        if (from is not null)
            uri.Append($"&startTime={(long)(from.Value - DateTime.UnixEpoch).TotalMilliseconds}");
        if (to is not null)
            uri.Append($"&endTime={(long)(to.Value - DateTime.UnixEpoch).TotalMilliseconds}");


        var response = await _httpClient.GetAsync(uri.ToString());

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var candlesResponse = JsonConvert.DeserializeObject<List<List<object>>>(content) 
            ?? throw new JsonException("Failed to deserialize candleResponse");

        return candlesResponse
            .Select(arr => CandleRestResponse.CreateCandleRestResponse(arr).ConvertToCandle(pair));
    }

    /// <summary>
    /// Event triggered when purchasing the subscribed currency.
    /// </summary>
    public event Action<Trade> NewBuyTrade;
    /// <summary>
    /// Event triggered when selling the subscribed currency.
    /// </summary>
    public event Action<Trade> NewSellTrade;
    
    private void OnMessage(object? sender, MessageEventArgs e)
    {
        string messageWSData = e.Data;
        
        if (string.IsNullOrEmpty(messageWSData))
            return;

        if (messageWSData.Contains("kline")) 
        {
            var candleResponse = JsonConvert.DeserializeObject<CandleWebSocketResponse>(messageWSData);

            if (candleResponse is null)
                return;

            var candle = candleResponse.ConvertToCandle();

            CandleSeriesProcessing?.Invoke(candle);
        }

        if (messageWSData.Contains("trade"))
        {
            var tradeResponse = JsonConvert.DeserializeObject<TradeWebSocketResponse>(messageWSData);

            if (tradeResponse is null)
                return;

            var trade = tradeResponse.ConvertToTrade();

            if (trade.Side == "True")
                NewSellTrade?.Invoke(trade);
            else 
                NewBuyTrade?.Invoke(trade);
        }
    }

    /// <summary>
    /// Subscribes to trades for the specified currency pair.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    public void SubscribeTrades(string pair)
    {
        if (!_webSocket.IsAlive)
        {
            _webSocket.Connect();
        }

        var parameters = new string[] { $"{pair.ToLower()}@trade" };

        SentMessageWebSocket("SUBSCRIBE", parameters, messageId++);
    }

    /// <summary>
    /// Unsubscribes from trades for the specified currency pair.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    public void UnsubscribeTrades(string pair)
    {
        var parameters = new string[] { $"{pair.ToLower()}@trade" };

        SentMessageWebSocket("UNSUBSCRIBE", parameters, messageId++);
    }

    /// <summary>
    /// Triggered when new candlestick data is received from the API.
    /// </summary>
    public event Action<Candle> CandleSeriesProcessing;

    /// <summary>
    /// Subscribes to candlestick updates for the specified currency pair with the given interval.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    /// <param name="interval">The candlestick interval.</param>
    public void SubscribeCandles(string pair, TimeInterval interval)
    {
        if (!_webSocket.IsAlive)
        {
            _webSocket.Connect();
        }

        var parameters = new string[] { $"{pair.ToLower()}@kline_{interval.ToStringInterval()}" };

        SentMessageWebSocket("SUBSCRIBE", parameters, messageId++);
    }

    /// <summary>
    /// Unsubscribes from candlestick updates for the specified currency pair with the given interval.
    /// </summary>
    /// <param name="pair">The currency pair.</param>
    /// <param name="interval">The candlestick interval.</param>
    public void UnsubscribeCandles(string pair, TimeInterval interval)
    {
        var parameters = new string[] { $"{pair.ToLower()}@kline_{interval.ToStringInterval()}" };

        SentMessageWebSocket("UNSUBSCRIBE", parameters, messageId++);
    }

    private void SentMessageWebSocket(string method, string[] parameters, int id)
    {
        var data = new
        {
            method = method,
            @params = parameters,
            id = id,
        };
        var dataJson = JsonConvert.SerializeObject(data);

        _webSocket.Send(dataJson);
    }
}