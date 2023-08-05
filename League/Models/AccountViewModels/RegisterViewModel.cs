using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.AccountViewModels;

public class RegisterViewModel
{
    [HiddenInput]
    public string? Code { get; set; }

    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [StringLength(200, MinimumLength = 6, ErrorMessageResourceName = nameof(DataAnnotationResource.StringLengthBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))] // Min length should come from Identity.Password.Options
    [DataType(DataType.Text)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and the confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Salutation")]
    [RegularExpression("[mfu]", ErrorMessage = "Invalid 'Salutation' value")]
    public string Gender { get; set; } = "u";

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Nickname")]
    public string Nickname { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [Display(Name = "Primary phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    public static char[] Genders => new[] {'u', 'f', 'm'};
}
