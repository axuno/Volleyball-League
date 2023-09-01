using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="RoundLegEntity"/> <see cref="List{T}"/> extensions.
/// </summary>
public static class RoundLegEntityListExtensions
{
    /// <summary>
    /// Gets the first <see cref="RoundLegEntity"/> in the <see cref="List{T}"/> that contains the <paramref name="dateTime"/>,
    /// or <see langword="null"/> if none was found.
    /// </summary>
    /// <param name="roundLegList">The <see cref="IList{RoundLegEntity}"/> for which the calculation takes place.</param>
    /// <param name="dateTime"></param>
    /// <returns>Returns the first <see cref="RoundLegEntity"/> in the <see cref="List{T}"/> that contains the <paramref name="dateTime"/>,
    /// or <see langword="null"/> if none was found.</returns>
    public static RoundLegEntity? GetRoundLegForDate(this IList<RoundLegEntity> roundLegList, DateTime? dateTime)
    {
        return roundLegList.FirstOrDefault(roundLeg => roundLeg.ContainsDate(dateTime));
    }
}
