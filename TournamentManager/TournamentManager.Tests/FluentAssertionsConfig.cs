using TournamentManager.Tests;

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

namespace TournamentManager.Tests;

public static class AssertionEngineInitializer
{
    /// <summary>
    /// Acknowledges the soft warning from FluentAssertions about the license acceptance.
    /// </summary>
    public static void AcknowledgeSoftWarning()
    {
        FluentAssertions.License.Accepted = true;
    }
}

