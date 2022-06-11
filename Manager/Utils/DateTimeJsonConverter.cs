using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Manager.Utils;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        TimeHelper.TimeStamp2DateTime(int.Parse(reader.GetString()!));

    public override void Write(
        Utf8JsonWriter writer,
        DateTime dateTimeValue,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(TimeHelper.DataTime2TimeStamp(dateTimeValue).ToString());
}