using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ModelValidators;

namespace League.Models.MatchViewModels;

public class EditFixtureViewModel
{
    public EditFixtureViewModel()
    {}

    public EditFixtureViewModel(PlannedMatchRow? plannedMatch, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
    {
        PlannedMatch = plannedMatch;
        TimeZoneConverter = timeZoneConverter;

        // Set form fields
        if (PlannedMatch != null)
        {
            Id = PlannedMatch.Id;
            VenueId = PlannedMatch.VenueId;
            // set date and time for the current time zone
            var currentDate =
                TimeZoneConverter.ToZonedTime(PlannedMatch.PlannedStart);
            MatchDate = currentDate?.DateTimeOffset.Date;
            MatchTime = currentDate != null ? TimeOnly.FromTimeSpan(currentDate.DateTimeOffset.TimeOfDay) : null;

            // mark a list item as selected
            VenueNotSpecifiedKey = null;  // "not specified" will not show in the list
            VenueId = PlannedMatch.VenueId ?? VenueNotSpecifiedKey ?? 0;
        }
    }

    #region *** Form fields ***

    [HiddenInput]
    public long Id { get; set; }

    [HiddenInput]
    public string? Hash { get; set; }

    [Display(Name = "Match date")]
    public DateTime? MatchDate { get; set; }

    [Display(Name = "Match start time")]
    public TimeOnly? MatchTime { get; set; }

    [Display(Name = "Venue")]
    public long? VenueId { get; set; }

    [Display(Name = "Ignore notices")]
    public bool OverrideWarnings { get; set; }

    #endregion

    public TournamentEntity? Tournament { get; set; }

    public PlannedMatchRow? PlannedMatch { get;}


    /// <summary>
    /// If <c>true</c>, validation messages are warnings, which can be overridden with <see cref="OverrideWarnings"/>.
    /// </summary>
    public bool IsWarning { get; set; }

    /// <summary>
    /// This is the key for a "not specified" venue. If this value is null, "not specified" cannot be selected
    /// </summary>
    public long? VenueNotSpecifiedKey;

    /// <summary>
    /// The list of venues for this fixture belonging to the home or guest team.
    /// </summary>
    public List<VenueEntity> VenuesOfMatchTeams { get; set; } = new();

    /// <summary>
    /// The list of active venues to select for this fixture.
    /// </summary>
    public List<VenueEntity> ActiveVenues { get; set; } = new();

    /// <summary>
    /// The list of unused venues to select for this fixture.
    /// </summary>
    public List<VenueEntity> UnusedVenues { get; set; } = new();

    /// <summary>
    /// The TimeZoneConverter, used by the razor view
    /// </summary>
    public Axuno.Tools.DateAndTime.TimeZoneConverter? TimeZoneConverter { get; }

    private string ComputeInputHash()
    {
        return Axuno.Tools.Hash.Md5.GetHash(string.Join(string.Empty,
            Id.ToString(),
            MatchDate?.Ticks.ToString() ?? string.Empty,
            MatchTime?.Ticks.ToString() ?? string.Empty,
            VenueId?.ToString() ?? string.Empty));
    }

    public async Task<bool> ValidateAsync(FixtureValidator validator, ModelStateDictionary modelState)
    {
        await validator.CheckAsync(CancellationToken.None);

        foreach (var fact in validator.GetFailedFacts())
        {
            if (fact.Exception != null)
            {
                throw new InvalidOperationException(null, fact.Exception);
            }

            if (fact.Type == FactType.Critical || fact.Type == FactType.Error)
            {
                if (fact.FieldNames.Contains(nameof(FixtureValidator.Model.PlannedStart)))
                {
                    modelState.AddModelError(nameof(EditFixtureViewModel.MatchDate), fact.Message);
                    modelState.AddModelError(nameof(EditFixtureViewModel.MatchTime), fact.Message);
                }
                else
                {
                    foreach (var fieldName in fact.FieldNames)
                    {
                        modelState.AddModelError(fieldName, fact.Message);
                    }
                }
            }
            else
            {
                modelState.AddModelError(string.Empty, fact.Message);
                // Validator generates FactType.Warning only, if no errors exist
                IsWarning = true;
            }
        }

        // to override warnings, form fields must be unchanged since last post
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

    public class FixtureMessage
    {
        public long MatchId { get; set; }
        public bool ChangeSuccess { get; set; }
    }
}
