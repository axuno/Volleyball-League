using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace League.TagHelpers;
// Similar to:
// https://github.com/dpaquette/TagHelperSamples/blob/master/TagHelperSamples/src/TagHelperSamples.Authorization/AuthorizeTagHelper.cs

/// <summary>
/// The SiteAuthorize tag helper allows to render blocks of HTML only for users who are authorized based on ASP.NET Core Authorization Roles and Policies.
/// For example, a section of HTML might be rendered only if a user has the <i>Admin</i> role, or if the user is not authenticated.
/// Since everything is evaluated on the server, this tag helper provides an easy way to ensure that users only see the HTML sections they are supposed to see.
/// If roles AND policy arguments are used together, BOTH conditions must be met.
/// </summary>
/// <remarks>
/// The SiteAuthorize tag helper works in the same way as the Authorize filter works for controllers and action Methods.
/// </remarks>
/// <example>
/// Usage for authorized users:
/// &lt;div site-authorize site-roles="RoleName"&gt;Content-only-for-authorized-users&lt;/div&gt;
/// Usage for anonymous users only:
/// &lt;div site-authorize site-anonymous-only="true">For anonymous users.&lt;div&gt;
/// </example>
[HtmlTargetElement(Attributes = TagHelperAttributeName)]
public class SiteAuthorizeTagHelper : TagHelper, IAuthorizeData
{
    public const string TagHelperAttributeName = "site-authorize";

    private readonly IAuthorizationPolicyProvider _policyProvider;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SiteAuthorizeTagHelper(IHttpContextAccessor httpContextAccessor, IAuthorizationPolicyProvider policyProvider, IPolicyEvaluator policyEvaluator)
    {
        _httpContextAccessor = httpContextAccessor;
        _policyProvider = policyProvider;
        _policyEvaluator = policyEvaluator;
    }

    #region *** IAuthorizeData ***

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the HTML block.
    /// </summary>
    [HtmlAttributeName("site-roles")]
    public string? Roles { get; set; }

    /// <summary>
    /// Gets or sets the policy name that determines access to the HTML block.
    /// </summary>
    [HtmlAttributeName("site-policy")]
    public string? Policy { get; set; }
        
    /// <summary>
    /// Gets or sets a comma delimited list of schemes from which user information is constructed.
    /// </summary>
    [HtmlAttributeName("site-authentication-schemes")]
    public string? AuthenticationSchemes { get; set; }

    #endregion

    /// <summary>
    /// Gets or sets whether the HTML block shall be rendered for authenticated users (<c>false</c>)
    /// or anonymous access (<c>true</c>). If set to true, Roles and Policy will not be considered.
    /// </summary>
    [HtmlAttributeName("site-anonymous-only")]
    public bool AnonymousOnly { get; set; } = false;
        
    ///<inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var policy = await AuthorizationPolicy.CombineAsync(_policyProvider, new[] { this });
        var authenticateResult = await _policyEvaluator.AuthenticateAsync(policy!, _httpContextAccessor.HttpContext!);
        output.Attributes.RemoveAll(TagHelperAttributeName);  // remove tag helper's attribute

        if (AnonymousOnly)
        {
            if (authenticateResult.Succeeded)
            {
                output.SuppressOutput();
                return;
            }

            return;
        }

        var authorizeResult = await _policyEvaluator.AuthorizeAsync(policy!, authenticateResult, _httpContextAccessor.HttpContext!, null);

        if (!authorizeResult.Succeeded)
        {
            output.SuppressOutput();
        }
    }
}
