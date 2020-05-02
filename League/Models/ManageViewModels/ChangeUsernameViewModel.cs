using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.ManageViewModels
{
    public class ChangeUsernameViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [Display(Name = "Username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Username { get; set; }
    }
}
