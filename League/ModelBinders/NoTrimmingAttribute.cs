using System;

namespace League.ModelBinders;

/// <summary>
/// When this attribute is set to a model field or property of type <see cref="string"/>, <see cref="StringTrimmingModelBinder"/> will not trim the property value.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NoTrimmingAttribute : Attribute
{ }
