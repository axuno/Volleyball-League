using System.ComponentModel.DataAnnotations;

namespace League.Models.ManageViewModels;

public class EditPhoneViewModel
{
    [RegularExpression("^(\\+{1}|[\\d]{1})[0-9 \\-/\\+]+$", ErrorMessage = "Only digits, blanks and -/+ are allowed")]
    [Display(Name = "Primary Phone No.")]
    public string PhoneNumber { get; set; }
}