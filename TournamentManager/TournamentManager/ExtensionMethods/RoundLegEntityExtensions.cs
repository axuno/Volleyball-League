using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="RoundLegEntity"/> extension methods.
/// </summary>
public static class RoundLegEntityExtensions
{
    /// <summary>
    /// Checks whether the <paramref name="dateTime"></paramref> is between
    /// <see cref="RoundLegEntity.StartDateTime"/> and <see cref="RoundLegEntity.EndDateTime"/>.
    /// If one of the date values is <see langword="null"/>, <see langword="false"/> is returned.
    /// </summary>
    /// <param name="roundLeg"></param>
    /// <param name="dateTime"></param>
    /// <returns>Returns <see langword="true"/>, if the <paramref name="dateTime"></paramref> is between
    /// <see cref="RoundLegEntity.StartDateTime"/> and <see cref="RoundLegEntity.EndDateTime"/>.
    /// If one of the date value is <see langword="null"/>, <see langword="false"/> is returned.
    /// </returns>
    public static bool ContainsDate(this RoundLegEntity roundLeg, DateTime? dateTime)
    {
        const int millisecondsOneSecond = 1000;

        if (roundLeg.StartDateTime.TimeOfDay.TotalMilliseconds < millisecondsOneSecond &&
            roundLeg.EndDateTime.TimeOfDay.TotalMilliseconds < millisecondsOneSecond)
        {
            return new DateTimePeriod(roundLeg.StartDateTime, roundLeg.EndDateTime.AddDays(1).AddMilliseconds(-1))
                .Contains(dateTime);
        }

        return new DateTimePeriod(roundLeg.StartDateTime, roundLeg.EndDateTime).Contains(dateTime);
    }
}
