using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TournamentManager.JsonCSerializer;

/// <summary>
/// Writes configuration objects as JSONC, i.e. JSON with // comments.
/// Comments are created from <see cref="JsonCommentAttribute"/>.
/// <para/>
/// For deserialization, comments are ignored
/// and the JSON is parsed by <see cref="JsonSerializer"/>.
/// </summary>
public static class JsonCSerializer<T>
{
    /// <summary>
    /// Serializes the given value to a JSONC string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="options">The serialization options.</param>
    /// <returns>The JSONC string representation of the value.</returns>
    public static string Serialize(
        T value,
        JsonCSerializerOptions? options = null)
    {
        options ??= new JsonCSerializerOptions();
        var context = new WriterContext(options);
        
        if (options.WriteRootAsNamedProperty)
        {
            WriteWrappedRoot(context, value, typeof(T), options);
        }
        else
        {
            WriteDirectRoot(context, value, typeof(T), options);
        }

        return context.ToString();
    }

    /// <summary>
    /// Deserializes a JSON or JSONC string into an object of type T.
    /// </summary>
    /// <param name="jsonC">The JSONC string to deserialize.</param>
    /// <param name="sectionName">The optional section name to deserialize.</param>
    /// <param name="options">The deserialization options.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
    public static T Deserialize(
        string jsonC,
        string? sectionName = null,
        JsonCSerializerOptions? options = null)
    {
        options ??= new JsonCSerializerOptions();

        using var document = JsonDocument.Parse(
            jsonC,
            new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip
            });

        JsonElement section;

