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
        const string path = "/Email/";
        const string ext = ".tpl";

        context.Add(
            new TemplateDefinition(
                    TemplateName.ConfirmNewPrimaryEmailHtml, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName,
                    layout: TemplateName.HtmlLayout)
                .WithVirtualFilePath(
                    path + nameof(TemplateName.ConfirmNewPrimaryEmailHtml) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.ConfirmNewPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    path + nameof(TemplateName.ConfirmNewPrimaryEmailTxt) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ConfirmTeamApplicationTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.ConfirmTeamApplicationTxt) + ext, //template content path
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ContactFormTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.ContactFormTxt) + ext, //template content folder
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
                    path + nameof(TemplateName.PasswordResetHtml) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PasswordResetTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    path + nameof(TemplateName.PasswordResetTxt) + ext, //template content path
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
                    path + nameof(TemplateName.PleaseConfirmEmailHtml) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.PleaseConfirmEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    path + nameof(TemplateName.PleaseConfirmEmailTxt) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ChangeFixtureTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.ChangeFixtureTxt) + ext, //template content folder
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
                    path + nameof(TemplateName.NotifyCurrentPrimaryEmailHtml) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                    TemplateName.NotifyCurrentPrimaryEmailTxt, 
                    localizationResource: typeof(EmailResource),
                    defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName)
                .WithVirtualFilePath(
                    path + nameof(TemplateName.NotifyCurrentPrimaryEmailTxt) + ext, //template content path
                    isInlineLocalized: true
                )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ResultEnteredTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.ResultEnteredTxt) + ext, //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.ResultRemovedTxt,
                localizationResource: typeof(EmailResource),
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.ResultRemovedTxt) + ext, //template content folder
                isInlineLocalized: true
            )
        );

        context.Add(
            new TemplateDefinition(
                name: TemplateName.AnnounceNextMatchTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.AnnounceNextMatchTxt) + ext, //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.RemindMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.RemindMatchResultTxt) + ext, //template content folder
                isInlineLocalized: false
            )
        );
            
        context.Add(
            new TemplateDefinition(
                name: TemplateName.UrgeMatchResultTxt,
                localizationResource: null,
                defaultCultureName: CultureInfo.DefaultThreadCurrentCulture?.Name ?? fallbackCultureName
            ).WithVirtualFilePath(
                path + nameof(TemplateName.UrgeMatchResultTxt) + ext, //template content folder
                isInlineLocalized: false
            )
        );

        context.Add(
            new TemplateDefinition(
                TemplateName.HtmlLayout,
                isLayout: true
            ).WithVirtualFilePath(
                path + nameof(TemplateName.HtmlLayout) + ext, //template content path
                isInlineLocalized: true
            )
        );
    }
}
