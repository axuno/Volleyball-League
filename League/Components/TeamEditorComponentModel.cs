﻿using System.ComponentModel.DataAnnotations;
using League.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.EntityClasses;

namespace League.Components;

public class TeamEditorComponentModel
{
    [HiddenInput]
    [Required]
    public long Id { get; set; }
    [HiddenInput]
    [Required]
    public bool IsNew { get; set; }
    [Display(Name = "Team name")]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string Name { get; set; } = string.Empty;
    [Display(Name = "Club name")]
    public string ClubName { get; set; } = string.Empty;
    [Display(Name = "Weekday of home matches")]
    public int? MatchDayOfWeek { get; set; }
    [Display(Name = "Start time of home matches")]
    public TimeOnly? MatchTime { get; set; }

    /// <summary>
    /// Gets or sets the prefix for name (e.g. "prefix.field") and id (e.g. "prefix_field") of input fields.
    /// </summary>
    [BindNever]
    public string HtmlFieldPrefix { get; set; } = string.Empty;

    public void MapFormFieldsToEntity(TeamEntity teamEntity)
    {
        teamEntity.IsNew = IsNew;
        teamEntity.Id = Id;
        teamEntity.Name = Name;
        teamEntity.ClubName = ClubName;
        teamEntity.MatchTime = MatchTime?.ToTimeSpan();
        teamEntity.MatchDayOfWeek = MatchDayOfWeek;
    }

    public void MapEntityToFormFields(TeamEntity teamEntity)
    {
        IsNew = teamEntity.IsNew;
        Id = teamEntity.Id;
        Name = teamEntity.Name;
        ClubName = teamEntity.ClubName;
        MatchDayOfWeek = teamEntity.MatchDayOfWeek;
        MatchTime = teamEntity.MatchTime != null ? TimeOnly.FromTimeSpan((TimeSpan) teamEntity.MatchTime) : null;
    }
}
