using System;

namespace TournamentManager.ExtensionMethods;

public static class TimeSpanExtensions
{
    public static string ToShortTimeString(this TimeSpan timeSpan)
    {
        return TimeOnly.FromTimeSpan(timeSpan).ToShortTimeString();
    }

    public static string ToLongTimeString(this TimeSpan timeSpan)
    {
        return TimeOnly.FromTimeSpan(timeSpan).ToLongTimeString();
    }
}
