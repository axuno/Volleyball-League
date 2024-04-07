namespace TournamentManager.ModelValidators;

/// <summary>
/// The mode to validate a match.
/// This setting is used with the <see cref="MatchResultValidator"/>, <see cref="SetsValidator"/> and <see cref="SingleSetValidator"/>.
/// </summary>
public enum MatchValidationMode
{
    Default = 0,
    Overrule
}

