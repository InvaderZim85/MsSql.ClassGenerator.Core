namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Provides the additional information for a table / column.
/// </summary>
public class AdditionalInformationEntry
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
    internal bool IsEmpty => string.IsNullOrEmpty(Summary) && string.IsNullOrEmpty(Remarks);
}
