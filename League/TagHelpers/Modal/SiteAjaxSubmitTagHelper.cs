using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers.Modal;

/// <summary>
/// Sets the save button to submit a form with Ajax.
/// Requires Site.ModalForm.js to be loaded.
/// </summary>
// <button id="saveBtn" type="submit" class="btn btn-primary" site-data-save="modal">Save</button>
[HtmlTargetElement(Attributes = TagHelperAttributeName)]
public class SiteAjaxSubmitTagHelper : TagHelper
{
    public const string TagHelperAttributeName = "site-ajax-submit";

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll(TagHelperAttributeName); // remove tag helper's attribute
        if (!output.Attributes.ContainsName("type")) output.Attributes.SetAttribute("type", "submit");
        output.Attributes.SetAttribute("site-data", "submit");
        if (!output.Attributes.ContainsName("id")) output.Attributes.SetAttribute("id", TagHelperAttributeName);
    }
}