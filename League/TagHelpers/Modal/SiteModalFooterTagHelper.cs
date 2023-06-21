using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace League.TagHelpers.Modal;

/// <summary>
/// The modal-footer portion of Bootstrap modal dialog
/// </summary>
[HtmlTargetElement(TagHelperAttributeName, ParentTag = "modal")]
public class SiteModalFooterTagHelper : TagHelper
{
    public const string TagHelperAttributeName = "modal-footer";

    /// <summary>
    /// Whether or not to show a button to dismiss the dialog. 
    /// Default: <c>true</c>
    /// </summary>
    public bool ShowDismiss { get; set; } = true;

    /// <summary>
    /// The text to show on the Dismiss button
    /// Default: Cancel
    /// </summary>
    public string DismissText { get; set; } = "Cancel";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll(TagHelperAttributeName);

        if (ShowDismiss)
        {
            output.PreContent.AppendFormat(@"<button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>{0}</button>", DismissText);
        }
        var childContent = await output.GetChildContentAsync();
        var footerContent = new DefaultTagHelperContent();
        if (ShowDismiss)
        {
            _ = footerContent.AppendFormat(@"<button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>{0}</button>", DismissText);
        }
        _ = footerContent.AppendHtml(childContent);
        var modalContext = (ModalContext)context.Items[typeof(SiteModalTagHelper)];
        modalContext.Footer = footerContent;
        output.SuppressOutput();
    }
}
