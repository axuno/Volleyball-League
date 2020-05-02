﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using League.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.TeamApplicationViewModels
{
    public class ApplicationSelectTeamModel
    {
        [BindProperty]
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [Display(Name = "Existing team")]
        public long? SelectedTeamId { get; set; }

        [BindNever]
        public List<LatestTeamTournamentRow> TeamsManagedByUser { get; set; } = new List<LatestTeamTournamentRow>();

        [BindNever]
        public string TournamentName { get; set; }
    }
}