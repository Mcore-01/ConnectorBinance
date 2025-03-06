using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TestTrade.Utilities;

public class UnixTimeToDateTimeOffSetConvertor : DateTimeConverterBase
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        DateTimeOffset UnixEpoch = DateTime.UnixEpoch;

        return UnixEpoch.AddMilliseconds((long)reader.Value!);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is DateTimeOffset dateTimeOffset)
        {
            long milliseconds = (long)(dateTimeOffset - DateTime.UnixEpoch).TotalMilliseconds;
            writer.WriteValue(milliseconds);
        }
    }
}