using TestTrade;
using TestTrade.Enums;

var connector = new ConnectorBinance();

connector.CandleSeriesProcessing += (candle) => Console.WriteLine(candle.ToString());

Console.WriteLine("Начало подписки:");
var from = DateTime.UnixEpoch.AddMicroseconds(1741352922000);


connector.SubscribeCandles("BTCUSDT", TimeInterval.OneMinute);

await Task.Delay(12000);

connector.UnsubscribeCandles("BTCUSDT", TimeInterval.OneHour);

Console.ReadKey();