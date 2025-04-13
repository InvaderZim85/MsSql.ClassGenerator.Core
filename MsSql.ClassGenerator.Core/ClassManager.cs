using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model;
using MsSql.ClassGenerator.Core.Model.Database;
using MsSql.ClassGenerator.Core.Model.Internal;
using System.Text;
using MsSql.ClassGenerator.Core.Common.Enums;

namespace MsSql.ClassGenerator.Core;

/// <summary>
/// Provides the functions for the class generation.
/// </summary>
public sealed partial class ClassManager
{
    /// <summary>
    /// Occurs when progress was made.
    /// </summary>
    public event EventHandler<string>? ProgressEvent;

    /// <summary>
    /// Contains the generator options of the user.
    /// </summary>
    private ClassGeneratorOptions? _options;

    /// <summary>
    /// Generates the classes for the specified tables with the specified options.
    /// </summary>
    /// <param name="tables">The list with the tables.</param>
    /// <param name="options">The desired options.</param>
    /// <returns>The result of the class generation.</returns>
    /// <exception cref="ArgumentNullException">Will be thrown when the options are null.</exception>
    /// <exception cref="DirectoryNotFoundException">Will be thrown when the specified directory doesn't exist.</exception>
    public async Task<GeneratorResult> GenerateClassesAsync(List<TableEntry> tables,
        ClassGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
        
        // Check the tables
        if (tables.Count == 0)
            return new GeneratorResult();

        // Load the type conversion information (needed for the class generator)
        await LoadTypeConversionDataAsync();

        // Load the additional information
        await LoadAdditionalInformationAsync(options.AdditionalInformationFilepath);
        if (_additionalInformation.Count > 0)
            options.AddSummary = true; // Overwrite the original value, when information were specified

        // Generate the classes for each table
        // Load the class template
        var classTemplate = await LoadClassTemplateAsync(!string.IsNullOrEmpty(_options!.Namespace));

        // Iterate through the tables.
        var result = new GeneratorResult();
        var count = 1;
        foreach (var table in tables)
        {
            ProgressEvent?.Invoke(this, $"{count++} of {tables.Count} > Generate class for table '{table.Name}'.");
            result.Classes.Add(await GenerateClassAsync(table, classTemplate));
        }

        // Generate the EF Key code (if desired)
        if (options.DbModel)
            result.EfKeyCode = GenerateEfKeyCode(tables);

        // Return the result.
        return result;
    }

    /// <summary>
    /// Generates the C# code for a single table.
    /// </summary>
    /// <param name="table">The table which should be created as C# class.</param>
    /// <param name="classTemplate">The class template.</param>
    /// <returns>The generated class.</returns>
    private async Task<ClassEntry> GenerateClassAsync(TableEntry table, List<string> classTemplate)
    {
        // The list with the replacement values
        var replaceList = new List<ReplacementDto>();

        // Create a clone of the template
        var template = await classTemplate.CloneAsync();

        // Load the property template
        var propertyTemplate = await LoadPropertyTemplateAsync();

        // Get the additional information of the table
        var columnInformation = _additionalInformation
            .Where(w => w.Table.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(w.Column)).ToList();
        var tableInformation = _additionalInformation
            .FirstOrDefault(f => f.Table.Equals(table.Name, StringComparison.OrdinalIgnoreCase) &&
                                 string.IsNullOrWhiteSpace(f.Column));

        // Generate the properties
        var properties = new List<string>();
        for (var i = 0; i < table.Columns.Count; i++)
        {
            var column = table.Columns[i];
            properties.Add(await GeneratePropertyCodeAsync(column, propertyTemplate, columnInformation, i + 1 != table.Columns.Count));
        }

        // The properties
        replaceList.Add(new ReplacementDto("properties", string.Join(Environment.NewLine, properties), true));

        // The namespace
        replaceList.Add(new ReplacementDto("namespace", GenerateNamespace()));

        // The modifier
        var modifier = _options!.Modifier switch
        {
            ClassModifier.Internal => "internal",
            ClassModifier.Protected => "protected",
            ClassModifier.ProtectedInternal => "protected internal",
            _ => "public"
        };
        replaceList.Add(new ReplacementDto("modifier", modifier));

        // The sealed value
        replaceList.Add(new ReplacementDto("sealed", _options.SealedClass ? " sealed" : string.Empty));

        // The class name
        var className = GenerateClassName(table.ClassName);
        replaceList.Add(new ReplacementDto("name", className));

        // Set the "inherits" value of the community toolkit
        var inheritValue = string.Empty;
        if (_options.AddSetProperty)
        {
            template.Insert(0, "using CommunityToolkit.Mvvm.ComponentModel;");
            inheritValue = ": ObservableObject";
        }
        replaceList.Add(new ReplacementDto("inherits", inheritValue));

        // The additions
        replaceList.Add(new ReplacementDto("addition", CreateClassAddition(table, tableInformation)));

        return new ClassEntry
        {
            Name = className,
            TableName = table.Name,
            Code = FinalizeTemplate(template, replaceList, false)
        };
    }

