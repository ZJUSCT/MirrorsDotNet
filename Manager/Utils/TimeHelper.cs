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
    
    public static TimeSpan ParseTimeSpan(string input)
    {
        // Define the format for parsing
        string[] formats = { @"h\h", @"m\m", @"s\s" }; // Customize this array to include more formats if needed
        
        // Parse the input string
        var timeSpan = TimeSpan.Zero;
        foreach (var format in formats)
        {
            try
            {
                timeSpan = TimeSpan.ParseExact(input, format, null);
                break; // If successfully parsed, exit the loop
            }
            catch (FormatException)
            {
                // Format not matched, continue to the next format
            }
        }

        return timeSpan;
    }
}
