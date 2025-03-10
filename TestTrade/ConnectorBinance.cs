using Newtonsoft.Json;
using System.Globalization;
using WebSocketSharp;
using TestTrade.Enums;
using TestTrade.Interfaces;
using TestTrade.Models;
using TestTrade.ResponseModels;

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
        _webSocket.Connect();
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var response = await _httpClient.GetAsync($"trades?symbol={pair}&limit={maxCount}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Http error with code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var trades = JsonConvert.DeserializeObject<List<Trade>>(content);

        foreach (var trade in trades)
        {
            trade.Pair = pair;
        }

        return trades;
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, TimeInterval interval, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        string uri = $"klines?symbol={pair}&interval={ToStringInterval(interval)}&limit={count}";

        if (from is not null)
            uri += $"&startTime={(long)(from.Value - DateTime.UnixEpoch).TotalMilliseconds}";
        if (to is not null)
            uri += $"&endTime={(long)(to.Value - DateTime.UnixEpoch).TotalMilliseconds}";


        var response = await _httpClient.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Http error with code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var candlesObj = JsonConvert.DeserializeObject<List<List<object>>>(content);
        var candles = new List<Candle>();

        foreach (var candleObj in candlesObj)
        {
            Candle currentCandle = CreateNewCandle(pair, candleObj);

            candles.Add(currentCandle);
        }

        return candles;
    }

    private static Candle CreateNewCandle(string pair, List<object> candleObj)
    {
        var currentCandle = new Candle()
        {
            Pair = pair,
            OpenPrice = Convert.ToDecimal(candleObj[1], CultureInfo.InvariantCulture),
            HighPrice = Convert.ToDecimal(candleObj[2], CultureInfo.InvariantCulture),
            LowPrice = Convert.ToDecimal(candleObj[3], CultureInfo.InvariantCulture),
            ClosePrice = Convert.ToDecimal(candleObj[4], CultureInfo.InvariantCulture),
            TotalPrice = Convert.ToDecimal(candleObj[7], CultureInfo.InvariantCulture),
            TotalVolume = Convert.ToDecimal(candleObj[5], CultureInfo.InvariantCulture),
            OpenTime = DateTime.UnixEpoch.AddMilliseconds((long)candleObj[0]),
        };

        return currentCandle;
    }

    public static string ToStringInterval(TimeInterval interval)
    {
        return interval switch
        {
            TimeInterval.OneSecond => "1s",
            TimeInterval.OneMinute => "1m",
            TimeInterval.ThreeMinutes => "3m",
            TimeInterval.FiveMinutes => "5m",
            TimeInterval.FifteenMinutes => "15m",
            TimeInterval.ThirtyMinutes => "30m",
            TimeInterval.OneHour => "1h",
            TimeInterval.TwoHours => "2h",
            TimeInterval.FourHours => "4h",
            TimeInterval.SixHours => "6h",
            TimeInterval.EightHours => "8h",
            TimeInterval.TwelveHours => "12h",
            TimeInterval.OneDay => "1d",
            TimeInterval.ThreeDays => "3d",
            TimeInterval.OneWeek => "1w",
            TimeInterval.OneMonth => "1M",
            _ => "1s"
        };
    }


    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    
    private void OnMessage(object sender, MessageEventArgs e)
    {
        string data = e.Data;

        if (string.IsNullOrEmpty(data)) {
            return;
        }

        if (data.Contains("kline")) 
        {
            var candleResponse = JsonConvert.DeserializeObject<CandleResponse>(data);

            var candle = CreateCandle(candleResponse);

            CandleSeriesProcessing?.Invoke(candle);
        }

        if (data.Contains("trade"))
        {
            var tradeResponse = JsonConvert.DeserializeObject<TradeResponse>(data);

            var trade = CreateTrade(tradeResponse);

            if (trade.Side == "True")
            {
                NewSellTrade?.Invoke(trade);
                return;
            }

            NewBuyTrade?.Invoke(trade);
        }
    }

    private Trade CreateTrade(TradeResponse tradeResponse)
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

    private Candle CreateCandle(CandleResponse candleResponse)
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
        var data = new
        {
            method = "SUBSCRIBE",
            @params = new string[] { $"{pair.ToLower()}@trade" },
            id = messageId++,
        };

        var dataJson = JsonConvert.SerializeObject(data);

        _webSocket.Send(dataJson);
    }

    public void UnsubscribeTrades(string pair)
    {
        var data = new
        {
            method = "UNSUBSCRIBE",
            @params = new string[] { $"{pair.ToLower()}@trade" },
            id = messageId++,
        };

        var dataJson = JsonConvert.SerializeObject(data);

        _webSocket.Send(dataJson);
    }

    public event Action<Candle> CandleSeriesProcessing;

    public void SubscribeCandles(string pair, TimeInterval interval)
    {
        var data = new
        {
            method = "SUBSCRIBE",
            @params = new string[] { $"{pair.ToLower()}@kline_{ToStringInterval(interval)}" },
            id = messageId++,
        };

        var dataJson = JsonConvert.SerializeObject(data);

        _webSocket.Send(dataJson);
    }

    public void UnsubscribeCandles(string pair, TimeInterval interval)
    {
        var data = new
        {
            method = "UNSUBSCRIBE",
            @params = new string[] { $"{pair.ToLower()}@kline_{ToStringInterval(interval)}" },
            id = messageId++,
        };

        var dataJson = JsonConvert.SerializeObject(data);

        _webSocket.Send(dataJson);
    }
}