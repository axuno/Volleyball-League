namespace League.Authorization;

public static class PolicyName
{
    /// <summary>
    /// Policy for general access edit match data
    /// </summary>
    public const string MatchPolicy = nameof(MatchPolicy);
    /// <summary>
    /// Policy for seeing team contact data
    /// </summary>
    public const string SeeTeamContactsPolicy = nameof(SeeTeamContactsPolicy);
    /// <summary>
    /// Policy for "my team" views, primarily for team managers and players.
    /// </summary>
    public const string MyTeamPolicy = nameof(MyTeamPolicy);
    /// <summary>
    /// Admin policy for "my team" views, so that admin can show any team as "my team"
    /// </summary>
    public const string MyTeamAdminPolicy = nameof(MyTeamAdminPolicy);
}
