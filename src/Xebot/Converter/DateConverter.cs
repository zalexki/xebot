using System;

namespace Xebot.Converter;

public static class DateConverter
{
    public static string ConvertSecondsToHumanHourReadable(ulong seconds)
    {
        return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
    }

    public static string ConvertUtcToParisTimeHumanReadable(DateTime utcDate)
    {
        TimeZoneInfo parisTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time");
        return TimeZoneInfo.ConvertTimeFromUtc(utcDate, parisTimeZone).ToString("dd/MM HH:mm:ss");
    }
}
