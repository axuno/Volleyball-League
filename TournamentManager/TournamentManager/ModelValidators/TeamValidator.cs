using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.TeamNameIsSet,
                FieldNames = new[] { nameof(Model.Name) },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = TeamValidatorResource.ResourceManager.GetString(
                            nameof(FactId.TeamNameIsSet)) ?? string.Empty,
                        Success = !string.IsNullOrWhiteSpace(Model.Name)
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.TeamNameIsUnique,
                FieldNames = new[] { nameof(Model.Name) },
                Enabled = !string.IsNullOrEmpty(Model.Name),
                Type = FactType.Critical,
                CheckAsync = async (cancellationToken) =>
                {
                    var teamName = await Data.DbContext.AppDb.TeamRepository.TeamNameExistsAsync(Model, cancellationToken);
                    return new FactResult
                    {
                        Message = string.Format(TeamValidatorResource.ResourceManager.GetString(
                            nameof(FactId.TeamNameIsUnique)) ?? string.Empty, teamName),
                        Success = string.IsNullOrEmpty(teamName)
                    };
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.MatchDayOfWeekAndTimeIsSet,
                FieldNames = new[] {nameof(Model.MatchDayOfWeek), nameof(Model.MatchTime)},
                Enabled = Data.TournamentContext.TeamRuleSet.HomeMatchTime.IsEditable && Data.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = TeamValidatorResource.ResourceManager.GetString(
                            nameof(FactId.MatchDayOfWeekAndTimeIsSet)) ?? string.Empty,
                        Success = Model.MatchDayOfWeek.HasValue && Model.MatchTime.HasValue
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.DayOfWeekWithinRange,
                FieldNames = new[] {nameof(Model.MatchDayOfWeek)},
                Enabled = Data.TournamentContext.TeamRuleSet.HomeMatchTime.IsEditable && Data.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet,
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
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.MatchTimeWithinRange,
                FieldNames = new[] {nameof(Model.MatchTime)},
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
            });
    }
}