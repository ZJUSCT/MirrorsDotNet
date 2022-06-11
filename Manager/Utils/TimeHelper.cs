using System;

namespace Manager.Utils;

public static class TimeHelper
{
    public static int UnixTimeStamp()
    {
        return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static int DataTime2TimeStamp(DateTime time)
    {
        return (int)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }

    public static DateTime TimeStamp2DateTime(int timeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1).AddSeconds(timeStamp);
        return dateTime;
    }
}