    /// <summary>
    /// Generates the code for the column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="propertyTemplate">The template of the property.</param>
    /// <param name="additionalInformation">The list with the additional information.</param>
    /// <param name="addEmptyLine"><see langword="true"/> to add an empty line at the end, otherwise <see langword="false"/>.</param>
    /// <returns>The code of the property.</returns>
    private async Task<string> GeneratePropertyCodeAsync(ColumnEntry column, List<string> propertyTemplate, List<InfoEntry> additionalInformation, bool addEmptyLine)
    {
        // The list with the replacement values
        var replaceList = new List<ReplacementDto>();

        // Create a clone of the template
        var template = await propertyTemplate.CloneAsync();

        // Get the C# type
        var dataType = GetCSharpType(column.DataType);

        // Prepare the template
        SetStringDefault(template, dataType);

        // Add the "replace" values
        // The data type
        replaceList.Add(new ReplacementDto("type", dataType));

        // The nullable value
        replaceList.Add(new ReplacementDto("nullable", column.IsNullable ? "?" : ""));

        // The property name
        var propertyName = GeneratePropertyName(column.PropertyName);
        replaceList.Add(new ReplacementDto("name", propertyName));

        // The backing field
        replaceList.Add(new ReplacementDto("name2", $"_{propertyName.FirstCharToLower()}"));

        // The addition (summary, remarks, attributes)
        replaceList.Add(new ReplacementDto("addition", CreatePropertyAddition(column, dataType, additionalInformation)));

        // Finalize the template
        return FinalizeTemplate(template, replaceList, addEmptyLine);
    }

