namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Contains the result of the class generation.
/// </summary>
public class GeneratorResult
{
    /// <summary>
    /// Gets or sets the list with the classes.
    /// </summary>
    public List<ClassEntry> Classes { get; set; } = [];

    /// <summary>
    /// Gets or sets the EF Core key code which was generated.
    /// </summary>
    public string EfKeyCode { get; set; } = string.Empty;
}
