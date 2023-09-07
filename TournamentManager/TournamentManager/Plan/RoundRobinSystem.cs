using System.Collections.ObjectModel;

namespace TournamentManager.Plan;

/// <summary>
/// Specifies which team will be assigned referee for a match.
/// </summary>
public enum RefereeType
{
    HomeTeam = 1,
    GuestTeam,
    OtherTeamOfGroup
}

/// <summary>
/// Specifies the type of the leg to calculate match combinations.
/// </summary>
public enum LegType
{
    First = 1,
    Return
}

/// <summary>
/// This generic class calculates all matches for a group of teams.
/// The round robin system is applied, i.e. all teams in the group play each other.
/// </summary>
/// <typeparam name="T">The type of the team objects. Objects must have IComparable implemented.</typeparam>
public class RoundRobinSystem<T>
{
    private int _maxNumOfCombinations;
    private readonly TeamCombinationGroup<T> _combinationGroup = new();
    private readonly TeamCombinationGroup<T> _combinationGroupReturnLeg = new();
		
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="teams">A collection of teams to build matches for.</param>
    public RoundRobinSystem(Collection<T> teams)
    {
        Teams = teams;
    }

    /// <summary>
    /// Doing the job of creating matches for all teams.
    /// </summary>
    private void CalcCombinations()
    {
        /*
        Round robin match calculations are as follows.
        Example with 5 teams (which total in 10 matches):
                +-   A -+    -+ -+
                |       |     |  |
             +- | +- B -+ -+  |  |
             |  | |        |  |  |
          +- |  | |  C -+ -+  | -+
          |  |  | |     |     |
          |  |  | +- D -+ -+ -+
          |  |  |          |
          +- +- +-   E    -+
        */
        if (Teams.Count < 2)
            throw new InvalidOperationException("Round Robin system requires at least 2 teams.");

        if (Teams.Count < 3 && RefereeType == RefereeType.OtherTeamOfGroup)
            throw new InvalidOperationException("Round Robin system with separate referee requires at least 3 teams.");

        _combinationGroup.Clear();
        _maxNumOfCombinations = Teams.Count * (Teams.Count - 1) / 2;
        TeamCombinationsPerLeg = Teams.Count - 1;

        for (var count = 0; count < _maxNumOfCombinations; count++)
        {
            var homeTeam = GetHomeTeam();
            var guestTeam = GetGuestTeam(homeTeam);
            var referee = homeTeam;

            var homeTeamNumOfHomeCombs = GetNumOfHomeCombinations(homeTeam);
            var guestTeamNumOfHomeCombs = GetNumOfHomeCombinations(guestTeam);

            // make sure that home matches are alternating
            if (homeTeamNumOfHomeCombs <= guestTeamNumOfHomeCombs)
                _combinationGroup.Add(new TeamCombination<T>(homeTeam, guestTeam, referee));
            else
                _combinationGroup.Add(new TeamCombination<T>(guestTeam, homeTeam, referee));

            // re-assign referee according to settings
            switch (RefereeType)
            {
                case RefereeType.HomeTeam:
                    _combinationGroup[count].Referee = _combinationGroup[count].HomeTeam; break;
                case RefereeType.GuestTeam:
                    _combinationGroup[count].Referee = _combinationGroup[count].GuestTeam; break;
                case RefereeType.OtherTeamOfGroup:
                    _combinationGroup[count].Referee = GetReferee(homeTeam, guestTeam); break;
            }
        }

        CalcCombinationsReturnLeg();
    }

    /// <summary>
    /// Calculates the return leg based on the previously calculated first leg
    /// by swapping home / guest team and assigning the referee.
    /// </summary>
    private void CalcCombinationsReturnLeg()
    {
        _combinationGroupReturnLeg.Clear();
        var referee = _combinationGroup[0].Referee; // just to make the compiler happy

        foreach (var match in _combinationGroup)
        {
            // re-assign referee according to settings:
            switch (RefereeType)
            {
                case RefereeType.HomeTeam:
                    referee = match.GuestTeam; break;
                case RefereeType.GuestTeam:
                    referee = match.HomeTeam; break;
                case RefereeType.OtherTeamOfGroup:
                    referee = match.Referee; break;
            }
            _combinationGroupReturnLeg.Add(new TeamCombination<T>(match.GuestTeam, match.HomeTeam, referee));
        }
    }


    /// <summary>
    /// Determines the next home team by checking which of the
    /// teams has least matches up to now.
    /// </summary>
    /// <returns>Returns the home team.</returns>
    private T GetHomeTeam()
    {
        var lastMaxMissingCount = int.MinValue;
        var lastMaxMissingTeam = Teams[0];

        foreach (var team in Teams)
        {
            var currentMissingCount = GetMissingCombinationsCount(team);
            if (currentMissingCount > lastMaxMissingCount)
            {
                lastMaxMissingCount = currentMissingCount;
                lastMaxMissingTeam = team;
            }
        }
        return lastMaxMissingTeam;
    }

    /// <summary>
    /// Determines the next guest team by checking
    /// a) the team with least matches, that is not identical to home team
    /// b) that the team combination for the two teams does not exists (home/guest, guest/home).
    /// </summary>
    /// <param name="homeTeam">The home team already fixed for the match.</param>
    /// <returns>Returns the guest team.</returns>
    private T GetGuestTeam(T homeTeam)
    {
        var lastMaxMissingCount = int.MinValue;
        var lastMaxMissingTeam = Teams[0];

        foreach (var team in Teams)
        {
            var currentMissingCount = GetMissingCombinationsCount(team);
            if (currentMissingCount > lastMaxMissingCount && (Comparer<T>.Default.Compare(team, homeTeam) != 0) && !CombinationExists(homeTeam, team))
            {
                lastMaxMissingCount = currentMissingCount;
                lastMaxMissingTeam = team;
            }
        }
			
        return lastMaxMissingTeam;
    }


