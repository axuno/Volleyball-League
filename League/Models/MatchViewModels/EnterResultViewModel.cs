using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HarfBuzzSharp;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.ModelValidators;

namespace League.Models.MatchViewModels;

public class EnterResultViewModel
{
    private readonly StringLocalizer<EnterResultViewModel> _localizer;
    private readonly int _maxNumberOfSets;

    public EnterResultViewModel()
    {
        _localizer = CreateModelStringLocalizer();
    }

    public EnterResultViewModel(TournamentEntity tournament, RoundEntity round, 
        MatchEntity match, MatchRuleEntity matchRule, IList<TeamInRoundEntity> teamInRound, 
        Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter) : this()
    {
        Tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
        Round = round ?? throw new ArgumentNullException(nameof(round));
        Match = match ?? throw new ArgumentNullException(nameof(match));
        Opponent = new(
            teamInRound.FirstOrDefault(o => o.TeamId == match.HomeTeamId)?.TeamNameForRound ?? throw new ArgumentNullException(nameof(teamInRound)),
            teamInRound.FirstOrDefault(o => o.TeamId == match.GuestTeamId)?.TeamNameForRound ?? throw new ArgumentNullException(nameof(teamInRound))); ;
        TimeZoneConverter = timeZoneConverter ?? throw new ArgumentNullException(nameof(timeZoneConverter));
        
        _maxNumberOfSets = matchRule.MaxNumOfSets();
        MapEntityToFormFields();
    }

