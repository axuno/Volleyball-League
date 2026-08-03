using System.Text.Json.Serialization;
using NUnit.Framework;
using TournamentManager.JsonCSerializer;

namespace TournamentManager.Tests.JsonCSerializer;

[TestFixture]
public class JsonCSerializerTests
{
    [JsonComment("A sample person object for testing")]
    private class Person
    {
        [JsonComment("The person's full name")]
        public string Name { get; set; } = string.Empty;

        [JsonComment("The person's age in years")]
        public int Age { get; set; }

        [JsonComment("Whether the person is active")]
        public bool IsActive { get; set; }
    }

    [JsonComment(
        """
        A test class with multiline comment
        using raw string literals
        """)]
    private class MultilineCommentClass
    {
        [JsonComment(
            """
            This is a multiline property comment
            that spans multiple lines
            """)]
        public string TestProperty { get; set; } = "test value";

        [JsonComment("Single line comment")]
        public int Number { get; set; } = 42;
    }

    [JsonComment("Class with a dictionary property")]
    private class ClassContainingDictionary
    {
        [JsonComment("This is a dictionary comment")]
        public Dictionary<int, string> TestDictionary { get; set; } = new()
        {
            { 1, "One" },
            { 2, "Two" },
            { 3, "Three" }
        };

        [JsonComment("Just a number")]
        public int Number { get; set; } = 42;
    }

    [JsonComment("Class with custom class / property name")]
    [JsonTypeName("CustomConfig")]
    private class ConfigWithCustomName
    {
        [JsonComment("Setting value")]
        [JsonPropertyName("Config")]
        public string Setting { get; set; } = "default";

        public object? NullValue { get; set; } = null;
    }

    private class TestClassWithList
    {
        public List<int> Numbers { get; set; } = [];
    }

