using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers
{
    // Similar to this tag helper written by Rick Strahl:
    // https://github.com/dpaquette/TagHelperSamples/blob/master/TagHelperSamples/src/TagHelperSamples.Bootstrap/AlertTagHelper.cs


    /// <summary>
    /// TagHelper to display a BootStrap Alert box and FontAwesome icon.
    /// 
    /// Message and Header values can be assigned from model values using
    /// standard Razor expressions.
    /// 
    /// The Helper only displays content when message or header are set
    /// otherwise the content is not displayed, so when binding to your 
    /// model and the model value is empty nothing displays.
    /// </summary>
    /// <remarks>
    /// Expects the following css class definitions are set:
    /// .site-alert {
    ///     &amp; div:first-of-type {
    ///         margin-left: 1rem !important;
    ///     }
    /// }
    /// 
    /// .validation-summary-valid {
    ///     display: none !important;
    /// }
    /// </remarks>
    [HtmlTargetElement(TagHelperAttributeName)]
    public class SiteAlertTagHelper : TagHelper
    {
        public const string TagHelperAttributeName = "site-alert";

        private AlertType _type = AlertType.Info;

        /// <summary>
        /// Acceptable alert types.
        /// </summary>
        public enum AlertType
        {
            Success,
            Info,
            Warning,
            Danger,
            Primary,
            Secondary
        }

        /// <summary>
        /// The <see cref="AlertType"/> to display.
        /// </summary>
        public AlertType Type
        {
            get
            {
                return _type;
            }

            set
            {
                if ((uint) value > Enum.GetNames(typeof(AlertType)).Length - 1)
                {
                    throw new InvalidOperationException(@$"Undefined {nameof(AlertType)} value");
                }

                _type = value;
            }
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// FontAwesome icon name including the fa- prefix, that will override the default icons for <see cref="AlertType"/>s.
        /// Example: fa-lightbulb-o, 
        /// If none is specified - "warning" is used
        /// To force no icon use "none"
        /// </summary>
        [HtmlAttributeName("icon")]
        public string IconClass { get; set; }

        /// <summary>
        /// Defines the size of the FontAwesome Icon. Default is fa-2x.
        /// </summary>
        [HtmlAttributeName("icon-size-class")]
        public string IconSizeClass { get; set; } = "fa-2x";

        /// <summary>
        /// CSS class to append to the default CSS classes of <see cref="SiteAlertTagHelper"/>.
        /// </summary>
        [HtmlAttributeName("class")]
        public string CssClass { get; set; }

        /// <summary>
        /// If true displays a close icon to close the alert.
        /// </summary>
        [HtmlAttributeName("dismissible")]
        public bool Dismissible { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var alertDivClass = "site-alert d-flex align-items-center alert {0} rounded px-3";

            if (string.IsNullOrWhiteSpace(IconSizeClass))
            {
                IconSizeClass = "fa-2x";
            }

            if (!string.IsNullOrWhiteSpace(CssClass))
            {
                alertDivClass = string.Join(' ', alertDivClass, CssClass);
            }

            var iconClassDefault = string.Empty;

            switch (_type)
            {
                case AlertType.Success:
                    iconClassDefault = string.Join(' ', "fas fa-check-circle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-success");
                    break;
                case AlertType.Info:
                    iconClassDefault = string.Join(' ', "fas fa-info-circle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-info");
                    break;
                case AlertType.Warning:
                    iconClassDefault = string.Join(' ', "fas fa-exclamation-triangle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-warning");
                    break;
                case AlertType.Danger:
                    iconClassDefault = string.Join(' ', "fas fa-times-circle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-danger");
                    break;
                case AlertType.Primary:
                    iconClassDefault = string.Join(' ', "fas fa-info-circle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-primary");
                    break;
                case AlertType.Secondary:
                    iconClassDefault = string.Join(' ', "fas fa-info-circle", IconSizeClass);
                    CssClass = string.Format(alertDivClass, "alert-secondary");
                    break;
            }

            if (string.IsNullOrWhiteSpace(IconClass)) IconClass = iconClassDefault;

            // this is the html (or text) inside the site-alert tag
            // it is displayed right to the icon
            var content = await output.GetChildContentAsync();

            var sb = new StringBuilder();

            if (Dismissible)
                sb.Append(
                    "<button type =\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\">\r\n" +
                    "   <span aria-hidden=\"true\">&times;</span>\r\n" +
                    "</button>\r\n");

            sb.Append(content.GetContent());

            if (Dismissible && !CssClass.Contains("alert-dismissible"))
                CssClass += " alert-dismissible";

            // use "div" instead of "site-alert"
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", CssClass);
            output.Attributes.Add("role", "alert");

            output.Content.SetHtmlContent($"<i class=\"{string.Join(' ', IconClass, IconSizeClass)}\"></i>\r\n<div>{sb}</div>\r\n");
        }
    }
}
