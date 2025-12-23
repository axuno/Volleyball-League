using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ManageViewModels;

public class PersonalDetailsViewModel
{
    [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [DataType(DataType.Text)]
    [Display(Name = "Salutation")]
    [RegularExpression("[mfu]", ErrorMessage = "Invalid 'Salutation' value")]
    public string Gender { get; set; } ="u";

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
    public string? Nickname { get; set; }

    public static char[] Genders => ['u', 'f', 'm'];
}
