using MsSql.ClassGenerator.Core.Common;
using MsSql.ClassGenerator.Core.Model.Internal;

namespace MsSql.ClassGenerator.Core;

partial class ClassManager
{
    #region Constant values
    
    /// <summary>
    /// Contains the tab indent.
    /// </summary>
    private static readonly string Tab = new(' ', 4);

    /// <summary>
    /// Gets the path of the <i>Files</i> directory.
    /// </summary>
    private static string FileDir => Path.Combine(AppContext.BaseDirectory, "Files");

    /// <summary>
    /// Provides the different documentation tags.
    /// </summary>
    private struct DocumentationTags
    {
        /// <summary>
        /// Start tag of a summary.
        /// </summary>
        internal const string SummaryStart = "/// <summary>";

        /// <summary>
        /// End tag of a summary.
        /// </summary>
        internal const string SummaryEnd = "/// </summary>";

        /// <summary>
        /// Start tag of a remarks.
        /// </summary>
        internal const string RemarksStart = "/// <remarks>";

        /// <summary>
        /// End tag of a remarks.
        /// </summary>
        internal const string RemarksEnd = "/// </remarks>";
    }

    /// <summary>
    /// Provides the different file names.
    /// </summary>
    private struct FileNames
    {
        /// <summary>
        /// The name of the class template file with namespace.
        /// </summary>
        internal const string ClassTemplateWithNamespace = "ClassDefaultWithNs.cgt";

        /// <summary>
        /// The name of the class template file without namespace.
        /// </summary>
        internal const string ClassTemplateWithoutNamespace = "ClassDefault.cgt";

        /// <summary>
        /// The name of the property with a backing field.
        /// </summary>
        internal const string PropertyBackingField = "PropertyBackingField.cgt";

        /// <summary>
        /// The name of the property template with a backing field and usage of the <c>SetProperty</c> method.
        /// </summary>
        internal const string PropertyBackingFieldSetProperty = "PropertyBackingFieldSetProperty.cgt";

        /// <summary>
        /// The name of the default property template.
        /// </summary>
        internal const string PropertyDefault = "PropertyDefault.cgt";
    }

    #endregion
    
    /// <summary>
    /// The list with the type conversion data.
    /// </summary>
    /// <remarks>
    /// <i>Info</i>: To load the data, call the function <see cref="LoadTypeConversionDataAsync"/>.
    /// </remarks>
    private List<TypeConversionDto> _typeConversionList = [];

    /// <summary>
    /// The list with the additional information.
    /// </summary>
    /// <remarks>
    /// <i>Info</i>: To load the data, call the function <see cref="LoadAdditionalInformationAsync"/>.
    /// </remarks>
    private List<InfoEntry> _additionalInformation = [];

    /// <summary>
    /// Loads the type conversion data and stores the data into <see cref="_typeConversionList"/>.
    /// </summary>
    /// <returns>The awaitable task.</returns>
    private async Task LoadTypeConversionDataAsync()
    {
        if (_typeConversionList.Count > 0)
            return;

        var path = Path.Combine(FileDir, "TypeConversion.json");
        if (!File.Exists(path))
        {
            return;
        }

        _typeConversionList = await Helper.LoadJsonAsync<List<TypeConversionDto>>(path);
    }

    /// <summary>
    /// Loads the additional information and stores them into <see cref="_additionalInformation"/>.
    /// </summary>
    /// <param name="filepath">The path of the file.</param>
    /// <returns>The additional information.</returns>
    private async Task LoadAdditionalInformationAsync(string filepath)
    {
        _additionalInformation = [];

        try
        {
            if (!File.Exists(filepath))
                return;

            _additionalInformation = await Helper.LoadJsonAsync<List<InfoEntry>>(filepath);
        }
        catch
        {
            // Ignore
        }
    }

    /// <summary>
    /// Loads the content of the template file.
    /// </summary>
    /// <param name="withNamespace"><see langword="true"/> for the template with namespace, otherwise <see langword="false"/>.</param>
    /// <returns>The content of the file.</returns>
    private static async Task<List<string>> LoadClassTemplateAsync(bool withNamespace)
    {
        var path = Path.Combine(FileDir,
            withNamespace ? FileNames.ClassTemplateWithNamespace : FileNames.ClassTemplateWithoutNamespace);

        return await LoadFileContentAsync(path);
    }

