using System;
using System.Globalization;


public static class DateUtils
{
    public static long? ConvertStringToTsMilliseconds(string timeText)
    {
        if (timeText == null || timeText.Trim() == "") return null;

        var ts = DateTime.ParseExact(timeText, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        return new DateTimeOffset(ts).ToUnixTimeMilliseconds();
    }

    public static string ConvertMillisecondsToString(long? millisec)
    {
        if (millisec == null) return null;
        return DateTimeOffset.FromUnixTimeMilliseconds((long)millisec).UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }



}
