using System;
using System.Globalization;


public static class DateUtils
{
    public static long? ConvertUTCStringToTsMilliseconds(string timeText, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        if (timeText == null || timeText.Trim() == "") return null;

        // Specify that the input time is in UTC
        var ts = DateTime.ParseExact(timeText, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        return new DateTimeOffset(ts).ToUnixTimeMilliseconds();
    }

    public static string ConvertMillisecondsToUTCString(long? millisec, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        if (millisec == null) return null;
        return DateTimeOffset.FromUnixTimeMilliseconds((long)millisec).UtcDateTime.ToString(dateFormat);
    }

    public static string ConvertMillisecondsToLocalString(long? millisec, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        if (millisec == null) return null;


        return DateTimeOffset.FromUnixTimeMilliseconds((long)millisec).ToLocalTime().ToString(dateFormat);

    }

    public static string ConvertUTCToLocalString(DateTime dateTime, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToLocalTime().ToString(dateFormat);
    }

    public static DateTime? ConvertUTCStringToUTCDate(string timeText, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        if (timeText == null || timeText.Trim() == "") return null;

        // Specify that the input time is in UTC
        return DateTime.ParseExact(timeText, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

    }

    public static DateTime? ConvertUTCStringToUTCDate(string timeText, string[] dateFormats = null)
    {
        if (string.IsNullOrWhiteSpace(timeText)) return null;

        // Define the date formats, including one with milliseconds
        dateFormats = dateFormats ?? new string[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss" };

        // Try to parse the date with the specified formats
        DateTime parsedDate;
        if (DateTime.TryParseExact(timeText, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsedDate))
        {
            return parsedDate.ToUniversalTime();
        }

        // If parsing fails, return null
        return null;
    }

    public static string ConvertUTCDateToUTCString(DateTime dateTime, string dateFormat = "yyyy-MM-dd HH:mm:ss")
    {
        return dateTime.ToString(dateFormat);
    }



}
