﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.Match;
using TournamentManager.ModelValidators;

namespace League.Models.MatchViewModels
{
    public class EnterResultViewModel
    {
        private readonly IStringLocalizer<EnterResultViewModel> _localizer;
        private readonly int _maxNumberOfSets;

        public EnterResultViewModel()
        { }

        public EnterResultViewModel(TournamentEntity tournament, RoundEntity round, 
            MatchEntity match, MatchRuleEntity matchRule, IList<TeamInRoundEntity> teamInRound, 
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter)
        {
            Tournament = tournament ?? throw new ArgumentNullException(nameof(tournament));
            Round = round ?? throw new ArgumentNullException(nameof(round));
            Match = match ?? throw new ArgumentNullException(nameof(match));
            Opponent = new Opponent(
                teamInRound.FirstOrDefault(o => o.TeamId == match.HomeTeamId)?.TeamNameForRound ?? throw new ArgumentNullException(nameof(teamInRound)),
                teamInRound.FirstOrDefault(o => o.TeamId == match.GuestTeamId)?.TeamNameForRound ?? throw new ArgumentNullException(nameof(teamInRound))); ;
            TimeZoneConverter = timeZoneConverter ?? throw new ArgumentNullException(nameof(timeZoneConverter));

            _localizer = CreateModelStringLocalizer();

            _maxNumberOfSets = matchRule.MaxNumOfSets();
            MapEntityToFormFields();
        }

