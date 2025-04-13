using MsSql.ClassGenerator.Core.Common.Enums;

namespace MsSql.ClassGenerator.Core.Model;

/// <summary>
/// Provides the class generator options which are needed to generate a class.
/// </summary>
public sealed class ClassGeneratorOptions
{
    /// <summary>
    /// Gets the desired namespace of the classes.
    /// </summary>
    public required string Namespace { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a sealed class should be created.
    /// </summary>
    public bool SealedClass { get; init; } = true;

    /// <summary>
    /// Gets the desired modifier.
    /// </summary>
    public ClassModifier Modifier { get; init; } = ClassModifier.Public;

    /// <summary>
    /// Gets the value which indicates whether a class should be created which can be used with <i>Entity Framework Core</i>.
    /// </summary>
    public bool DbModel { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a column attribute should be added to each property.
    /// </summary>
    public bool AddColumnAttribute { get; init; }

    /// <summary>
    /// Gets the value which indicates whether a backing field should be created for each property.
    /// </summary>
    public bool WithBackingField { get; init; }

    /// <summary>
    /// Gets the value which indicates whether the <i>SetProperty</i> function of the <a href="https://learn.microsoft.com/de-de/dotnet/communitytoolkit/mvvm/">CommunityToolKit.MVVM</a> should be used.
    /// </summary>
    /// <remarks>
    /// <b>Note</b>: If the value is set to <see langword="true"/>, a backing field is automatically created, even if the value of <see cref="WithBackingField"/> is set to <see langword="false"/>.
    /// </remarks>
    public bool AddSetProperty { get; init; }

    /// <summary>
    /// Gets or sets the value which indicates whether an empty <i>summary</i> should be added to the class and each property.
    /// </summary>
    public bool AddSummary { get; set; }

    /// <summary>
    /// Gets the value which indicates whether the table name should be added in the class summary.
    /// </summary>
    public bool AddTableNameToClassSummary { get; init; }

    /// <summary>
    /// Gets the path of the file which contains additional information.
    /// </summary>
    public string AdditionalInformationFilepath { get; init; } = string.Empty;
}
