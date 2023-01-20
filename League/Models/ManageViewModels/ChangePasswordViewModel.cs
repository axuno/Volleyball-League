using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ManageViewModels;

public class ChangePasswordViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [StringLength(200, MinimumLength = 6, ErrorMessageResourceName = nameof(DataAnnotationResource.StringLengthBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))] // Min Length Should come from Identity.Password.Options
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
