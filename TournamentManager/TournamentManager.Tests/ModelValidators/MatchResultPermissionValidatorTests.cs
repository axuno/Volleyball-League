using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class MatchResultPermissionValidatorTests
{
    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Tournament_Is_In_Active_Mode(bool tournamentInPlanMode, bool failureExpected)
    {
        var data = (new TenantContext(),
            (TournamentInPlanMode: tournamentInPlanMode, RoundIsCompleted: false,
                CurrentDateUtc: new DateTime(2020, 07, 01, 20, 00, 00)));
        var match = new MatchEntity();
        var mpv = new MatchResultPermissionValidator(match, data);
        await mpv.CheckAsync(MatchResultPermissionValidator.FactId.TournamentIsInActiveMode,
            CancellationToken.None);
        var factResults = mpv.GetFailedFacts();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResults.Count == 1, Is.EqualTo(failureExpected));
            Assert.That(factResults.FirstOrDefault()?.Id == MatchResultPermissionValidator.FactId.TournamentIsInActiveMode, Is.EqualTo(failureExpected));
            Assert.That(factResults.All(fr => fr.Exception == null), Is.True);
        }
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Round_Is_Still_Running(bool roundCompleted, bool failureExpected)
    {
        var data = (new TenantContext(),
            (TournamentInPlanMode: false, RoundIsCompleted: roundCompleted,
                CurrentDateUtc: new DateTime(2020, 07, 01, 20, 00, 00)));
        var match = new MatchEntity();
        var mpv = new MatchResultPermissionValidator(match, data);
        await mpv.CheckAsync(MatchResultPermissionValidator.FactId.RoundIsStillRunning,
            CancellationToken.None);
        var factResults = mpv.GetFailedFacts();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResults.Count == 1, Is.EqualTo(failureExpected));
            Assert.That(factResults.FirstOrDefault()?.Id == MatchResultPermissionValidator.FactId.RoundIsStillRunning, Is.EqualTo(failureExpected));
            Assert.That(factResults.All(fr => fr.Exception == null), Is.True);
        }
    }

    [TestCase("2020-07-02 20:00:00", 10, false)]
    [TestCase("2020-07-30 20:00:00", 10, true)]
    [TestCase("2020-07-30 20:00:00", 0, true)]
    public async Task Current_Date_Is_Before_Result_Correction_Deadline(DateTime changeDate, int maxDaysForCorrection, bool failureExpected)
    {
        var data = (new TenantContext() { TournamentContext = new() {MaxDaysForResultCorrection = maxDaysForCorrection}},
            (TournamentInPlanMode: false, RoundIsCompleted: false,
                CurrentDateUtc: changeDate));
        var match = new MatchEntity {RealStart = new DateTime(2020, 07, 01, 20, 00, 00) };
        var mpv = new MatchResultPermissionValidator(match, data);
        await mpv.CheckAsync(MatchResultPermissionValidator.FactId.CurrentDateIsBeforeResultCorrectionDeadline,
            CancellationToken.None);
        var factResults = mpv.GetFailedFacts();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResults.Count == 1, Is.EqualTo(failureExpected));
            Assert.That(factResults.FirstOrDefault()?.Id == MatchResultPermissionValidator.FactId.CurrentDateIsBeforeResultCorrectionDeadline, Is.EqualTo(failureExpected));
            Assert.That(factResults.All(fr => fr.Exception == null), Is.True);
        }
    }
}
