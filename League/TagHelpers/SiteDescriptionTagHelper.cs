using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;span&gt; elements with an <c>site-description-for</c> attribute.
    /// Adds an <c>id</c> attribute and sets the content of the &lt;span&gt; with the <c>Description</c> property from the model data annotation DisplayAttribute.
    /// </summary>
    /// <example>
    /// <c>The model:</c>
    /// [Display(Name = &quot;Phone&quot;, Description=&quot;This is my description&quot;)]
    /// public string Phone { get; set; }
    /// <c>The <see cref="ITagHelper"/>:</c>
    /// &lt;span&gt;site-description-for=&quot;Phone&quot;" class=&quot;form-text text-muted&quot;&lt;/span&gt;
    /// </example>
    [HtmlTargetElement("span", Attributes = DescriptionForAttributeName)]
    public class DescriptionTagHelper : TagHelper
    {
        private const string DescriptionForAttributeName = "site-description-for";

        /// <summary>
        /// Creates a new <see cref="DescriptionTagHelper"/>.
        /// </summary>
        /// <param name="generator">The <see cref="IHtmlGenerator"/>.</param>
        public DescriptionTagHelper(IHtmlGenerator generator)
        {
            Generator = generator;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        [HtmlAttributeNotBound] 
        [ViewContext] 
        public ViewContext ViewContext { get; set; }

        protected IHtmlGenerator Generator { get; }

        /// <summary>
        /// An expression to be evaluated against the current model.
        /// </summary>
        [HtmlAttributeName(DescriptionForAttributeName)]
        public ModelExpression DescriptionFor { get; set; }

        /// <inheritdoc />
        /// <remarks>There is no output if <see cref="DescriptionFor"/> is <c>null</c>.</remarks>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context)); 
            _ = output ?? throw new ArgumentNullException(nameof(output));

            var metadata = DescriptionFor.Metadata ??
                           throw new InvalidOperationException($"No metadata provided ({DescriptionForAttributeName})");

            if (string.IsNullOrWhiteSpace(metadata.Description)) return Task.CompletedTask;
            
            output.Attributes.SetAttribute("id", metadata.PropertyName + "-description");
            output.Content.SetContent(metadata.Description);
            output.TagMode = TagMode.StartTagAndEndTag;

            return Task.CompletedTask;
        }
    }
}