    /// <summary>
    /// Loads the content of the property template file according to the specified options.
    /// </summary>
    /// <returns>The content of the file.</returns>
    private async Task<List<string>> LoadPropertyTemplateAsync()
    {
        var withBackingField = _options!.AddSetProperty || _options.WithBackingField;

        var file = withBackingField switch
        {
            true when _options is { AddSetProperty: false } => FileNames.PropertyBackingField,
            true when _options is { AddSetProperty: true } => FileNames.PropertyBackingFieldSetProperty,
            _ => FileNames.PropertyDefault
        };

        return await LoadFileContentAsync(Path.Combine(FileDir, file));
    }

    /// <summary>
    /// Loads the content of the specified file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The content of the file.</returns>
    private static async Task<List<string>> LoadFileContentAsync(string path)
    {
        var stream = File.OpenRead(path);

        var result = new List<string>();

        var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync() is { } line)
        {
            result.Add(line);
        }

        return result;
    }

    /// <summary>
    /// Gets the C# type according to the specified sql type.
    /// </summary>
    /// <param name="sqlType">The sql type.</param>
    /// <returns>The C# type</returns>
    private string GetCSharpType(string sqlType)
    {
        return _typeConversionList
                   .FirstOrDefault(f => f.SqlType.Equals(sqlType, StringComparison.InvariantCultureIgnoreCase))
                   ?.CsharpType ??
               "object"; // Fallback
    }

    /// <summary>
    /// Gets the index of the desired value.
    /// </summary>
    /// <param name="lines">The list with the lines.</param>
    /// <param name="value">The desired value.</param>
    /// <returns>The index of the line. <c>-1</c> if nothing was found.</returns>
    private static int GetLineIndex(List<string> lines, string value)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(value))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Generates a <i>clean</i> property name and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <param name="name">The desired name.</param>
    /// <returns>The generated property name.</returns>
    private static string GeneratePropertyName(string name)
    {
        // Remove all "invalid" chars
        name = GetInvalidCharReplaceList()
            .Aggregate(name, (current, entry) => current.Replace(entry.Key, entry.Value));

        // Change the first char to upper
        name = name.FirstChatToUpper();

        return name.StartsWithNumber() ? $"Column{name}" : name;
    }

    /// <summary>
    /// Gets the list with the <i>invalid</i> chars which should be always replaced.
    /// </summary>
    /// <param name="includeUnderscore"><see langword="true"/> to include an underscore in the list, otherwise <see langword="false"/>.</param>
    /// <returns>The list with the values.</returns>
    private static List<ReplacementDto> GetInvalidCharReplaceList(bool includeUnderscore = true)
    {
        var tmpList = new List<ReplacementDto>
        {
            new(" ", ""), // this should never happen...
            new("ä", "ae"),
            new("ö", "oe"),
            new("ü", "ue"),
            new("ß", "ss"),
            new("Ä", "Ae"),
            new("Ö", "Oe"),
            new("Ü", "Ue")
        };

        if (includeUnderscore)
            tmpList.Add(new ReplacementDto("_", ""));

        return tmpList;
    }

    /// <summary>
    /// Generates a <i>clean</i> namespace and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <returns>The cleaned namespace name</returns>
    private string GenerateNamespace()
    {
        var name = _options!.Namespace;

        const char dot = '.';
        if (!name.Contains(dot))
            return name.FirstChatToUpper().Replace(" ", "");

        var content = name.Split(dot, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        name = string.Join(dot, content.Select(s => s.FirstChatToUpper()));

        var result = name.FirstChatToUpper().Replace(" ", "");

        // Remove all "invalid" chars
        result = GetInvalidCharReplaceList(false)
            .Aggregate(result, (current, entry) => current.Replace(entry.Key, entry.Value));

        return result.StartsWithNumber() ? $"Ns{result}" : result;
    }

    /// <summary>
    /// Generates a <i>clean</i> class name and removes all <i>invalid</i> chars like ä, ö, ü and so on.
    /// </summary>
    /// <param name="name">The desired name.</param>
    /// <returns>The generated class name.</returns>
    private static string GenerateClassName(string name)
    {
        IReadOnlyCollection<ReplacementDto> replaceList;

        if (name.Contains('_'))
        {
            replaceList = GetInvalidCharReplaceList(false);

            // Split the entry at the underscore
            var content = name.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Create a new "class" name
            name = content.Aggregate(string.Empty, (current, entry) => current + entry.FirstChatToUpper());
        }
        else
        {
            replaceList = GetInvalidCharReplaceList();
            name = name.FirstChatToUpper();
        }

        // Remove all "invalid" chars
        var result = replaceList.Aggregate(name, (current, entry) => current.Replace(entry.Key, entry.Value));

        return result.StartsWithNumber() ? $"Class{result}" : result;
    }
}
