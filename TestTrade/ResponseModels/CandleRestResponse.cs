using System.Globalization;
using TestTrade.Models;

namespace TestTrade.ResponseModels;

public class CandleRestResponse
{
    public long OpenTime { get; set; }          
    public decimal OpenPrice { get; set; }         
    public decimal HighPrice { get; set; }          
    public decimal LowPrice { get; set; }             
    public decimal ClosePrice { get; set; }           
    public decimal Volume { get; set; }          
    public long CloseTime { get; set; }         
    public decimal QuoteAssetVolume { get; set; }
    public int NumberOfTrades { get; set; }     
    public decimal TakerBuyBaseAssetVolume { get; set; }  
    public decimal TakerBuyQuoteAssetVolume { get; set; } 
    public string Ignore { get; set; }

    public DateTimeOffset GetOpenTimeAsDateTimeOffset()
    {
        return DateTime.UnixEpoch.AddMilliseconds(OpenTime);
    }

    public static CandleRestResponse CreateCandleRestResponse(List<object> values)
    {
        try
        {
            return new CandleRestResponse()
            {
                OpenTime = (long)(values[0]),
                OpenPrice = Convert.ToDecimal(values[1], CultureInfo.InvariantCulture),
                HighPrice = Convert.ToDecimal(values[2], CultureInfo.InvariantCulture),
                LowPrice = Convert.ToDecimal(values[3], CultureInfo.InvariantCulture),
                ClosePrice = Convert.ToDecimal(values[4], CultureInfo.InvariantCulture),
                Volume = Convert.ToDecimal(values[5], CultureInfo.InvariantCulture),
                CloseTime = (long)values[6],
                QuoteAssetVolume = Convert.ToDecimal(values[7], CultureInfo.InvariantCulture),
                NumberOfTrades = Convert.ToInt32(values[8]),
                TakerBuyBaseAssetVolume = Convert.ToDecimal(values[9], CultureInfo.InvariantCulture),
                TakerBuyQuoteAssetVolume = Convert.ToDecimal(values[10], CultureInfo.InvariantCulture),
                Ignore = values[11].ToString()
            };
        }
        catch (Exception e)
        {
            throw new InvalidCastException($"Failed to convert array to class object. Error message: {e.Message}");            
        }
    }

    public Candle ConvertToCandle(string pair)
    {
        return new Candle()
        {
            Pair = pair,
            OpenPrice = OpenPrice,
            HighPrice = HighPrice,
            LowPrice = LowPrice,
            ClosePrice = ClosePrice,
            TotalPrice = QuoteAssetVolume,
            TotalVolume = Volume,
            OpenTime = GetOpenTimeAsDateTimeOffset(),
        };
    }
}