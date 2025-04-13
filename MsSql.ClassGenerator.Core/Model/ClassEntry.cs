namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Represents a single table.
/// </summary>
public class ClassEntry
{
    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the name of the table which is the source.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// Gets the generated code of the table.
    /// </summary>
    public required string Code { get; init; }
}