        if (sectionName is not null)
        {
            section = document.RootElement.GetProperty(sectionName);
        }
        else
        {
            // Auto-detect: if root has single property matching type name, unwrap it
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                var properties = document.RootElement.EnumerateObject().ToList();
                if (properties.Count == 1 && properties[0].Name == typeof(T).Name)
                {
                    section = properties[0].Value;
                }
                else
                {
                    section = document.RootElement;
                }
            }
            else
            {
                section = document.RootElement;
            }
        }

        return section.Deserialize<T>(options.SerializerOptions)
               ?? throw new InvalidOperationException("Failed to deserialize input string");
    }

    /// <summary>
    /// Deserializes a JSONC string using the root type name as the section name.
    /// </summary>
    /// <param name="jsonC">The JSONC string to deserialize.</param>
    /// <param name="options">The deserialization options.</param>
    /// <returns>The deserialized object of type T.</returns>
    /// <exception cref="InvalidOperationException">Thrown if deserialization fails.</exception>
    public static T Deserialize<TSection>(
        string jsonC,
        JsonCSerializerOptions? options = null)
        where TSection : class
    {
        return Deserialize(jsonC, typeof(TSection).Name, options);
    }

    /// <summary>
    /// Writes the root value wrapped in an outer object with a property name.
    /// </summary>
    private static void WriteWrappedRoot(
        WriterContext context,
        T value,
        Type type,
        JsonCSerializerOptions options)
    {
        context.WriteRaw("{" + options.NewLine);

        if (options.WriteRootComment && type.GetCustomAttribute<JsonCommentAttribute>() is { } comment)
        {
            context.WriteComment(comment.Text, 1);
        }

        context.WriteIndent(1);
        context.WriteJsonString(GetRootPropertyName(type));
        context.WriteRaw(": ");
        context.WriteValue(value, type, 1);
        context.WriteRaw(options.NewLine + "}");
    }

    /// <summary>
    /// Writes the root value directly without wrapping.
    /// </summary>
    private static void WriteDirectRoot(
        WriterContext context,
        T value,
        Type type,
        JsonCSerializerOptions options)
    {
        if (options.WriteRootComment && type.GetCustomAttribute<JsonCommentAttribute>() is { } comment)
        {
            context.WriteComment(comment.Text, 0);
        }

        context.WriteValue(value, type, 0);
    }

    private static string GetRootPropertyName(Type type)
    {
        // Use JsonTypeNameAttribute if present
        var jsonTypeName = type.GetCustomAttribute<JsonTypeNameAttribute>();
        return jsonTypeName is not null ? jsonTypeName.Name : type.Name;
    }

    /// <summary>
    /// Class for writing JSONC with comments and indentation.
    /// </summary>
    private sealed class WriterContext(JsonCSerializerOptions options)
    {
        private readonly StringBuilder _sb = new();
        private readonly JsonSerializerOptions _jsonOptions = options.SerializerOptions;

        private readonly HashSet<object> _visited =
            new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

        public override string ToString()
        {
            return _sb.ToString();
        }

        public void WriteRaw(string text)
        {
            _sb.Append(text);
        }

        public void WriteValue(object? value, Type declaredType, int level)
        {
            if (value is null)
            {
                _sb.Append("null");
                return;
            }

            // Check for collections first (before delegating to JsonSerializer)
            if (IsDictionary(value))
            {
                WriteDictionary(value, level);
                return;
            }

            if (value is IEnumerable enumerable and not string)
            {
                WriteArray(enumerable, level);
                return;
            }

            // Check if it's a complex object that needs custom serialization
            var actualType = value.GetType();
            if (!IsComplexObject(actualType))
            {
                // Let JsonSerializer handle all simple values (primitives, strings, DateTime, etc.)
                _sb.Append(JsonSerializer.Serialize(value, declaredType, _jsonOptions));
                return;
            }

            WriteObject(value, actualType, level);
        }

        /// <summary>
        /// Determines if a type is a complex object requiring custom serialization.
        /// </summary>
        private static bool IsComplexObject(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            // Simple types that JsonSerializer handles well
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) ||
                type == typeof(decimal) || type == typeof(Guid) ||
                type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) || type == typeof(DateOnly) || type == typeof(TimeOnly))
            {
                return false;
            }

            // Everything else is a complex object
            return true;
        }

        private void WriteObject(object value, Type type, int level)
        {
            CheckForCycle(value);

            _sb.Append('{');
            _sb.Append(options.NewLine);

            var properties = GetWritableProperties(type)
                .Select(p => new PropertyWriteInfo(
                    Property: p,
                    JsonName: GetJsonPropertyName(p),
                    Comment: p.GetCustomAttribute<JsonCommentAttribute>()?.Text,
                    Value: p.GetValue(value)))
                .Where(x => options.WriteNullValues || x.Value is not null)
                .ToList();

            for (var i = 0; i < properties.Count; i++)
            {
                var item = properties[i];

                if (!string.IsNullOrWhiteSpace(item.Comment))
                {
                    WriteComment(item.Comment, level + 1);
                }

                WriteIndent(level + 1);
                WriteJsonString(item.JsonName);
                _sb.Append(": ");

                WriteValue(
                    item.Value,
                    item.Property.PropertyType,
                    level + 1);

                if (i < properties.Count - 1)
                {
                    _sb.Append(',');
                }

                _sb.Append(options.NewLine);
            }

            WriteIndent(level);
            _sb.Append('}');

            _visited.Remove(value);
        }

        private void WriteArray(IEnumerable enumerable, int level)
        {
            CheckForCycle(enumerable);

            var items = enumerable.Cast<object?>().ToList();

            _sb.Append('[');

            if (items.Count > 0)
            {
                _sb.Append(options.NewLine);

                for (var i = 0; i < items.Count; i++)
                {
                    WriteIndent(level + 1);

                    var item = items[i];
                    WriteValue(item, item?.GetType() ?? typeof(object), level + 1);

                    if (i < items.Count - 1)
                    {
                        _sb.Append(',');
                    }

                    _sb.Append(options.NewLine);
                }

                WriteIndent(level);
            }

            _sb.Append(']');

            _visited.Remove(enumerable);
        }

        private void WriteDictionary(object dictionary, int level)
        {
            CheckForCycle(dictionary);

            var entries = GetDictionaryEntries(dictionary).ToList();

            _sb.Append('{');

            if (entries.Count > 0)
            {
                _sb.Append(options.NewLine);

                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];

                    WriteIndent(level + 1);

                    WriteJsonString(Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? "");
                    _sb.Append(": ");

                    WriteValue(
                        entry.Value,
                        entry.Value?.GetType() ?? typeof(object),
                        level + 1);

                    if (i < entries.Count - 1)
                    {
                        _sb.Append(',');
                    }

                    _sb.Append(options.NewLine);
                }

                WriteIndent(level);
            }

            _sb.Append('}');

            _visited.Remove(dictionary);
        }

        public void WriteComment(string comment, int level)
        {
            foreach (var line in comment.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                WriteIndent(level);
                _sb.Append("// ").Append(line.Trim()).Append(options.NewLine);
            }
        }

        public void WriteIndent(int level)
        {
            for (var i = 0; i < level; i++)
            {
                _sb.Append(options.Indent);
            }
        }

        public void WriteJsonString(string value)
        {
            _sb.Append(JsonSerializer.Serialize(value, _jsonOptions));
        }

        private IReadOnlyList<PropertyInfo> GetWritableProperties(Type type)
        {
            var query = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetMethod is not null)
                .Where(p => p.GetIndexParameters().Length == 0);

            if (options.RespectJsonIgnore)
            {
                query = query.Where(p =>
                {
                    var ignore = p.GetCustomAttribute<JsonIgnoreAttribute>();

                    if (ignore is null)
                    {
                        return true;
                    }

                    return ignore.Condition != JsonIgnoreCondition.Always;
                });
            }

            if (options.RespectJsonPropertyOrder)
            {
                query = query
                    .OrderBy(p => p.GetCustomAttribute<JsonPropertyOrderAttribute>()?.Order ?? 0)
                    .ThenBy(p => p.MetadataToken);
            }
            else
            {
                // Properties in declaration order
                query = query.OrderBy(p => p.MetadataToken);
            }

            return query.ToList();
        }

        private string GetJsonPropertyName(PropertyInfo property)
        {
            if (!options.RespectJsonPropertyName) return property.Name;

            var jsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>();

            if (jsonName is not null)
            {
                return jsonName.Name;
            }

            return property.Name;
        }

        private static bool IsDictionary(object value)
        {
            if (value is IDictionary)
            {
                return true;
            }

            var type = value.GetType();

            return type
                .GetInterfaces()
                .Any(i =>
                    i.IsGenericType
                    && (
                        i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                        || i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    ));
        }

        private static IEnumerable<DictionaryEntryInfo> GetDictionaryEntries(object dictionary)
        {
            if (dictionary is IDictionary nonGenericDictionary)
            {
                foreach (DictionaryEntry entry in nonGenericDictionary)
                {
                    yield return new DictionaryEntryInfo(entry.Key, entry.Value);
                }

                yield break;
            }

            foreach (var item in (IEnumerable) dictionary)
            {
                var itemType = item.GetType();

                var keyProperty = itemType.GetProperty("Key");
                var valueProperty = itemType.GetProperty("Value");

                if (keyProperty is null || valueProperty is null)
                {
                    continue;
                }

                yield return new DictionaryEntryInfo(
                    keyProperty.GetValue(item)!, //NOSONAR
                    valueProperty.GetValue(item));
            }
        }

        private void CheckForCycle(object value)
        {
            if (value is string)
            {
                return;
            }

            var type = value.GetType();

            if (type.IsValueType)
            {
                return;
            }

            if (!_visited.Add(value))
            {
                throw new InvalidOperationException(
                    $"Circular reference detected while writing JSONC. Type: {type.FullName}");
            }
        }

        private sealed record PropertyWriteInfo(
            PropertyInfo Property,
            string JsonName,
            string? Comment,
            object? Value);

        private sealed record DictionaryEntryInfo(
            object Key,
            object? Value);
    }
}
