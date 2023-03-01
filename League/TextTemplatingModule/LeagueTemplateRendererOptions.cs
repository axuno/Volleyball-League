
namespace League.TextTemplatingModule;

public class LeagueTemplateRendererOptions
{
    /// <summary>
    /// Gets or sets the <see cref="VariableNotFoundAction"/> to be taken if variables cannot be found from the model or other scopes.
    /// Defaults to the Scriban <see cref="Scriban.Template.RenderAsync(Scriban.TemplateContext)"/> default, which corresponds to <see cref="RenderErrorAction.ThrowError"/>.
    /// </summary>
    public RenderErrorAction VariableNotFoundAction { get; set; } = RenderErrorAction.ThrowError;

    /// <summary>
    /// Gets or sets the <see cref="MemberNotFoundAction"/> to be taken if a member cannot be found from the model.
    /// Defaults to the Scriban <see cref="Scriban.Template.RenderAsync(Scriban.TemplateContext)"/> default, which corresponds to <see cref="RenderErrorAction.LeaveEmpty"/>.
    /// </summary>
    public RenderErrorAction MemberNotFoundAction { get; set; } = RenderErrorAction.LeaveEmpty;
}