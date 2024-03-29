﻿using League.Templates.Email;

namespace League.Emailing.TemplateModels;

/// <summary>
/// The model is used for templates <see cref="TemplateName.PasswordResetTxt"/>, <see cref="TemplateName.PasswordResetHtml"/>,
/// <see cref="TemplateName.PleaseConfirmEmailTxt"/>, <see cref="TemplateName.PleaseConfirmEmailHtml"/>, <see cref="TemplateName.ConfirmNewPrimaryEmailTxt"/>,
/// <see cref="TemplateName.ConfirmNewPrimaryEmailHtml"/>
/// </summary>
public class ChangeUserAccountModel
{
    public string Email { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
}
