namespace MsSql.ClassGenerator.Core.Model.Database;

/// <summary>
/// Represents a column.
/// </summary>
public class ColumnEntry
{
    /// <summary>
    /// Gets or sets the table id.
    /// </summary>
    public int TableId { get; set; }

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alias (overwrites the <see cref="Name"/>).
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    /// Gets the value which indicates whether the <see cref="Alias"/> is different from the <see cref="Name"/>.
    /// </summary>
    internal bool HasDifferentAlias => !string.IsNullOrWhiteSpace(Alias) && !Name.Equals(Alias);

    /// <summary>
    /// Gets the name which should be used for the property.
    /// </summary>
    /// <remarks>
    /// If the <see cref="Alias"/> is not empty, it will be used, otherwise the <see cref="Name"/>.
    /// </remarks>
    public string PropertyName => string.IsNullOrWhiteSpace(Alias) ? Name : Alias;

    /// <summary>
    /// Gets or sets the order of the column.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the sql data type of the column.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the max length.
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the column can be <i>null</i>.
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets the value which indicates whether the column is a part of the <b>PK</b>.
    /// </summary>
    public bool IsPrimaryKey { get; set; }
}