    [Test]
    public void Serialize_WithoutRootAsNamedProperty_OutputsObjectDirectly()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true
        };

        var options = new JsonCSerializerOptions
        {
            WriteRootComment = true,
            WriteRootAsNamedProperty = false
        };

        // Act
        var result = JsonCSerializer<Person>.Serialize(person, options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("// A sample person object for testing"));
            Assert.That(result, Does.Contain("\"Name\": \"John Doe\""));
            Assert.That(result, Does.Contain("\"Age\": 30"));
            Assert.That(result, Does.StartWith("// A sample person")); // Comment at the start

            // Should NOT be wrapped in an outer object
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var firstJsonLine = lines.FirstOrDefault(l => !l.TrimStart().StartsWith("//"));
            Assert.That(firstJsonLine, Does.StartWith("{")); // Direct object
        }
    }

    [Test]
    public void Serialize_WithRootAsNamedProperty_OutputsWrappedObject()
    {
        // Arrange
        var person = new Person
        {
            Name = "Jane Smith",
            Age = 25,
            IsActive = false
        };

        var options = new JsonCSerializerOptions
        {
            WriteRootComment = true,
            WriteRootAsNamedProperty = true
        };

        // Act
        var result = JsonCSerializer<Person>.Serialize(person, options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("// A sample person object for testing"));
            Assert.That(result, Does.Contain("\"Person\":"));
            Assert.That(result, Does.Contain("\"Name\": \"Jane Smith\""));
            Assert.That(result, Does.Contain("\"Age\": 25"));

            // Should be wrapped in an outer object
            var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[0].Trim(), Is.EqualTo("{")); // Outer opening brace
            Assert.That(result, Does.Match(@"\{\s+//.*\s+""Person"":\s+\{")); // Outer object with Person property
        }
    }

    [Test]
    public void Serialize_WithRootAsNamedProperty_NoComment_OutputsWrappedObjectWithoutComment()
    {
        // Arrange
        var person = new Person
        {
            Name = "Bob Johnson",
            Age = 40,
            IsActive = true
        };

        var options = new JsonCSerializerOptions
        {
            WriteRootComment = false,
            WriteRootAsNamedProperty = true
        };

        // Act
        var result1 = JsonCSerializer<Person>.Serialize(person, options);
        options.RespectJsonPropertyOrder = false; // default is true
        var result2 = JsonCSerializer<Person>.Serialize(person, options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1, Is.EquivalentTo(result2)); // no ordering present
            Assert.That(result1, Does.Contain("\"Person\":"));
            Assert.That(result1, Does.Contain("\"Name\": \"Bob Johnson\""));
            Assert.That(result1, Does.Not.Contain("// A sample person object for testing"));

            var lines = result1.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.That(lines[0].Trim(), Is.EqualTo("{")); // Outer opening brace
            Assert.That(lines[1].Trim(), Does.StartWith("\"Person\":")); // Person property without comment
        }
    }

    [Test]
    public void Serialize_WithMultilineRawStringComments_OutputsMultilineComments()
    {
        // Arrange
        var obj = new MultilineCommentClass
        {
            TestProperty = "test value",
            Number = 42
        };

        var options = new JsonCSerializerOptions
        {
            Indent = "  ",
            WriteRootComment = true
        };

        // Act
        var result = JsonCSerializer<MultilineCommentClass>.Serialize(obj, options);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("// A test class with multiline comment"));
            Assert.That(result, Does.Contain("// using raw string literals"));
            Assert.That(result, Does.Contain("// This is a multiline property comment"));
            Assert.That(result, Does.Contain("// that spans multiple lines"));
            Assert.That(result, Does.Contain("// Single line comment"));
            Assert.That(result, Does.Contain("\"TestProperty\": \"test value\""));
            Assert.That(result, Does.Contain("\"Number\": 42"));
        }
    }

    [Test]
    public void DictionarySerialization()
    {
        var obj = new ClassContainingDictionary();
        var options = new JsonCSerializerOptions
        {
            Indent = "  ",
            WriteRootComment = true
        };

        // Act
        var result = JsonCSerializer<ClassContainingDictionary>.Serialize(obj, options);
        var deserialized = JsonCSerializer<ClassContainingDictionary>.Deserialize(result);
        var deserializedSectionExplicit = JsonCSerializer<ClassContainingDictionary>.Deserialize<ClassContainingDictionary>(result);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("// Class with a dictionary property"));
            Assert.That(result, Does.Contain("// This is a dictionary comment"));
            Assert.That(result, Does.Contain("\"TestDictionary\":"));
            Assert.That(result, Does.Contain("\"1\": \"One\""));
            Assert.That(result, Does.Contain("\"2\": \"Two\""));
            Assert.That(result, Does.Contain("\"3\": \"Three\""));
            Assert.That(deserialized.TestDictionary, Is.EquivalentTo(obj.TestDictionary));
            Assert.That(deserializedSectionExplicit.TestDictionary, Is.EquivalentTo(obj.TestDictionary));
        }
    }

    [Test]
    public void Deserialize_WithMultiplePropertiesAtRoot_UsesRootElement()
    {
        // Arrange
        // JSON with multiple properties at root
        // (doesn't match auto-unwrap criteria)
        var json = """
        {
            "Name": "Alice",
            "Age": 35,
            "IsActive": true
        }
        """;

        // Act
        var result = JsonCSerializer<Person>.Deserialize(json);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Alice"));
            Assert.That(result.Age, Is.EqualTo(35));
            Assert.That(result.IsActive, Is.True);
        }
    }

    [Test]
    public void Deserialize_WithSinglePropertyNotMatchingTypeName_UsesRootElement()
    {
        // Arrange
        // JSON with single property at root but name doesn't match type name
        var json = """
                   {
                       "Data": {
                           "Name": "Bob",
                           "Age": 42,
                           "IsActive": false
                       }
                   }
                   """;

        // Act & Assert - Should throw because the root object has unmapped "Data" property
        Assert.Throws<System.Text.Json.JsonException>(() =>
            JsonCSerializer<Person>.Deserialize(json));
    }

    [Test]
    public void Deserialize_WithNonObjectRoot_UsesRootElement()
    {
        // Arrange - JSON with array at root (not an object)
        var jsonArray = """
        [
            { "Name": "Charlie", "Age": 28, "IsActive": true }
        ]
        """;

        // Act & Assert - Should fail since root is array, not object
        Assert.Throws<System.Text.Json.JsonException>(() =>
            JsonCSerializer<Person>.Deserialize(jsonArray));
    }

    [Test]
    public void Deserialize_WithPrimitiveRoot_UsesRootElement()
    {
        // Arrange - JSON with primitive at root (string)
        var jsonString = "\"Simple string value\"";

        // Act
        var result = JsonCSerializer<string>.Deserialize(jsonString);

        // Assert
        Assert.That(result, Is.EqualTo("Simple string value"));
    }

    [Test]
    public void Deserialize_WithNumberRoot_UsesRootElement()
    {
        // Arrange - JSON with number at root
        var jsonNumber = "42";

        // Act
        var result = JsonCSerializer<int>.Deserialize(jsonNumber);

        // Assert
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void Serialize_WithJsonPropertyNameAttribute_UsesCustomPropertyName()
    {
        // Arrange - Tests GetRootPropertyName when JsonPropertyNameAttribute is not null
        var config = new ConfigWithCustomName
        {
            Setting = "production",
            NullValue = null
        };

        var options = new JsonCSerializerOptions
        {
            WriteRootComment = true,
            WriteRootAsNamedProperty = true
        };

        // Act
        var result = JsonCSerializer<ConfigWithCustomName>.Serialize(config, options);
        /*
          {
             // Class with custom class / property name
             "CustomConfig": {
               // Setting value
               "Config": "production",
               "NullValue": null
             }
           }
         */

        // Assert - Should use
        // 1. "CustomConfig" from class attribute instead of "ConfigWithCustomName"
        // 2. "Config" from attribute instead of "Setting"
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("// Class with custom class / property name"));
            Assert.That(result, Does.Contain("// Setting value"));
            Assert.That(result, Does.Contain("\"CustomConfig\": {"));
            Assert.That(result, Does.Contain("\"Config\": \"production\""));
            Assert.That(result, Does.Contain("\"NullValue\": null"));
        }
    }

    [Test]
    public void Serialize_WithListOfIntegers_WritesJsonArray()
    {
        // Arrange
        var testData = new TestClassWithList
        {
            Numbers = [1, 2, 3]
        };

        // Act
        var result = JsonCSerializer<TestClassWithList>.Serialize(testData);

        // Assert
        Assert.That(result, Does.Contain("\"Numbers\": ["));
        Assert.That(result, Does.Contain("1,"));
        Assert.That(result, Does.Contain("2,"));
        Assert.That(result, Does.Contain("3"));
        Assert.That(result, Does.Contain("]"));
    }
}
