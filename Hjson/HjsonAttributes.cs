using System;

namespace Hjson;

/// <summary>Specifies the property name used in Hjson serialization.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HjsonPropertyNameAttribute(string name) : Attribute
{
    /// <summary>Gets the name of the property.</summary>
    public string Name { get; } = name;
}

/// <summary>Indicates that a property should be ignored during Hjson serialization.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HjsonIgnoreAttribute : Attribute;

/// <summary>Indicates that a non-public property or field should be included during Hjson serialization.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HjsonIncludeAttribute : Attribute;

/// <summary>Specifies a comment to be written above the property in Hjson output.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HjsonCommentAttribute(string comment) : Attribute
{
    /// <summary>Gets the comment text.</summary>
    public string Comment { get; } = comment;
}
