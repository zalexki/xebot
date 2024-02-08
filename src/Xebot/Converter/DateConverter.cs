using System;

namespace Xebot.Converter;

public static class DateConverter
{
    public static string ConvertSecondsToHumanHourReadable(ulong seconds)
    {
        return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
    }

    public static DateTime ConvertUtcToParisTime(DateTime utcDate)
    {
        TimeZoneInfo parisTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisTimeZone);
    }
}