        private StringLocalizer<EnterResultViewModel> CreateModelStringLocalizer()
        {
            // no need for any params if using a StringLocalizer<T>
            var options = Microsoft.Extensions.Options.Options.Create(new LocalizationOptions());
            var factory = new ResourceManagerStringLocalizerFactory(options, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            // Note: The resource is also used by System.ComponentModel.DataAnnotations
            return new StringLocalizer<EnterResultViewModel>(factory);
        }

        public void MapEntityToFormFields()
        {
            if (Match.RealStart.HasValue && Match.RealEnd.HasValue)
            {
                var startDate = TimeZoneConverter.ToZonedTime(Match.RealStart);
                var endDate = TimeZoneConverter.ToZonedTime(Match.RealEnd);
                MatchDate = startDate.DateTimeOffset.Date;
                MatchTimeFrom = startDate.DateTimeOffset.TimeOfDay;
                MatchTimeTo = endDate.DateTimeOffset.TimeOfDay;
                Remarks = Match.Remarks;
            }
            else
            {
                MatchDate = TimeZoneConverter.ToZonedTime(Match.PlannedStart)?.DateTimeOffset.Date;
            }

            Match.Sets.Sort((int)SetFieldIndex.SequenceNo, ListSortDirection.Ascending);
            foreach (var set in Match.Sets)
            {
                Sets.Add(new PointResultNullable(set.HomeBallPoints, set.GuestBallPoints));
            }

            while (Sets.Count < _maxNumberOfSets)
            {
                Sets.Add(new PointResultNullable(null, null));
            }

            Id = Match.Id;

            Remarks = Match.Remarks;
        }

        public void MapFormFieldsToEntity()
        {
            // Save match date/time to entity
            if (MatchDate.HasValue && MatchTimeFrom.HasValue && MatchTimeTo.HasValue)
            {
                var period = new DateTimePeriod(MatchDate?.Add(MatchTimeFrom.Value), MatchDate?.Add(MatchTimeTo.Value));
                Match.SetRealStart(TimeZoneConverter.ToUtc(period.Start), period.Duration());
            }
            else
            {
                Match.SetRealStart(null, TimeSpan.Zero);
            }

            // Add sets to entity
            Match.Sets.Clear(true);
            for (var i = 0; i < Sets.Count; i++)
            {
                // sets where home and guest ball points are NULL, will be ignored
                if (Sets[i].Home.HasValue || Sets[i].Guest.HasValue)
                {
                    // home or guest NULL values are invalidated with -1
                    Match.Sets.Add(new SetEntity { MatchId = Match.Id, SequenceNo = i + 1, HomeBallPoints = Sets[i].Home ?? -1, GuestBallPoints = Sets[i].Guest ?? -1 });
                }
            }

            // point calculation must run before validation because of tie-break handling
            Match.Sets.CalculateSetPoints(Round.SetRule, Round.MatchRule);
            Match.Remarks = Remarks;
            Match.ChangeSerial++;
            Match.IsComplete = true;
        }

        #region *** Form fields ***

        [HiddenInput]
        public long Id { get; set; }

        [HiddenInput]
        public string Hash { get; set; }

        /// <summary>
        /// <see cref="ReturnUrl"/> is needed to return to either fixtures or results,
        /// when the cancel button is clicked.
        /// </summary>
        [HiddenInput]
        public string ReturnUrl { get; set; }

        [Display(Name = "Match date")]
        public DateTime? MatchDate { get; set; }

        [Display(Name = "Match start time")]
        public TimeSpan? MatchTimeFrom { get; set; }

        [Display(Name = "Match end time")]
        public TimeSpan? MatchTimeTo { get; set; }

        [Display(Name = "Ignore notices")]
        public bool OverrideWarnings { get; set; }

        [Display(Name="Remarks")]
        [MaxLength(2000)]
        public string Remarks { get; set; }

        public List<PointResultNullable> Sets { get; set; } = new List<PointResultNullable>();

        #endregion

        public TournamentEntity Tournament { get; set; }

        public RoundEntity Round { get; set; }

        public MatchEntity Match { get;}

        public Opponent Opponent { get; }


        /// <summary>
        /// If <c>true</c>, validation messages are warnings, which can be overridden with <see cref="OverrideWarnings"/>.
        /// </summary>
        public bool IsWarning { get; set; }
 
        /// <summary>
        /// The TimeZoneConverter, used by the razor view
        /// </summary>
        public Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter { get; }

        private string ComputeInputHash()
        {
            return Axuno.Tools.Hash.Md5.GetHash(string.Join(string.Empty,
                Id.ToString(),
                MatchDate?.Ticks.ToString() ?? string.Empty,
                MatchTimeFrom?.Ticks.ToString() ?? string.Empty,
                MatchTimeTo?.ToString() ?? string.Empty,
                string.Join(string.Empty, Sets.Select(s => s.Home.ToString() + s.Guest.ToString())), 
                Remarks));
        }

        public async Task<bool> ValidateAsync(MatchResultValidator validator, ModelStateDictionary modelState)
        {
            await validator.CheckAsync(CancellationToken.None);

            foreach (var fact in validator.GetFailedFacts())
            {
                if (fact.Type == FactType.Critical || fact.Type == FactType.Error)
                {
                    if (fact.FieldNames.Contains(nameof(MatchResultValidator.Model.RealStart)) 
                        || fact.FieldNames.Contains(nameof(MatchResultValidator.Model.RealEnd)))
                    {
                        modelState.AddModelError(nameof(EnterResultViewModel.MatchDate), fact.Message);
                    }

                    if (fact.Id == MatchResultValidator.FactId.SetsValidatorSuccessful)
                    {
                        foreach (var setsError in validator.SetsValidator.GetFailedFacts())
                        {
                            // This the the hint for existing errors in single sets
                            if (setsError.Id == SetsValidator.FactId.AllSetsAreValid)
                            {
                                foreach (var singleSet in validator.SetsValidator.SingleSetErrors)
                                {
                                    modelState.AddModelError($"set-{singleSet.SequenceNo-1}", string.Concat(string.Format(_localizer["Set #{0}"], singleSet.SequenceNo), ": ", singleSet.ErrorMessage));
                                }
                            }
                            else
                            {
                                // Errors about sets in general
                                modelState.AddModelError("", setsError.Message);
                            }
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

        public class MatchResultMessage
        {
            public long MatchId { get; set; }
            public bool ChangeSuccess { get; set; }
        }
    }
}
