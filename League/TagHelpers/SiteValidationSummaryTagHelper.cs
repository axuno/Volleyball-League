using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Org.BouncyCastle.Utilities.Collections;
using System.Collections.Generic;

namespace League.TagHelpers
{
    /// <summary>
    /// A replacement for "asp-validation-summary" tag helper.
    /// Designed for Bootstrap 4 and FontAwesome icons.
    /// </summary>
    /// <remarks>
    /// Expects the following css class definitions are set:
    /// .validation-summary-errors {
    ///     &amp; ul {
    ///         padding-left: 1rem !important;
    ///         margin-bottom: 0 !important;
    ///     }
    ///     &amp; div:first-of-type {
    ///         margin-left: 1rem !important;
    ///     }
    /// }
    /// 
    /// .validation-summary-valid {
    ///     display: none !important;
    /// }
    /// </remarks>
    [HtmlTargetElement(TagHelperAttributeName, Attributes = Show, TagStructure = TagStructure.NormalOrSelfClosing)]
    public class SiteValidationSummaryTagHelper : TagHelper
    {
        public const string TagHelperAttributeName = "site-validation-summary";

        private const string Show = "show";
        private ValidationSummary _validationSummaryEnum;

        /// <summary>
        /// Creates a new BootstrapValidationSummaryTagHelper.
        /// </summary>
        /// <param name="generator">The <see cref="T:Microsoft.AspNetCore.Mvc.ViewFeatures.IHtmlGenerator" />.</param>
        public SiteValidationSummaryTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// The css classes for the DIV element containing the validation summary.
        /// Default: d-flex align-items-center alert alert-danger rounded px-3
        /// </summary>
        public string Class { get; set; } = "d-flex align-items-center alert alert-danger rounded px-3";

        /// <summary>
        /// The FontAwesome Icon css classes.
        /// Default: fas fa-times-circle fa-2x
        /// </summary>
        public string IconClass { get; set; } = "fas fa-times-circle fa-2x";

        /// <summary>
        /// If <c>false</c>, the summary is displayed with design "danger",
        /// if <c>true</c> the summary is displayed as "warning".
        /// </summary>
        public bool Warning { get; set; } = false;

        /// <summary>
        /// If <c>true</c>, error messages will be unique,
        /// e.g. if the same error message apply for more than one field.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool UniqueErrorText { get; set; } = false;

        [HtmlAttributeNotBound] [ViewContext] public ViewContext ViewContext { get; set; }

        [HtmlAttributeNotBound] protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// If <see cref="F:Microsoft.AspNetCore.Mvc.Rendering.ValidationSummary.All" /> or <see cref="F:Microsoft.AspNetCore.Mvc.Rendering.ValidationSummary.ModelOnly" />, appends a validation
        /// summary. Otherwise (<see cref="F:Microsoft.AspNetCore.Mvc.Rendering.ValidationSummary.None" />, the default), this tag helper does nothing.
        /// </summary>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown if setter is called with an undefined <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.ValidationSummaryTagHelper.ValidationSummary" /> value e.g.
        /// <c>(ValidationSummary)23</c>.
        /// </exception>
        [HtmlAttributeName(Show)]
        public ValidationSummary ValidationSummary
        {
            get => _validationSummaryEnum;
            set
            {
                if ((uint) value > 2U)
                    throw new ArgumentException($"Undefined {nameof(ValidationSummary)} value",
                        nameof(ValidationSummary));
                _validationSummaryEnum = value;
            }
        }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="P:Microsoft.AspNetCore.Mvc.TagHelpers.ValidationSummaryTagHelper.ValidationSummary" /> is <see cref="F:Microsoft.AspNetCore.Mvc.Rendering.ValidationSummary.None" />.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (ValidationSummary == ValidationSummary.None)
                return;
            
            // bug in asp.net core 2.1: headertag argument is never used
            var validationSummaryHtml = Generator.GenerateValidationSummary(ViewContext,
                ValidationSummary == ValidationSummary.ModelOnly, (string) null, (string) null, (object) null);

            if (validationSummaryHtml == null || !validationSummaryHtml.HasInnerHtml)
            {
                output.SuppressOutput();
            }
            else
            {
                if (Warning)
                {
                    Class = Class.Replace("alert-danger", "alert-warning");
                    IconClass = IconClass.Replace("fa-times-circle", "fa-exclamation-triangle");
                }

                // use "div" instead of "alert"
                output.TagName = "div";
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Attributes.Add("class", Class);
                output.MergeAttributes(validationSummaryHtml);

                // this is the html (or text) inside the BootstrapValidationSummary alert tag
                // it is used as the description for the list of property errors
                var content = await output.GetChildContentAsync();
                
                // the <ul> list of model errors and property errors
                var ulHtml = new StringWriter();
                if (UniqueErrorText)
                {
                    // use custom list of error messages
                    GetUniqueValidationErrorsHtml().WriteTo(ulHtml, HtmlEncoder.Default);
                }
                else
                {
                    // use default validation summary error messages
                    validationSummaryHtml.InnerHtml.WriteTo(ulHtml, HtmlEncoder.Default);
                }
                
                output.Content.SetHtmlContent($"<i class=\"{IconClass}\"></i>\r\n<div>{content.GetContent()}\r\n{ulHtml}</div>");

                // as of asp.net core 2.1/3.1: noop
                await base.ProcessAsync(context, output);
            }
        }

        private HtmlContentBuilder GetUniqueValidationErrorsHtml()
        {
            // Create list of unique errors
            var modelErrorMessages = new HashSet<string>(ViewContext.ModelState
                .Where(entry =>
                    ValidationSummary != ValidationSummary.ModelOnly || string.IsNullOrEmpty(entry.Key))
                .SelectMany(x => x.Value.Errors)
                .Select(e => e.ErrorMessage));
            
            var cb = new HtmlContentBuilder();

            if (!modelErrorMessages.Any()) return cb;

            _ = cb.AppendHtml("<ul>");
            foreach (var msg in modelErrorMessages)
            {
                _ = cb.AppendHtml($"<li>{msg}</li>");
            }
            _ = cb.AppendHtml("</ul>");
            return cb;
        }
    }
}
