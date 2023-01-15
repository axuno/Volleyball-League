using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ManageViewModels;

public class EditEmail2ViewModel
{
    [EmailAddress(ErrorMessageResourceName = nameof(DataAnnotationResource.EmailAddressInvalid), ErrorMessageResourceType = typeof(DataAnnotationResource))]
    [Display(Name = "Additional Email")]
    public string Email2 { get; set; }
}