using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers;

/// <summary>
/// The <see cref="SiteScriptTagHelper"/> will render scripts after <c>DOMContentLoaded</c>.
/// This way, scripts can be included in a <see cref="Microsoft.AspNetCore.Mvc.ViewComponent"/>.
/// </summary>
[HtmlTargetElement("script")]
public class SiteScriptTagHelper : TagHelper
{

    /// <summary>
    /// Execute script inline.
    /// </summary>
    [HtmlAttributeName("site-inline")]
    public bool Inline { get; set; }

    /// <summary>
    /// Execute script only once the document is loaded.
    /// </summary>
    [HtmlAttributeName("site-on-content-loaded")]
    public bool OnContentLoaded { get; set; } = false;

    /// <summary>
    /// Execute script only once an ajax call is loaded.
    /// </summary>
    [HtmlAttributeName("site-on-ajax-complete")]
    public bool OnAjaxComplete { get; set; } = false;

    /// <summary>
    /// Execute script only once the Bootstrap 4 Model is shown (event 'shown.bs.modal' is fired).
    /// </summary>
    [HtmlAttributeName("site-on-modal-shown")]
    public bool OnBootstrapModelShown { get; set; } = false;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var jsFunctionName = "F_" + Guid.NewGuid().ToString("N");

        if (!OnContentLoaded && !OnBootstrapModelShown && !Inline)
        {
            base.Process(context, output);
        }
        else
        {
            var sb = new System.Text.StringBuilder();
            var content = await output.GetChildContentAsync();
            var javascript = content.GetContent();
            sb.Append("function ");
            sb.Append(jsFunctionName);
            sb.Append("()  {");
            sb.Append(javascript);
            sb.Append("}\n");

            if (OnContentLoaded)
            {
                sb.Append("document.addEventListener('DOMContentLoaded',");
                sb.Append(jsFunctionName);
                sb.Append(");\n");
            }
            if (OnAjaxComplete)
            {
                sb.Append("$(document).ajaxComplete(");
                sb.Append(jsFunctionName);
                sb.Append(");\n");
            }
            if (OnBootstrapModelShown)
            {
                sb.Append("$(document).on('shown.bs.modal',");
                sb.Append(jsFunctionName);
                sb.Append(");\n");
            }
            if (Inline)
            {
                sb.Append(jsFunctionName);
                sb.Append("();\n");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}