    private static StringLocalizer<EnterResultViewModel> CreateModelStringLocalizer()
    {
        // no need for any params if using a StringLocalizer<T>
        var options = Microsoft.Extensions.Options.Options.Create(new LocalizationOptions());
        var factory = new ResourceManagerStringLocalizerFactory(options, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
        // Note: The resource is also used by System.ComponentModel.DataAnnotations
        return new(factory);
    }

    public void MapEntityToFormFields()
    {
        if (Match!.RealStart.HasValue && Match.RealEnd.HasValue)
        {
            var startDate = TimeZoneConverter!.ToZonedTime(Match.RealStart);
            var endDate = TimeZoneConverter.ToZonedTime(Match.RealEnd);
            MatchDate = startDate?.DateTimeOffset.Date;
            MatchTimeFrom = startDate != null ? TimeOnly.FromTimeSpan(startDate.DateTimeOffset.TimeOfDay) : null;
            MatchTimeTo = endDate != null ? TimeOnly.FromTimeSpan(endDate.DateTimeOffset.TimeOfDay) : null;
            Remarks = Match.Remarks;
        }
        else
        {
            MatchDate = TimeZoneConverter!.ToZonedTime(Match.PlannedStart)?.DateTimeOffset.Date;
        }

        // Important: Sort sets by sequence number (i.e. 1, 2, 3, ...)
        Match.Sets.Sort((int)SetFieldIndex.SequenceNo, ListSortDirection.Ascending);

        // Add sets from storage to form fields
        foreach (var set in Match.Sets)
        {
            BallPoints.Add(new(set.HomeBallPoints, set.GuestBallPoints));
            SetPoints.Add(new(set.HomeSetPoints, set.GuestSetPoints));
        }

        // Add empty ball/set points to form fields
        while (BallPoints.Count < _maxNumberOfSets)
        {
            BallPoints.Add(new(default, default(int?)));
            SetPoints.Add(new(default, default(int?)));
        }

        HomePoints = Match.HomePoints;
        GuestPoints = Match.GuestPoints;

        Id = Match.Id;

        Remarks = Match.Remarks;
    }

    public void MapFormFieldsToEntity()
    {
        // Save match date/time to entity
        if (MatchDate.HasValue && MatchTimeFrom.HasValue && MatchTimeTo.HasValue)
        {
            var period = new DateTimePeriod(MatchDate?.Add(MatchTimeFrom.Value.ToTimeSpan()), MatchDate?.Add(MatchTimeTo.Value.ToTimeSpan()));
            Match!.SetRealStart(TimeZoneConverter!.ToUtc(period.Start), period.Duration());
        }
        else
        {
            Match!.SetRealStart(null, TimeSpan.Zero);
        }

        // Add sets to entity
        Match.Sets.Clear(true);
        // sets where home or guest ball points are NULL will be ignored
        if (IsOverruling)
        {
            Match.Sets.Add(Match.Id, BallPoints, SetPoints);
            Match.HomePoints = HomePoints;
            Match.GuestPoints = GuestPoints;
            Match.IsOverruled = true;
        }
        else
        {
            Match.Sets.Add(Match.Id, BallPoints);
            Match.IsOverruled = false;
            // Automatic point calculation must run before validation because of tie-break handling
            _ = Match.Sets.CalculateSetPoints(Round!.SetRule, Round.MatchRule);
        }

        Match.Remarks = Remarks;
        Match.ChangeSerial++;
        Match.IsComplete = true;
    }

    #region *** Form fields ***

    [HiddenInput]
    public long? Id { get; set; }

    [HiddenInput]
    public string? Hash { get; set; }

    /// <summary>
    /// <see cref="ReturnUrl"/> is needed to return to either fixtures or results,
    /// when the cancel button is clicked.
    /// </summary>
    [HiddenInput]
    public string? ReturnUrl { get; set; }

    [Display(Name = "Match date")]
    public DateTime? MatchDate { get; set; }

    [Display(Name = "Match start time")]
    public TimeOnly? MatchTimeFrom { get; set; }

    [Display(Name = "Match end time")]
    public TimeOnly? MatchTimeTo { get; set; }

    [Display(Name = "Ignore notices")]
    public bool OverrideWarnings { get; set; }

    [Display(Name="Remarks")]
    [MaxLength(2000)]
    public string? Remarks { get; set; }

    public List<PointResult> BallPoints { get; set; } = [];

    #region * Overruling *

    /// <summary>
    /// <c>true</c> to allow overruling to award the set and match points manually.
    /// </summary>
    [HiddenInput]
    public bool IsOverruling { get; set; } = false;

    /// <summary>
    /// Show, form fields if <see cref="IsOverruling"/> is <c>true</c>.
    /// </summary>
    public List<PointResult> SetPoints { get; set; } = [];

    /// <summary>
    /// Show field for home match points, if <see cref="IsOverruling"/> is <c>true</c>.
    /// </summary>
    public int? HomePoints { get; set; }

    /// <summary>
    /// Show field for guest match points, if <see cref="IsOverruling"/> is <c>true</c>.
    /// </summary>
    public int? GuestPoints { get; set; }

    #endregion

    #endregion

    public TournamentEntity? Tournament { get; set; }

    public RoundEntity? Round { get; set; }

    public MatchEntity? Match { get;}

    public Opponent? Opponent { get; }


    /// <summary>
    /// If <c>true</c>, validation messages are warnings, which can be overridden with <see cref="OverrideWarnings"/>.
    /// </summary>
    public bool IsWarning { get; set; }
 
    /// <summary>
    /// The TimeZoneConverter, used by the razor view
    /// </summary>
    public Axuno.Tools.DateAndTime.TimeZoneConverter? TimeZoneConverter { get; }

    private string ComputeInputHash()
    {
        return Axuno.Tools.Hash.Md5.GetHash(string.Join(string.Empty,
            Id.ToString(),
            MatchDate?.Ticks.ToString() ?? string.Empty,
            MatchTimeFrom?.Ticks.ToString() ?? string.Empty,
            MatchTimeTo?.ToString() ?? string.Empty,
            string.Join(string.Empty, BallPoints.Select(s => s.Home + s.Guest.ToString())), 
            Remarks));
    }

    public async Task<bool> ValidateAsync(MatchResultValidator validator, ModelStateDictionary modelState)
    {
        await ValidateWithValidatorAsync(validator, modelState);

        if (!modelState.IsValid)
        {
            HandleInvalidModelState(modelState);
        }

        return modelState.IsValid;
    }

    private async Task ValidateWithValidatorAsync(MatchResultValidator validator, ModelStateDictionary modelState)
    {
        await validator.CheckAsync(CancellationToken.None);

        foreach (var fact in validator.GetFailedFacts())
        {
            if (fact.Type == FactType.Critical || fact.Type == FactType.Error)
            {
                HandleCriticalOrErrorFact(fact, validator, modelState);
            }
            else
            {
                HandleWarningFact(fact, modelState);
            }
        }
    }

    private void HandleCriticalOrErrorFact(Fact<MatchResultValidator.FactId> fact, MatchResultValidator validator, ModelStateDictionary modelState)
    {
        if (fact.FieldNames.Contains(nameof(MatchResultValidator.Model.RealStart)) ||
            fact.FieldNames.Contains(nameof(MatchResultValidator.Model.RealEnd)))
        {
            AddModelErrorForMatchDate(fact, modelState);
        }
        else if (fact.Id == MatchResultValidator.FactId.SetsValidatorSuccessful)
        {
            AddModelErrorForSets(validator, modelState);
        }
        else if (fact.Id == MatchResultValidator.FactId.MatchPointsAreValid)
        {
            AddModelErrorForMatchPoints(fact, modelState);
        }
        else
        {
            AddModelErrorForOtherFacts(fact, modelState);
        }
    }

    private void AddModelErrorForMatchDate(Fact<MatchResultValidator.FactId> fact, ModelStateDictionary modelState)
    {
        modelState.AddModelError(nameof(EnterResultViewModel.MatchDate), fact.Message);
    }

    private void AddModelErrorForSets(MatchResultValidator validator, ModelStateDictionary modelState)
    {
        foreach (var setsError in validator.SetsValidator.GetFailedFacts())
        {
            if (setsError.Id == SetsValidator.FactId.AllSetsAreValid)
            {
                foreach (var singleSet in validator.SetsValidator.SingleSetErrors)
                {
                    var fieldName = singleSet.FactId == SingleSetValidator.FactId.SetPointsAreValid
                        ? $"{nameof(SetPoints)}-{singleSet.SequenceNo - 1}"
                        : $"{nameof(BallPoints)}-{singleSet.SequenceNo - 1}";

                    modelState.AddModelError(fieldName, $"{string.Format(_localizer["Set #{0}"], singleSet.SequenceNo)}: {singleSet.ErrorMessage}");
                }
            }
            else
            {
                modelState.AddModelError("", setsError.Message);
            }
        }
    }

    private void AddModelErrorForMatchPoints(Fact<MatchResultValidator.FactId> fact, ModelStateDictionary modelState)
    {
        modelState.AddModelError($"{nameof(HomePoints)}", fact.Message);
    }

    private void AddModelErrorForOtherFacts(Fact<MatchResultValidator.FactId> fact, ModelStateDictionary modelState)
    {
        modelState.AddModelError(string.Empty, fact.Message);
    }

    private void HandleWarningFact(Fact<MatchResultValidator.FactId> fact, ModelStateDictionary modelState)
    {
        modelState.AddModelError(string.Empty, fact.Message);
        IsWarning = true;
    }

    private void HandleInvalidModelState(ModelStateDictionary modelState)
    {
        var newHash = ComputeInputHash();
        if (IsWarning && OverrideWarnings && newHash == Hash)
        {
            modelState.Clear();
            IsWarning = false;
        }
        OverrideWarnings = false;
        Hash = newHash;
    }

    public class MatchResultMessage
    {
        public long? MatchId { get; set; }
        public bool ChangeSuccess { get; set; }
    }
}
