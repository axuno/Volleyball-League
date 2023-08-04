using System.Linq;
using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="SetEntity"/> extension methods.
/// </summary>
public static class SetEntityExtensions
{
    /// <summary>
    /// Assigns &quot;set points&quot; to the <see cref="SetEntity"/>.
    /// The <see cref="SetEntity.IsOverruled"/> property will be set to <see langword="false"/>
    /// </summary>
    /// <param name="setEntity"></param>
    /// <param name="setRule">The <see cref="SetRuleEntity"/> with the rules to apply.</param>
    public static void CalculateSetPoints(this SetEntity setEntity, SetRuleEntity setRule)
    {
        setEntity.IsOverruled = false;
        if (setEntity.HomeBallPoints > setEntity.GuestBallPoints)
        {
            setEntity.HomeSetPoints = setRule.PointsSetWon;
            setEntity.GuestSetPoints = setRule.PointsSetLost;
        }
        else if (setEntity.HomeBallPoints < setEntity.GuestBallPoints)
        {
            setEntity.HomeSetPoints = setRule.PointsSetLost;
            setEntity.GuestSetPoints = setRule.PointsSetWon;
        }
        else
        {
            setEntity.HomeSetPoints = setEntity.GuestSetPoints = setRule.PointsSetTie;
        }
    }

    /// <summary>
    /// Sets home/guest ball points and home/guest set points.
    /// The <see cref="SetEntity.IsOverruled"/> property will be set to <see langword="true"/>,
    /// while <see cref="SetEntity.IsTieBreak"/> will be set to <see langword="false"/>.
    /// </summary>
    /// <param name="setEntity"></param>
    /// <param name="homeBallPoints"></param>
    /// <param name="guestBallPoints"></param>
    /// <param name="homeSetPoints"></param>
    /// <param name="guestSetPoints"></param>
    public static void Overrule(this SetEntity setEntity, int homeBallPoints, int guestBallPoints, int homeSetPoints, int guestSetPoints)
    {
        setEntity.HomeBallPoints = homeBallPoints;
        setEntity.GuestBallPoints = guestBallPoints;
        setEntity.HomeSetPoints = homeSetPoints;
        setEntity.GuestSetPoints = guestSetPoints;
        setEntity.IsTieBreak = false;
        setEntity.IsOverruled = true;
    }
}
