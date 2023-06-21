using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace League.TagHelpers.Modal;

public class ModalContext
{
    public IHtmlContent? Body { get; set; }
    public IHtmlContent? Footer { get; set; }
}

/// <summary>
/// A Bootstrap modal dialog
/// </summary>
[HtmlTargetElement(TagHelperAttributeName)]
[RestrictChildren("modal-body", "modal-footer")]
public class SiteModalTagHelper : TagHelper
{
    public const string TagHelperAttributeName = "modal";

    /// <summary>
    /// The title of the modal
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The Id of the modal
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// With these CSS classes, the width of the modal can be set,
    /// e.g. "modal-lg" or "mw-100 w-75" 
    /// </summary>
    public string? DialogClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var modalContext = new ModalContext();
        context.Items.Add(typeof(SiteModalTagHelper), modalContext);

        _ = await output.GetChildContentAsync().ConfigureAwait(false);
        var closeButton = "<button type='button' class='btn btn-secondary btn-close float-end' data-bs-dismiss='modal' aria-label='Close'></button>";

        var modalDialogClass = string.Join(' ', "modal-dialog", string.IsNullOrWhiteSpace(DialogClass) ? string.Empty : DialogClass);

        var template =
            $@"<div class='{modalDialogClass}' role='document'>
    <div class='modal-content'>";
        if (!string.IsNullOrWhiteSpace(Title))
        {
            template +=
                $@"<div class ='modal-header'>
    <h4 class ='modal-title' id='{context.UniqueId}Label'>{Title}</h4>
    {closeButton}
</div>";
        }

        template += "<div class='modal-body'>";
        template += string.IsNullOrWhiteSpace(Title) ? $"{closeButton}" : string.Empty;
        template += "<div class='container-fluid px-0'>"; // use the grid within the modal

        output.TagName = "div";
        output.Attributes.SetAttribute("role", "dialog");
        output.Attributes.SetAttribute("id", Id);
        output.Attributes.SetAttribute("aria-labelledby", $"{context.UniqueId}Label");
        output.Attributes.SetAttribute("tabindex", "-1");
        var classNames = "modal fade";  // adding class "fade" may cause issues with <div class="modal-backdrop"/> not being removed, which keeps the background blurred
        if (output.Attributes.ContainsName("class"))
        {
            classNames = $"{output.Attributes["class"].Value} {classNames}";
        }
        output.Attributes.SetAttribute("class", classNames);
        output.Content.AppendHtml(template);
        if (modalContext.Body != null)
        {
            output.Content.AppendHtml(modalContext.Body);
        }
        output.Content.AppendHtml("</div>"); // container fluid
        output.Content.AppendHtml("</div>"); // modal body
        if (modalContext.Footer != null)
        {
            output.Content.AppendHtml("<div class='modal-footer flex-wrap'>"); // default is "flex-nowrap"
            output.Content.AppendHtml(modalContext.Footer);
            output.Content.AppendHtml("</div>");
        }

        output.Content.AppendHtml("</div>"); // modal content
        output.Content.AppendHtml("</div>"); // modal dialog
    }
}
