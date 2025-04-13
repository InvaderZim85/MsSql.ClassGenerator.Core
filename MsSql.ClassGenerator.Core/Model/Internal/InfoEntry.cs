namespace MsSql.ClassGenerator.Core.Model.Internal;

/// <summary>
/// Represents an information entry.
/// </summary>
internal sealed class InfoEntry
{
    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public string Table { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public string? Column { get; set; }

    /// <summary>
    /// Gets or sets the <i>summary</i> value.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <i>remarks</i> value.
    /// </summary>
    public string Remarks { get; set; } = string.Empty;

    /// <summary>
    /// Gets the value which indicates whether the entry is <i>empty</i> (no summary and remarks value).
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Summary) && string.IsNullOrEmpty(Remarks);
}
