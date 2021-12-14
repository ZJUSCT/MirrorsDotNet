using System;

namespace Manager.Utils;

public class Convertor
{
    public static int DataTime2TimeStamp(DateTime time)
    {
        return (int)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}