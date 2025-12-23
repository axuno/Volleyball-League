using League.Resources;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace League.Models.AccountViewModels;

public class ExternalSignConfirmationViewModel
{
    [Display(Name = "Email")]
    [BindNever]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Salutation")]
    [RegularExpression("[mfu]", ErrorMessage = "Invalid 'Salutation' value")]
    public string Gender { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [Display(Name = "Nickname")]
    public string Nickname { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [Display(Name = "Primary phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public static char[] Genders => ['u', 'f', 'm'];
}
