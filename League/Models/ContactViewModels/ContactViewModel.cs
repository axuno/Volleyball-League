using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ContactViewModels;

public class ContactViewModel
{
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResource.EmailAddressInvalid), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Salutation")]
    [RegularExpression("[mfu]", ErrorMessage = "Invalid 'Salutation' value")]
    public string? Gender { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "First name")]
    public string? FirstName { get; set; }
        
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Last name")]
    public string? LastName { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [Display(Name = "Subject")]
    [DataType(DataType.Text)]
    public string? Subject { get; set; }

    [Display(Name = "Message")]
    public string? Message { get; set; }

    [Display(Name = "Result of math task in the image")]
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    public string? Captcha { get; set; }
}
