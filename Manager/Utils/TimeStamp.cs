using System;

namespace Manager.Utils;

public class TimeStamp
{
    public static int UnixTimeStamp()
    {
        return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}