using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers.Modal;

/// <summary>
/// Sets the element as the item that will toggle the specified Bootstrap modal dialog by
/// adding Bootstrap attributes <code>data-toggle="site-ajax-modal" data-url="/target/url"</code>.
/// Buttons or other elements can be used to toggle the modal by adding the
/// <strong>site-toggle-modal-ajax</strong> tag helper attribute and referencing the id of the modal.
/// Requires Site.ModalForm.js to be loaded.
/// </summary>
// <button type="button" class="btn btn-sm btn-primary" data-toggle="site-ajax-modal" data-url="@Url.Action(nameof(Manage.ChangeUserName))">
[HtmlTargetElement(Attributes = TagHelperAttributeName + ",site-data-url")]
public class SiteModalToggleAjaxTagHelper : TagHelper
{
    public const string TagHelperAttributeName = "site-toggle-modal-ajax";

    /// <summary>
    /// The URL of the partial view to load into a Bootstrap Modal.
    /// </summary>
    [HtmlAttributeName("site-data-url")]
    public string DataUrl { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll(TagHelperAttributeName); // remove tag helper's attribute
        output.Attributes.SetAttribute("data-toggle", "site-ajax-modal");
        output.Attributes.SetAttribute("data-url", DataUrl);
    }
}