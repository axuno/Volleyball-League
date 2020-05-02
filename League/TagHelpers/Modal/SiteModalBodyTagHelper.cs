using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace League.TagHelpers.Modal
{
    /// <summary>
    /// The modal-body portion of a Bootstrap modal dialog
    /// </summary>
    [HtmlTargetElement(TagHelperAttributeName, ParentTag = "modal")]
    public class ModalBodyTagHelper : TagHelper
    {
        public const string TagHelperAttributeName = "modal-body";

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll(TagHelperAttributeName);
            var childContent = await output.GetChildContentAsync();
            var modalContext = (ModalContext)context.Items[typeof(ModalTagHelper)];
            modalContext.Body = childContent;
            output.SuppressOutput();
        }
    }
}
