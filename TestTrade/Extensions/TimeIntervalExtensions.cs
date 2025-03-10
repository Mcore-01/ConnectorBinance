using TestTrade.Enums;

namespace TestTrade.Extensions;

public static class TimeIntervalExtensions
{
    public static string ToStringInterval(this TimeInterval interval)
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
}
