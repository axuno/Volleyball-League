using System.ComponentModel.DataAnnotations;
using League.Components;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;

namespace League.Models.TeamViewModels;

public class TeamEditModel
{
    [BindProperty]
    public RoundSelectorComponentModel? Round { get; set; }
        
    [BindProperty]
    public TeamEditorComponentModel? Team { get; set; }

    [HiddenInput]
    public string? Hash { get; set; }

    [Display(Name = "Ignore notices")]
    public bool OverrideWarnings { get; set; }

    [BindNever]
    public TeamEntity? TeamEntity { get; set; }

    [BindNever]
    public bool IsWarning { get; set; }

    public void MapFormFieldsToEntity()
    {
        // Only if the round selector was shown to the user.
        // Otherwise, the name of the current round was just displayed.
        if (Round!.ShowSelector && Round.SelectedRoundId.HasValue)
        {
            if (TeamEntity!.TeamInRounds.Any())
            {
                // Only the RoundId is updated, but not the TeamNameForRound!
                TeamEntity.TeamInRounds[0].RoundId = Round.SelectedRoundId.Value;
            }
            else
            {
                // trying to add an existing TeamInRound combination of TeamId and RoundId will throw
                var tir = TeamEntity.TeamInRounds.AddNew();
                tir.RoundId = Round.SelectedRoundId.Value;
                tir.TeamNameForRound = Team!.Name;
            }
        }

        Team!.MapFormFieldsToEntity(TeamEntity!);
    }

    public async Task<bool> ValidateAsync(TeamValidator teamValidator, long tournamentId, ModelStateDictionary modelState, CancellationToken cancellationToken)
    {
        if (Round!.ShowSelector && Round.SelectedRoundId.HasValue)
        {
            var tirValidator = new TeamInRoundValidator(
                new() {RoundId = Round.SelectedRoundId.Value, TeamId = teamValidator.Model.Id},
                (teamValidator.Data, tournamentId));

            await tirValidator.CheckAsync(cancellationToken);
            foreach (var fact in tirValidator.GetFailedFacts())
            {
                // validator currently only has one rule
                if (fact.Id == TeamInRoundValidator.FactId.RoundBelongsToTournament)
                {
                    modelState.AddModelError(string.Join('.', Round.HtmlFieldPrefix, nameof(Round.SelectedRoundId)),
                        fact.Message);
                }
            }
        }

        await teamValidator.CheckAsync(cancellationToken);
            
        foreach (var fact in teamValidator.GetFailedFacts())
        {
            if (fact.Exception != null)
            {
                throw new InvalidOperationException(null, fact.Exception);
            }

            if (fact.Type == FactType.Critical || fact.Type == FactType.Error)
            {
                foreach (var fieldName in fact.FieldNames)
                {
                    modelState.AddModelError(string.Join('.', Team!.HtmlFieldPrefix, fieldName), fact.Message);
                }
            }
            else
            {
                modelState.AddModelError(string.Empty, fact.Message);
                // Validator generates FactType.Warning only, if no errors exist
                IsWarning = true;
            }
        }

        // The Hash is re-calculated with the new submitted values.
        // We have to compare to the original hidden Hash field value,
        // because to override warnings, form fields must be unchanged since last post
        var newHash = ComputeInputHash();
        if (IsWarning && OverrideWarnings && newHash == Hash)
        {
            modelState.Clear();
            IsWarning = false;
        }

        if (!modelState.IsValid)
        {
            // Show checkbox unchecked
            OverrideWarnings = false;
            // Set hash field value to latest form fields content
            Hash = newHash;
        }

        return modelState.IsValid;
    }

    private string ComputeInputHash()
    {
        return Axuno.Tools.Hash.Md5.GetHash(string.Join(string.Empty,
            Round!.SelectedRoundId?.ToString() ?? string.Empty,
            Team!.Id.ToString(),
            Team.Name ?? string.Empty,
            Team.ClubName ?? string.Empty,
            Team.MatchTime?.ToString() ?? string.Empty,
            Team.MatchDayOfWeek?.ToString() ?? string.Empty));
    }
}
