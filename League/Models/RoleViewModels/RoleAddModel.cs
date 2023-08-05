using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.RoleViewModels;

public class RoleAddModel
{
    [Display(Name = "Email of team member to be added")]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string? UserEmail { get; set; }
    [Display(Name = "Role of new team member")]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string? ClaimType { get; set; }
    [HiddenInput]
    public long TeamId { get; set; }
    [HiddenInput]
    public string? ReturnUrl { get; set; }
}
