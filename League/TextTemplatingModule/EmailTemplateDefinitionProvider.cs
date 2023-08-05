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
                    "/Email/ConfirmNewPrimaryEmailHtml.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.ConfirmNewPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/ConfirmNewPrimaryEmailTxt.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ConfirmTeamApplicationTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/ConfirmTeamApplicationTxt.tpl", //template content path
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ContactFormTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/ContactFormTxt.tpl", //template content folder
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
                    "/Email/PasswordResetHtml.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PasswordResetTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/PasswordResetTxt.tpl", //template content path
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
                    "/Email/PleaseConfirmEmailHtml.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PleaseConfirmEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/PleaseConfirmEmailTxt.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ChangeFixtureTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/ChangeFixtureTxt.tpl", //template content folder
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
                    "/Email/NotifyCurrentPrimaryEmailHtml.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.NotifyCurrentPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    "/Email/NotifyCurrentPrimaryEmailTxt.tpl", //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ResultEnteredTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/ResultEnteredTxt.tpl", //template content folder
                isInlineLocalized: true
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.AnnounceNextMatchTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/AnnounceNextMatchTxt", //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.RemindMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/RemindMatchResultTxt", //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.UrgeMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                "/Email/UrgeMatchResultTxt", //template content folder
                isInlineLocalized: false
            )
        );

        context.Add(
            new TemplateDefinition(
                TemplateName.HtmlLayout,
                isLayout: true
            ).WithVirtualFilePath(
                "/Email/HtmlLayout.tpl", //template content path
                isInlineLocalized: true
            )
        );
    }
}