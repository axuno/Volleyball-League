using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.AccountViewModels;

public class CreateAccountViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResource.EmailAddressInvalid), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Result of math task in the image")]
    public string Captcha { get; set; } = string.Empty;
}
