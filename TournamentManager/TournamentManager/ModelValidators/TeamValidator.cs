using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class TeamValidator : AbstractValidator<TeamEntity, ITenantContext, TeamValidator.FactId>
{
    public enum FactId
    {
        TeamNameIsSet,
        TeamNameIsUnique,
        MatchDayOfWeekAndTimeIsSet,
        MatchTimeWithinRange,
        DayOfWeekWithinRange
    }

    public TeamValidator(TeamEntity model, ITenantContext tenantContext) : base(model, tenantContext)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        Facts.Add(TeamNameIsSet());
        Facts.Add(TeamNameIsUnique());
        Facts.Add(MatchDayOfWeekAndTimeIsSet());
        Facts.Add(DayOfWeekWithinRange());
        Facts.Add(MatchTimeWithinRange());
    }

    private Fact<FactId> MatchTimeWithinRange()
    {
        return new()
        {
            Id = FactId.MatchTimeWithinRange,
            FieldNames = [nameof(Model.MatchTime)],
            Enabled = Data.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet,
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = string.Format(
                        TeamValidatorResource.ResourceManager.GetString(
                            nameof(FactId.MatchTimeWithinRange)) ?? string.Empty,
                        Data.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MinDayTime.ToShortTimeString(),
                        Data.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MaxDayTime.ToShortTimeString()),
                    // regular start time is given in local time
                    Success = Model.MatchTime.HasValue && Model.MatchTime >= Data.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MinDayTime &&
                              Model.MatchTime <= Data.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MaxDayTime
                })
        };
    }

    private Fact<FactId> DayOfWeekWithinRange()
    {
        return new()
        {
            Id = FactId.DayOfWeekWithinRange,
            FieldNames = [nameof(Model.MatchDayOfWeek)],
            Enabled = Data.TournamentContext.TeamRuleSet.HomeMatchTime is { IsEditable: true, MustBeSet: true },
            Type = Data.TournamentContext.TeamRuleSet.HomeMatchTime.ErrorIfNotInDaysOfWeekRange
                ? FactType.Error
                : FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = string.Format(TeamValidatorResource.ResourceManager.GetString(
                            nameof(FactId.DayOfWeekWithinRange)) ?? string.Empty,
                        Data.TournamentContext.TeamRuleSet.HomeMatchTime.ErrorIfNotInDaysOfWeekRange
                            ? TeamValidatorResource.required
                            : TeamValidatorResource.recommended),
                    Success = Data.TournamentContext.TeamRuleSet.HomeMatchTime.DaysOfWeekRange.Select(dow => (int?) dow).ToList()
                        .Contains(Model.MatchDayOfWeek)
                })
        };
    }

    private Fact<FactId> MatchDayOfWeekAndTimeIsSet()
    {
        return new()
        {
            Id = FactId.MatchDayOfWeekAndTimeIsSet,
            FieldNames = [nameof(Model.MatchDayOfWeek), nameof(Model.MatchTime)],
            Enabled = Data.TournamentContext.TeamRuleSet.HomeMatchTime is { IsEditable: true },
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = TeamValidatorResource.ResourceManager.GetString(
                        nameof(FactId.MatchDayOfWeekAndTimeIsSet)) ?? string.Empty,
                    Success = (Data.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet && Model is { MatchDayOfWeek: not null, MatchTime: not null })
                              || (!Data.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet && Model is { MatchDayOfWeek: null, MatchTime: null } or { MatchDayOfWeek: not null, MatchTime: not null })
                })
        };
    }

    private Fact<FactId> TeamNameIsUnique()
    {
        return new()
        {
            Id = FactId.TeamNameIsUnique,
            FieldNames = [nameof(Model.Name)],
            Enabled = !string.IsNullOrEmpty(Model.Name),
            Type = FactType.Critical,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var teamName = await Data.DbContext.AppDb.TeamRepository.TeamNameExistsAsync(Model, cancellationToken)
                .ConfigureAwait(false);
            return new()
            {
                Message = string.Format(TeamValidatorResource.ResourceManager.GetString(
                    nameof(FactId.TeamNameIsUnique)) ?? string.Empty, teamName),
                Success = string.IsNullOrEmpty(teamName)
            };
        }
    }

    private Fact<FactId> TeamNameIsSet()
    {
        return new()
        {
            Id = FactId.TeamNameIsSet,
            FieldNames = [nameof(Model.Name)],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = TeamValidatorResource.ResourceManager.GetString(
                        nameof(FactId.TeamNameIsSet)) ?? string.Empty,
                    Success = !string.IsNullOrWhiteSpace(Model.Name)
                })
        };
    }
}
