﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using League.Resources;

namespace League.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [Display(Name = "Email or username")]
        public string EmailOrUsername { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessageResourceName = nameof(DataAnnotationResource.PropertyValueRequired), ErrorMessageResourceType = typeof(DataAnnotationResource))]
        [StringLength(200, MinimumLength = 6, ErrorMessageResourceName = nameof(DataAnnotationResource.StringLengthBetween), ErrorMessageResourceType = typeof(DataAnnotationResource))] // Min Length Should come from Identity.Password.Options
        [DataType(DataType.Text)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Confirm new password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
