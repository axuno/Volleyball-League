namespace TournamentManager.Tests.RoundRobin;

/// <summary>
/// IEquatable is implemented implicitly.
/// </summary>
internal record struct CustomTestStruct
{
    public int A { get; set; }
    public int B { get; set; }
    public int C { get; set; }

    public override readonly string ToString()
    {
        return $"{A}-{B}-{C}";
    }
}
