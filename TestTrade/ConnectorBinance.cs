using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using TestTrade.Enums;
using TestTrade.Interfaces;
using TestTrade.Models;

namespace TestTrade;

public class ConnectorBinance : ITestConnector
{
    private readonly HttpClient _httpClient;

    public ConnectorBinance()
    {
        _httpClient = new HttpClient();
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
    {
        var response = await _httpClient.GetAsync($"https://api.binance.com/api/v3/trades?symbol={pair}&limit={maxCount}");

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
        string uri = $"https://api.binance.com/api/v3/klines?symbol={pair}&interval={ToStringInterval(interval)}&limit={count}";

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


    public event Action<Trade> NewBuyTrade = null!;
    public event Action<Trade> NewSellTrade = null!;
    
    public void SubscribeTrades(string pair, int maxCount = 100)
    {
        throw new NotImplementedException();
    }

    public void UnsubscribeTrades(string pair)
    {
        throw new NotImplementedException();
    }

    public event Action<Candle> CandleSeriesProcessing = null!;

    public void SubscribeCandles(string pair, TimeInterval interval, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
    {
        throw new NotImplementedException();
    }

    public void UnsubscribeCandles(string pair)
    {
        throw new NotImplementedException();
    }
}