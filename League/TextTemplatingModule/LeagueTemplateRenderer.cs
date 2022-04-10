using System;
using System.Collections.Generic;
using Axuno.TextTemplating;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Scriban;
using Scriban.Parsing;
using Scriban.Syntax;
#nullable enable
namespace League.TextTemplatingModule
{
    /// <summary>
    /// Custom template renderer.
    /// </summary>
    /// <remarks>
    /// Inject as transient.
    /// </remarks>
    public class LeagueTemplateRenderer : TemplateRenderer
    {
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter? _timeZoneConverter;
        private readonly TournamentManager.MultiTenancy.ITenantContext _tenantContext;

        /// <summary>
        /// Gets or sets the <see cref="VariableNotFoundAction"/> to be taken if variables cannot be found from the model or other scopes.
        /// Defaults to the Scriban <see cref="Template.RenderAsync(Scriban.TemplateContext)"/> default, which corresponds to <see cref="RenderErrorAction.ThrowError"/>.
        /// </summary>
        protected RenderErrorAction VariableNotFoundAction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MemberNotFoundAction"/> to be taken if a member cannot be found from the model.
        /// Defaults to the Scriban <see cref="Template.RenderAsync(Scriban.TemplateContext)"/> default, which corresponds to <see cref="RenderErrorAction.LeaveEmpty"/>.
        /// </summary>
        protected RenderErrorAction MemberNotFoundAction { get; set; }

        public LeagueTemplateRenderer(
            ITemplateContentProvider templateContentProvider,
            ITemplateDefinitionManager templateDefinitionManager,
            IStringLocalizerFactory stringLocalizerFactory,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
            TournamentManager.MultiTenancy.ITenantContext tenantContext,
            IOptions<LeagueTemplateRendererOptions> options) : base(templateContentProvider,
            templateDefinitionManager, stringLocalizerFactory)
        {
            _timeZoneConverter = timeZoneConverter;
            _tenantContext = tenantContext;
            VariableNotFoundAction = options.Value.VariableNotFoundAction;
            MemberNotFoundAction = options.Value.MemberNotFoundAction;
        }

        protected override TemplateContext CreateScribanTemplateContext(
            TemplateDefinition templateDefinition,
            Dictionary<string, object> globalContext,
            object? model = null)
        {
            var baseContext = base.CreateScribanTemplateContext(templateDefinition, globalContext, model);
            baseContext.BuiltinObject.SetValue(NetDateTimeFunctions.DateVariable.Name, new NetDateTimeFunctions(_timeZoneConverter), true);
            baseContext.BuiltinObject.SetValue("org_ctx", _tenantContext.OrganizationContext, true);
            baseContext.BuiltinObject.SetValue("tenant_ctx", _tenantContext, true);
            // Culture may have been changed by the CultureSwitcher of TextTemplating - this is now implemented in Axuno.TextTemplating:
            // baseContext.PushCulture(System.Globalization.CultureInfo.CurrentCulture);
            
            // Add the predefined global variable for the current culture, which is also accessible from the 'Layout' templates
            // (same as with the global 'content' variable)
            baseContext.BuiltinObject["culture"] = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            // The ScriptObject is already registered as global
            baseContext.StrictVariables = true;  // Note: has no effect for child members
            baseContext.MemberRenamer = member => member.Name; // Use .NET object without changing the cases
            // will be called if the member of an object could not be found:
            baseContext.TryGetMember = (TemplateContext context, SourceSpan span, object target, string member,
                out object value) =>
            {
                switch (MemberNotFoundAction)
                {
                    case RenderErrorAction.MaintainToken: 
                        value = $"{{{{ {member} }}}}";
                        return true;
                    case RenderErrorAction.OutputErrorInResult:
                        value = $"## Member '{member}' not found ##";
                        return true;
                    // default behavior of Scriban: 
                    case RenderErrorAction.LeaveEmpty:
                        value = string.Empty;
                        return false;
                    case RenderErrorAction.ThrowError:
                        throw new Exception($"Member '{member}' not found.");
                    default:
                        throw new InvalidOperationException($"Unexpected value {MemberNotFoundAction} for {nameof(MemberNotFoundAction)}");
                }
            };
            // will be called if the variable could not be found from any scope
            baseContext. 
                    TryGetVariable =
                (TemplateContext context, SourceSpan span, ScriptVariable variable, out object value) =>
                {
                    switch (VariableNotFoundAction)
                    {
                        case RenderErrorAction.MaintainToken: 
                            value = $"{{{{ {variable} }}}}";
                            return true;
                        case RenderErrorAction.OutputErrorInResult:
                            value = $"## Variable '{variable}' not found from any scope ##";
                            return true;
                        case RenderErrorAction.LeaveEmpty:
                            value = string.Empty;
                            return true;
                        // default behavior of Scriban, throws a Scriban.Syntax.ScriptRuntimeException:
                        case RenderErrorAction.ThrowError:
                            value = string.Empty;
                            return false;
                        default:
                            throw new InvalidOperationException($"Unexpected value {VariableNotFoundAction} for {nameof(VariableNotFoundAction)}");
                    }
                };

            return baseContext;
        }
    }
}
