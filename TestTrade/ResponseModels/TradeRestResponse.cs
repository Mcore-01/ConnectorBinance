using TestTrade.Models;

namespace TestTrade.ResponseModels;

public class TradeRestResponse
{
    public string Id { get; set; }
    public decimal Price { get; set; }
    public decimal Qty { get; set; }
    public decimal QuoteQty { get; set; }
    public long Time { get; set; }
    public required string IsBuyerMaker { get; set; }
    public required string IsBestMatch { get; set; }

    public DateTimeOffset GetTimeAsDateTimeOffset()
    {
        return DateTime.UnixEpoch.AddMilliseconds(Time);
    }
            
    public Trade ConvertToTrade(string pair)
    {
        return new Trade()
        {
            Id = Id,
            Pair = pair,
            Price = Price,
            Amount = Qty,
            Side = IsBuyerMaker,
            Time = GetTimeAsDateTimeOffset(),
        };
    }
}