namespace MsSql.ClassGenerator.Core.Model.Database;

/// <summary>
/// Represents a database table.
/// </summary>
public sealed class TableEntry
{
    /// <summary>
    /// Gets or sets the id.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias (overwrites the <see cref="Name"/>).
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets the name which should be used for the class.
    /// </summary>
    /// <remarks>
    /// If the <see cref="Alias"/> is not empty, it will be used, otherwise the <see cref="Name"/>.
    /// </remarks>
    public string ClassName => string.IsNullOrWhiteSpace(Alias) ? Name : Alias;

    /// <summary>
    /// Gets or sets the name of the according schema like <i>dbo</i>.
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the columns of the table.
    /// </summary>
    public List<ColumnEntry> Columns { get; set; } = [];
}