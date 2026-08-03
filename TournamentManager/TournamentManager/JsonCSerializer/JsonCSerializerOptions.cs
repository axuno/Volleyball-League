using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TournamentManager.JsonCSerializer;

/// <summary>
/// Options for JsonCSerializer.
/// </summary>
public sealed class JsonCSerializerOptions
{
    /// <summary>
    /// Indentation string. Default: two spaces.
    /// </summary>
    public string Indent { get; set; } = "  ";

    /// <summary>
    /// Line break used for output. Default: Environment.NewLine.
    /// </summary>
    public string NewLine { get; set; } = Environment.NewLine;

    /// <summary>
    /// If false, properties with null values are omitted.
    /// </summary>
    public bool WriteNullValues { get; set; } = true;

    /// <summary>
    /// If true, enum values are written as strings. Recommended for configuration files.
    /// </summary>
    public bool WriteEnumsAsStrings { get; set; } = true;

    /// <summary>
    /// If true, JsonPropertyNameAttribute is respected.
    /// </summary>
    public bool RespectJsonPropertyName { get; set; } = true;

    /// <summary>
    /// If true, JsonPropertyOrderAttribute is respected.
    /// </summary>
    public bool RespectJsonPropertyOrder { get; set; } = true;

    /// <summary>
    /// If true, properties marked with JsonIgnoreAttribute are skipped.
    /// </summary>
    public bool RespectJsonIgnore { get; set; } = true;

    /// <summary>
    /// If true, comments attached to the root type are written before the root object.
    /// </summary>
    public bool WriteRootComment { get; set; } = true;

    /// <summary>
    /// If true, the root object will be wrapped in a JSON object with the type name as the property.
    /// If false, only the root object's contents are serialized.
    /// </summary>
    public bool WriteRootAsNamedProperty { get; set; } = true;

    /// <summary>
    /// Gets or sets the <see cref="JsonSerializerOptions"/> used for serialization and deserialization.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; set; } = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters =
        {
            // null for naming policy with default to PascalCase
            new JsonStringEnumConverter(null, allowIntegerValues: true)
        }
    };
}
