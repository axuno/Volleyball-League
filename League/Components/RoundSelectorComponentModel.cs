using System.ComponentModel.DataAnnotations;
using League.Resources;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.DAL.EntityClasses;

namespace League.Components;

public class RoundSelectorComponentModel
{
    /// <summary>
    /// The list of rounds with their round type.
    /// </summary>
    [BindNever]
    public IList<RoundEntity> RoundWithTypeList { get; set; } = new List<RoundEntity>();

    /// <summary>
    /// If the <see cref="SelectedRoundId"/> is <see langword="null"/> and
    /// <see cref="EnforceExplicitSelection"/> is <see langword="true"/>,
    /// the selection dropdown will display &quot;Please select...&quot;,
    /// until a value from the list is selected.
    /// </summary>
    [BindNever]
    public bool EnforceExplicitSelection { get; set; }

    /// <summary>
    /// Gets or sets the ID which should be shown as selected.
    /// </summary>
    [Display(Name = "Round")]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public long? SelectedRoundId { get; set; }

    /// <summary>
    /// Gets or sets the prefix for name (e.g. "prefix.field") and id (e.g. "prefix_field") of input fields.
    /// </summary>
    [BindNever]
    public string HtmlFieldPrefix { get; set; } = string.Empty;

    /// <summary>
    /// If true, the select field is show, else only the round description is rendered
    /// </summary>
    [BindNever]
    public bool ShowSelector { get; set; }

                
    /// <summary>
    /// The <see cref="TournamentEntity.Id"/> to use for the selection of rounds.
    /// </summary>
    [BindNever]
    public long TournamentId { get; set; }
}
