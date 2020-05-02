using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [Display(Name = "Email or username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmailOrUsername { get; set; }
    }
}