    /// <summary>
    /// Sets the string default (<c>string.Empty</c>) if needed.
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="dataType">The data type.</param>
    private static void SetStringDefault(List<string> template, string dataType)
    {
        const string variablePlaceholder = "$NAME2$";
        const string propertyPlaceholder = "$NAME$";
        const string defaultValue = " = string.Empty;";

        if (!dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            return;

        if (template.Any(a => a.Contains(variablePlaceholder)))
        {
            var index = GetLineIndex(template, variablePlaceholder);
            if (index == -1)
                return;

            template[index] = template[index].Replace(";", defaultValue);
        }
        else if (template.Any(a => a.Contains(propertyPlaceholder)))
        {
            var index = GetLineIndex(template, propertyPlaceholder);
            if (index == -1) 
                return;

            template[index] += defaultValue;
        }
    }

    /// <summary>
    /// Creates the class additions (summary, remarks, attributes).
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="tableInfo">The table information.</param>
    /// <returns>The additions.</returns>
    private string CreateClassAddition(TableEntry table, InfoEntry? tableInfo)
    {
        var additions = new SortedList<int, string>();
        var index = 1;

        var hasRemarks = tableInfo != null && 
                         !string.IsNullOrWhiteSpace(tableInfo.Remarks) ||
                         _options!.AddTableNameToClassSummary;

        if (_options!.AddSummary || _options.AddTableNameToClassSummary)
        {
            additions.Add(index++, DocumentationTags.SummaryStart);
            additions.Add(index++, tableInfo != null && !string.IsNullOrWhiteSpace(tableInfo.Summary) ? $"/// {tableInfo.Summary}" : "/// TODO");
            additions.Add(index++, DocumentationTags.SummaryEnd);

            if (hasRemarks)
            {
                additions.Add(index++, DocumentationTags.RemarksStart);

                var hasEntry = false;
                if (tableInfo != null && !string.IsNullOrWhiteSpace(tableInfo.Remarks))
                {
                    additions.Add(index++, $"/// {tableInfo.Remarks}");
                    hasEntry = true;
                }

                if (_options.AddTableNameToClassSummary)
                {
                    if (hasEntry)
                        additions.Add(index++, "/// <para />"); // Add a paragraph

                    additions.Add(index++, $"/// Table <c>[{table.Schema}].[{table.Name}]</c>");
                }

                additions.Add(index++, DocumentationTags.RemarksEnd);
            }
        }

        if (_options.DbModel)
        {
            additions.Add(index, string.IsNullOrWhiteSpace(table.Schema)
                ? $"[Table(\"{table.Name}\"]"
                : $"[Table(\"{table.Name}\", Schema = \"{table.Schema}\")]");
        }

        return string.Join(Environment.NewLine, additions.OrderBy(o => o.Key).Select(s => s.Value));
    }

    /// <summary>
    /// Creates the property additions (summary, remarks, attributes).
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="dataType">The C# data type.</param>
    /// <param name="additionalInformation">The additional information of the table to which the column belongs.</param>
    /// <returns>The additions.</returns>
    private string CreatePropertyAddition(ColumnEntry column, string dataType, List<InfoEntry> additionalInformation)
    {
        var additions = new SortedList<int, string>();
        var index = 1;

        var info = additionalInformation
            .FirstOrDefault(f => !string.IsNullOrEmpty(f.Column) &&
                                 f.Column.Equals(column.Name, StringComparison.OrdinalIgnoreCase));

        if (_options!.AddSummary)
        {
            additions.Add(index++, DocumentationTags.SummaryStart);
            additions.Add(index++, info != null && !string.IsNullOrWhiteSpace(info.Summary) ? $"/// {info.Summary}" : "/// TODO");
            additions.Add(index++, DocumentationTags.SummaryEnd);

            if (info != null && !string.IsNullOrWhiteSpace(info.Remarks))
            {
                additions.Add(index++, DocumentationTags.RemarksStart);
                additions.Add(index++, $"/// {info.Remarks}");
                additions.Add(index++, DocumentationTags.RemarksEnd);
            }
        }

        if (_options.DbModel && column.IsPrimaryKey)
            additions.Add(index++, "[Key]");

        if (_options.AddColumnAttribute || column.HasDifferentAlias)
            additions.Add(index++, $"[Column(\"{column.Name}\"]");

        if (column.DataType.Equals("date", StringComparison.OrdinalIgnoreCase))
            additions.Add(index++, "[DataType(DataType.Date)]");

        if (dataType.Equals("string"))
        {
            additions.Add(index, column.MaxLength == -1 // -1 indicates NVARCHAR(MAX)
            ? "[MaxLength(int.MaxValue)]"
            : $"[MaxLength({column.MaxLength})]");
        }

        return string.Join(Environment.NewLine, additions.OrderBy(o => o.Key).Select(s => s.Value));
    }

    /// <summary>
    /// Finalizes the template.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <param name="replaceList">The list with the replacement data.</param>
    /// <param name="addEmptyLine"><see langword="true"/> to add an empty line at the end, otherwise <see langword="false"/>.</param>
    /// <returns>The content of the template.</returns>
    private static string FinalizeTemplate(List<string> content, List<ReplacementDto> replaceList, bool addEmptyLine)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < content.Count; i++)
        {
            var line = content[i];

            // Check if the line contains a value which should be replaced.
            if (!line.Contains('$'))
            {
                if (i + 1 == content.Count)
                    sb.Append(line);
                else
                    sb.AppendLine(line);
                continue;
            }

            var tmpLine = line;

            // Iterate through the replacement values
            foreach (var replacement in replaceList)
            {
                var key = $"${replacement.Key.ToUpper()}$";

                if (!replacement.Indent)
                {
                    tmpLine = tmpLine.Replace(key, replacement.Value);
                    continue;
                }

                // Split the value, because it's possible that the value has multiple entries
                var tmpValue = replacement.Value
                    .Split([Environment.NewLine], StringSplitOptions.None)
                    .Select(s => $"{Tab}{s}");
                tmpLine = tmpLine.Replace(key, string.Join(Environment.NewLine, tmpValue));
            }

            if (string.IsNullOrWhiteSpace(tmpLine)) 
                continue;

            if (i + 1 == content.Count)
                sb.Append(tmpLine);
            else
                sb.AppendLine(tmpLine);
        }

        if (addEmptyLine)
            sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Generates the EF Key code.
    /// </summary>
    /// <param name="tables">The list with the tables.</param>
    /// <returns>The EF key code.</returns>
    private static string GenerateEfKeyCode(List<TableEntry> tables)
    {
        // Get all tables which contains more than one key column
        var tmpTables = tables.Where(w => w.Columns.Count(c => c.IsPrimaryKey) > 1).ToList();
        if (tmpTables.Count == 0)
            return string.Empty;

        var sb = PrepareStringBuilder();
        var count = 1;
        foreach (var tableEntry in tmpTables)
        {
            // Add the entity
            sb.AppendLine($"{Tab}modelBuilder.Entity<{tableEntry.ClassName}>().HasKey(k => new")
                .AppendLine($"{Tab}{{");

            // Get the key columns
            var columnCount = 1;
            var columns = tableEntry.Columns.Where(w => w.IsPrimaryKey).ToList();
            foreach (var columnEntry in columns)
            {
                var comma = columnCount++ != columns.Count ? "," : string.Empty;

                sb.AppendLine($"{Tab}{Tab}k.{columnEntry.PropertyName}{comma}");
            }

            // Add the closing brackets
            sb.AppendLine($"{Tab}}});");

            if (count++ != tmpTables.Count)
                sb.AppendLine(); // Spacer
        }

        return FinalizeStringBuilder();

        StringBuilder PrepareStringBuilder()
        {
            var stringBuilder = new StringBuilder()
                .AppendLine("/// <inheritdoc />")
                .AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                .AppendLine("{");

            return stringBuilder;
        }

        // Adds the final code
        string FinalizeStringBuilder()
        {
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
