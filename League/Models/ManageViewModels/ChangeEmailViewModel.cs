using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ManageViewModels;

public class ChangeEmailViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResource.EmailAddressInvalid), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [Display(Name = "Primary Email")]
    public string Email { get; set; } = string.Empty;
}
