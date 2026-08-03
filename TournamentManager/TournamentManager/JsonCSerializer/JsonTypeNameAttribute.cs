namespace TournamentManager.JsonCSerializer;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class JsonTypeNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
