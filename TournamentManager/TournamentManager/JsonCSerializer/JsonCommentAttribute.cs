namespace TournamentManager.JsonCSerializer;

/// <summary>
/// Adds a JSONC comment before a configuration property.
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public sealed class JsonCommentAttribute(string text) : Attribute
{
    public string Text { get; } = text;
}
