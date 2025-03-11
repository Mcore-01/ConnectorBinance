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
        _webSocket.OnMessage += OnMessage; ;
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

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    
    private void OnMessage(object? sender, MessageEventArgs e)
    {
        string data = e.Data;

        if (string.IsNullOrEmpty(data)) {
            return;
        }

        if (data.Contains("kline")) 
        {
            var candleResponse = JsonConvert.DeserializeObject<CandleWebSocketResponse>(data);

            var candle = CreateCandle(candleResponse);

            CandleSeriesProcessing?.Invoke(candle);
        }

        if (data.Contains("trade"))
        {
            var tradeResponse = JsonConvert.DeserializeObject<TradeWebSocketResponse>(data);

            var trade = CreateTrade(tradeResponse);

            if (trade.Side == "True")
            {
                NewSellTrade?.Invoke(trade);
                return;
            }

            NewBuyTrade?.Invoke(trade);
        }
    }

    private Trade CreateTrade(TradeWebSocketResponse tradeResponse)
    {
        return new Trade()
        {
            Id = tradeResponse.TradeId,
            Pair = tradeResponse.Symbol,
            Price = tradeResponse.Price,
            Amount = tradeResponse.Quantity,
            Side = tradeResponse.IsBuyerMaker,
            Time = DateTime.UnixEpoch.AddMilliseconds(tradeResponse.EventTime),
        };
    }

    private Candle CreateCandle(CandleWebSocketResponse candleResponse)
    {
        var candleData = candleResponse.Candle;

        return new Candle()
        {
            Pair = candleResponse.Symbol,
            OpenPrice = candleData.OpenPrice,
            HighPrice = candleData.HighPrice,
            LowPrice = candleData.LowPrice,
            ClosePrice = candleData.ClosePrice,
            TotalPrice = candleData.QuoteAssetVolume,
            TotalVolume = candleData.Volume,
            OpenTime = DateTime.UnixEpoch.AddMilliseconds(candleData.OpenTime),
        };
    }

    public void SubscribeTrades(string pair)
    {
        if (!_webSocket.IsAlive)
        {
            _webSocket.Connect();
        }

        var parameters = new string[] { $"{pair.ToLower()}@trade" };

        SentMessageWebSocket("SUBSCRIBE", parameters, messageId++);
    }

    public void UnsubscribeTrades(string pair)
    {
        var parameters = new string[] { $"{pair.ToLower()}@trade" };

        SentMessageWebSocket("UNSUBSCRIBE", parameters, messageId++);
    }

    public event Action<Candle> CandleSeriesProcessing;

    public void SubscribeCandles(string pair, TimeInterval interval)
    {
        if (!_webSocket.IsAlive)
        {
            _webSocket.Connect();
        }

        var parameters = new string[] { $"{pair.ToLower()}@kline_{interval.ToStringInterval()}" };

        SentMessageWebSocket("SUBSCRIBE", parameters, messageId++);
    }

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