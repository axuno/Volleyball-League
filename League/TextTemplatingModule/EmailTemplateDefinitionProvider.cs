using Axuno.TextTemplating;
using League.Templates.Email;
using League.Templates.Email.Localization;

namespace League.TextTemplatingModule;

// inject as transient
public class EmailTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        const string fallbackCultureName = "en";

        context.Add(
            new TemplateDefinition(
                    TemplateName.ConfirmNewPrimaryEmailHtml, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName,
                    layout: TemplateName.HtmlLayout)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.ConfirmNewPrimaryEmailHtml) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.ConfirmNewPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.ConfirmNewPrimaryEmailTxt) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ConfirmTeamApplicationTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.ConfirmTeamApplicationTxt) + ".tpl", //template content path
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ContactFormTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.ContactFormTxt) + ".tpl", //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PasswordResetHtml, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName,
                    layout: TemplateName.HtmlLayout)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.PasswordResetHtml) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PasswordResetTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.PasswordResetTxt) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PleaseConfirmEmailHtml, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName,
                    layout: TemplateName.HtmlLayout)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.PleaseConfirmEmailHtml) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PleaseConfirmEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.PleaseConfirmEmailTxt) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ChangeFixtureTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.ChangeFixtureTxt) + ".tpl", //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.NotifyCurrentPrimaryEmailHtml, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName,
                    layout: TemplateName.HtmlLayout)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.NotifyCurrentPrimaryEmailHtml) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.NotifyCurrentPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/" + nameof(TemplateName.NotifyCurrentPrimaryEmailTxt) + ".tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ResultEnteredTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.ResultEnteredTxt) + ".tpl", //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ResultRemovedTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.ResultRemovedTxt) + ".tpl", //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.AnnounceNextMatchTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.AnnounceNextMatchTxt) + ".tpl", //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.RemindMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.RemindMatchResultTxt) + ".tpl", //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.UrgeMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.UrgeMatchResultTxt) + ".tpl", //template content folder
                isInlineLocalized: false
            )
        );

        context.Add(
            new TemplateDefinition(
                TemplateName.HtmlLayout,
                isLayout: true
            ).WithVirtualFilePath(
                "/Email/" + nameof(TemplateName.HtmlLayout) + ".tpl", //template content path
                isInlineLocalized: true
            )
        );
    }
}
