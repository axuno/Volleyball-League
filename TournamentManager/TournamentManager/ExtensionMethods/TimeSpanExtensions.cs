using System;
using System.Collections.Generic;
namespace TournamentManager.ExtensionMethods
{
    public static class TimeSpanExtensions
    {
        public static string ToShortTimeString(this TimeSpan timeSpan)
        {
            return DateTime.UtcNow.Date.Add(timeSpan).ToShortTimeString();
        }

        public static string ToLongTimeString(this TimeSpan timeSpan)
        {
            return DateTime.UtcNow.Date.Add(timeSpan).ToLongTimeString();
        }
    }
}
