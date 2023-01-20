using System.ComponentModel.DataAnnotations;

namespace League.Models.ManageViewModels;

public class EditPhone2ViewModel
{
    [RegularExpression("^(\\+{1}|[\\d]{1})[0-9 \\-/\\+]+$", ErrorMessage = "Only digits, blanks and -/+ are allowed")]
    [Display(Name = "Additional Phone No.")]
    public string PhoneNumber2 { get; set; } = string.Empty;
}
