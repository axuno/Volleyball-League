using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers.Modal;

/// <summary>
/// Sets the element as the item that will toggle the specified Bootstrap modal dialog by
/// adding Bootstrap attributes <code>data-bs-toggle="modal" data-bs-target="#target-id"</code>.
/// Buttons or other elements can be used to toggle the modal by adding the
/// <strong>site-toggle-modal</strong> tag helper attribute and referencing the id of the modal.
/// </summary>
[HtmlTargetElement(Attributes = ModalTargetAttributeName)]
public class SiteModalToggleTagHelper : TagHelper
{
    public const string ModalTargetAttributeName = "site-toggle-modal";

    /// <summary>
    /// The id of the modal that will be toggled by this element.
    /// </summary>
    [HtmlAttributeName(ModalTargetAttributeName)]
    public string? ToggleModal { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-bs-toggle", "modal");
        output.Attributes.SetAttribute("data-bs-target", ToggleModal != null && ToggleModal.StartsWith('#') ? ToggleModal : $"#{ToggleModal}");
    }
}
