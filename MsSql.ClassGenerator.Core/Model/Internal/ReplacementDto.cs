namespace MsSql.ClassGenerator.Core.Model.Internal;

/// <summary>
/// Represents a replacement value.
/// </summary>
/// <param name="Key">The key.</param>
/// <param name="Value">The replacement.</param>
/// <param name="Indent"><see langword="true"/> if the value should be indented, otherwise <see langword="false"/>.</param>
public sealed record ReplacementDto(string Key, string Value, bool Indent = false);