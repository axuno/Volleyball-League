using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers;
// Similar to:
// https://github.com/dpaquette/TagHelperSamples/blob/master/TagHelperSamples/src/TagHelperSamples.Authorization/AuthorizeResourceTagHelper.cs
// Resource based authorization:
// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased?tabs=aspnetcore2x

/// <summary>
/// Resource based authorization allows you to evaluate policies and requirements for the current user against a particular resource.
/// </summary>
/// <example>
/// Show an "edit" link only if the policy for editing documents is fulfilled:
/// <code>&lt;a href="/edit" site-resource="document" site-policy="EditDocument"&gt;&lt;/a&gt;</code>
/// </example>
[HtmlTargetElement("*", Attributes = TagHelperAttributeName)]
public class SiteAuthorizeResourceTagHelper : TagHelper
{
    public const string TagHelperAttributeName = "site-authorize-resource";

    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SiteAuthorizeResourceTagHelper(IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets or sets the policy name that determines access to the HTML block.
    /// </summary>
    [HtmlAttributeName("site-policy")]
    public string? Policy { get; set; }

    /// <summary>
    /// Gets or sets a comma delimited list of requirements that are allowed to access the HTML block.
    /// </summary>
    [HtmlAttributeName("site-requirement")]
    public IAuthorizationRequirement? Requirement { get; set; }

    /// <summary>
    /// Gets or sets the resource to be authorized against a particular policy.
    /// </summary>
    [HtmlAttributeName("site-resource")]
    public object? Resource { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Remove the attribute "site-authorize-resource" from the output
        output.Attributes.RemoveAll(TagHelperAttributeName);

        if (Resource == null)
        {
            throw new ArgumentException("Resource cannot be null");
        }
        if (string.IsNullOrWhiteSpace(Policy) && Requirement == null)
        {
            throw new ArgumentException("Either Policy or Requirement must be specified");

        }
        if (!string.IsNullOrWhiteSpace(Policy) && Requirement != null)
        {
            throw new ArgumentException("Policy and Requirement cannot be specified at the same time");
        }

        AuthorizationResult authorizeResult;

        if (!string.IsNullOrWhiteSpace(Policy))
        {
            authorizeResult = await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext!.User, Resource, Policy);
        }
        else if (Requirement != null)
        {
            authorizeResult =
                await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext!.User, Resource,
                    Requirement);
        }
        else
        {
            throw new ArgumentException("Either Policy or Requirement must be specified");
        }

        if (!authorizeResult.Succeeded)
        {
            output.SuppressOutput();
        }
    }
}