    /// <summary>
    /// Determines the referee for a match.
    /// </summary>
    /// <param name="homeTeam">The home team of the match.</param>
    /// <param name="guestTeam">The guest team of the match.</param>
    /// <returns></returns>
    private T GetReferee(T homeTeam, T guestTeam)
    {
        var lastMaxRefereeCount = int.MaxValue;
        var lastMaxRefereeTeam = Teams[0];

        foreach (var team in Teams)
        {
            var currentRefereeCount = GetNumOfRefereeCombinations(team);
            if (currentRefereeCount < lastMaxRefereeCount && 
                (Comparer<T>.Default.Compare(team, homeTeam) != 0) && (Comparer<T>.Default.Compare(team, guestTeam) != 0) &&
                ! IsLastReferee(team))
            {
                lastMaxRefereeCount = currentRefereeCount;
                lastMaxRefereeTeam = team;
            }
        }

        return lastMaxRefereeTeam;
    }


    /// <summary>
    /// Checks whether the team was referee in the last match.
    /// Always returns false if the match collection is empty.
    /// </summary>
    /// <param name="team">The team to check whether it was referee in the last match.</param>
    /// <returns>Return true, if the team was referee in the last match, false otherwise.</returns>
    private bool IsLastReferee(T team)
    {
        if (_combinationGroup.Count == 0)
            return false;
        else
            return (Comparer<T>.Default.Compare(team, _combinationGroup[^1].Referee) == 0);
    }


    /// <summary>
    /// Calculates how many matches for the team are still not fixed.
    /// </summary>
    /// <param name="team">The team to calculate missing matches for.</param>
    /// <returns>Returns the number of missing matches for the team.</returns>
    private int GetMissingCombinationsCount(T team)
    {
        var missing = TeamCombinationsPerLeg;

        foreach (var match in _combinationGroup)
        {
            if (Comparer<T>.Default.Compare(match.HomeTeam, team) == 0) missing--;
            if (Comparer<T>.Default.Compare(match.GuestTeam, team) == 0) missing--;
        }

        return missing;
    }

    /// <summary>
    /// Checks whether the team combination for the two teams 
    /// does not exists (either home/guest, or guest/home).
    /// </summary>
    /// <param name="homeTeam">The home team to compare.</param>
    /// <param name="guestTeam">The guest team to compare.</param>
    /// <returns>Returns true, if the team combination already exists, false otherwise.</returns>
    private bool CombinationExists(T homeTeam, T guestTeam)
    {
        foreach (var match in _combinationGroup)
        {
            if ((Comparer<T>.Default.Compare(match.HomeTeam, homeTeam) == 0) && (Comparer<T>.Default.Compare(match.GuestTeam, guestTeam) == 0))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Calculates the number of team matches already fixed for the team.
    /// </summary>
    /// <param name="team">The team to calculate the number of home matches.</param>
    /// <returns>The number of home matches for the team.</returns>
    private int GetNumOfHomeCombinations(T team)
    {
        var count = 0;

        foreach (var match in _combinationGroup)
        {
            if (Comparer<T>.Default.Compare(match.HomeTeam, team) == 0) count++;
        }
        return count;
    }

    /// <summary>
    /// Calculates the number of matches a team was assigned referee.
    /// </summary>
    /// <param name="team">The team to calculate the number of referee matches.</param>
    /// <returns>The number of referee matches for the team.</returns>
    private int GetNumOfRefereeCombinations(T team)
    {
        var count = 0;

        foreach (var match in _combinationGroup)
        {
            if (Comparer<T>.Default.Compare(match.Referee, team) == 0) count++;
        }
        return count;
    }


    /// <summary>
    /// Calculates all matches for the given teams, using the round robin system.
    /// </summary>
    /// <param name="refereeType">Determines how referees will be assigned for the matches.</param>
    /// <param name="legType">First leg or return leg.</param>
    /// <param name="optiType">Optimization type for groups. Differences can be seen be with an uneven number of teams.</param>
    /// <returns>Return a collection containing collections of optimized team combinations.</returns>
    public Collection<TeamCombinationGroup<T>> GetBundledGroups(RefereeType refereeType, LegType legType, CombinationGroupOptimization optiType)
    {
        RefereeType = refereeType;
        CalcCombinations();

        return (new CombinationGroupOptimizer<T>((legType == LegType.First) ? _combinationGroup : _combinationGroupReturnLeg).GetBundledGroups(optiType));
    }

    /// <summary>
    /// Calculates all matches for the given teams, using the round robin system.
    /// </summary>
    /// <param name="refereeType">Determines how referees will be assigned for the matches.</param>
    /// <param name="legType">First leg or return leg.</param>
    /// <returns>Return a collection of team combinations.</returns>
    public TeamCombinationGroup<T> GetCombinationGroup(RefereeType refereeType, LegType legType)
    {
        RefereeType = refereeType;
        CalcCombinations();

        return (legType == LegType.First) ? _combinationGroup : _combinationGroupReturnLeg;
    }


    /// <summary>
    /// Gets the number of matches per team.
    /// </summary>
    public int TeamCombinationsPerLeg { get; private set; }

    /// <summary>
    /// Gets the number of teams.
    /// </summary>
    public int NumberOfTeams => Teams.Count;

    /// <summary>
    /// Gets the teams.
    /// </summary>
    public Collection<T> Teams { get; } = new();

    /// <summary>
    /// Gets whether the home team is also assigned referee (true for 'yes').
    /// </summary>
    public RefereeType RefereeType { get; private set; } = RefereeType.HomeTeam;
}
