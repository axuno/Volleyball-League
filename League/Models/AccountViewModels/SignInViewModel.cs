using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using League.Resources;

namespace League.Models.AccountViewModels
{
    public class SignInViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [Display(Name = "Email or username")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EmailOrUsername { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
