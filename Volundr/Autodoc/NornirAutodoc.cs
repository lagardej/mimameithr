using Brunnr.Autodoc;
using System.Reflection;
using System.Text;

namespace Autodoc;

internal static class NornirAutodoc
{
    public static int Run(string projectRoot, string outputFileName)
    {
        var assemblyPath = Path.Combine(projectRoot, "Nornir", "bin", "Debug", "net10.0", "Nornir.dll");
        var sourceRoot = Path.Combine(projectRoot, "Nornir");
        var autodocRoot = Path.Combine(projectRoot, "Volundr", "Autodoc");
        var templatePath = Path.Combine(autodocRoot, "component-template.adoc");
        var groupTemplatePath = Path.Combine(autodocRoot, "group-template.adoc");
        var indexTemplatePath = Path.Combine(autodocRoot, "components-index-template.adoc");

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine($"Assembly not found: {assemblyPath}");
            return 1;
        }

        if (!File.Exists(templatePath))
        {
            Console.Error.WriteLine($"Template not found: {templatePath}");
            return 1;
        }

        if (!File.Exists(groupTemplatePath))
        {
            Console.Error.WriteLine($"Group template not found: {groupTemplatePath}");
            return 1;
        }

        if (!File.Exists(indexTemplatePath))
        {
            Console.Error.WriteLine($"Index template not found: {indexTemplatePath}");
            return 1;
        }

        var template = File.ReadAllText(templatePath);
        var groupTemplate = File.ReadAllText(groupTemplatePath);
        var indexTemplate = File.ReadAllText(indexTemplatePath);
        var assembly = Assembly.LoadFrom(assemblyPath);

        var componentTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<ComponentAttribute>() is not null)
            .ToList();

        if (componentTypes.Count == 0)
        {
            Console.WriteLine("No [Component] types found.");
            return 0;
        }

        // Generate per-component READMEs
        const string nsPrefix = "Nornir.";
        foreach (var componentType in componentTypes)
        {
            var attr = componentType.GetCustomAttribute<ComponentAttribute>()!;
            var ns = componentType.Namespace ?? string.Empty;

            var relativePath = ns.StartsWith(nsPrefix)
                ? ns[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
                : string.Empty;

            var title = attr.Title.Length == 0 && relativePath.Length > 0
                ? Path.GetFileName(relativePath)
                : componentType.Name;

            var summary = AutodocHelpers.GetXmlSummary(componentType) ?? "_No summary provided._";
            var outputDir = Path.Combine(sourceRoot, relativePath);
            var outputPath = Path.Combine(outputDir, outputFileName);

            Directory.CreateDirectory(outputDir);

            var doc = template
                .Replace("{{title}}", title)
                .Replace("{{breadcrumb}}",
                    $"xref:../../{outputFileName}[Nornir] > xref:../{outputFileName}[{Path.GetDirectoryName(relativePath)}] > {title}")
                .Replace("{{summary}}", summary)
                .Replace("{{settings}}", BuildParameterRows(componentType))
                .Replace("{{states}}", BuildStateRows(componentType))
                .Replace("{{forcings}}", BuildForcingRows(assembly, ns));

            File.WriteAllText(outputPath, doc);
            Console.WriteLine($"Written: {outputPath}");
        }

        // Build group lookup
        var groupSummaries = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<GroupAttribute>() is not null)
            .ToDictionary(
                t => t.Name,
                t => t.GetCustomAttribute<GroupAttribute>()!.Summary);

        // Compute grouping
        var grouped = componentTypes
            .Select(t =>
            {
                var attr = t.GetCustomAttribute<ComponentAttribute>()!;
                var group = attr.Group;
                var slash = group.IndexOf('/');
                var topLevel = slash >= 0 ? group[..slash] : group;
                var subLevel = slash >= 0 ? group[(slash + 1)..] : string.Empty;
                var ns = t.Namespace ?? string.Empty;
                var relativePath = ns.StartsWith(nsPrefix)
                    ? ns[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
                    : string.Empty;
                var title = attr.Title.Length == 0 && relativePath.Length > 0
                    ? Path.GetFileName(relativePath)
                    : t.Name;

                return (topLevel, subLevel, title, attr.Summary, relativePath, Type: t);
            })
            .GroupBy(x => x.topLevel)
            .OrderBy(g => g.Key)
            .ToList();

        // Generate per-group READMEs
        foreach (var g in grouped)
        {
            var groupType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == g.Key + "Group" && t.GetCustomAttribute<GroupAttribute>() is not null);
            if (groupType is null)
            {
                continue;
            }

            var groupAttr = groupType.GetCustomAttribute<GroupAttribute>()!;
            var groupNs = groupType.Namespace ?? string.Empty;
            var groupRelativePath = groupNs.StartsWith(nsPrefix)
                ? groupNs[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
                : string.Empty;
            var groupOutputDir = Path.Combine(sourceRoot, groupRelativePath);
            var groupOutputPath = Path.Combine(groupOutputDir, outputFileName);

            var componentRows = new StringBuilder();
            foreach (var (_, subLevel, title, _, relativePath, componentType) in g.OrderBy(x => x.subLevel))
            {
                var label = string.IsNullOrEmpty(subLevel) ? title : subLevel;
                var compSummary = AutodocHelpers.GetXmlSummary(componentType) ?? "-";
                var relLink = Path.GetRelativePath(groupRelativePath, relativePath)
                    .Replace(Path.DirectorySeparatorChar, '/');
                componentRows.AppendLine($"| link:{relLink}/{outputFileName}[{label}] | {compSummary}");
            }

            var groupDoc = groupTemplate
                .Replace("{{title}}", g.Key)
                .Replace("{{breadcrumb}}", $"xref:../{outputFileName}[Nornir] > " + g.Key)
                .Replace("{{summary}}", groupAttr.Summary.Length != 0
                    ? groupAttr.Summary
                    : AutodocHelpers.GetXmlSummary(groupType) ?? "_No summary provided._")
                .Replace("{{components}}", componentRows.ToString().TrimEnd());

            Directory.CreateDirectory(groupOutputDir);
            File.WriteAllText(groupOutputPath, groupDoc);
            Console.WriteLine($"Written: {groupOutputPath}");
        }

        // Generate index
        var groupTable = new StringBuilder();
        var indexSections = new StringBuilder();

        foreach (var g in grouped)
        {
            var groupSummary = groupSummaries.GetValueOrDefault(g.Key + "Group");
            var anchor = $"_{g.Key.ToLowerInvariant()}";

            var groupType = assembly.GetTypes().FirstOrDefault(t => t.Name == g.Key + "Group");
            var groupNs = groupType?.Namespace ?? string.Empty;
            var groupRelativePath = groupNs.StartsWith(nsPrefix)
                ? groupNs[nsPrefix.Length..].Replace('.', '/')
                : string.Empty;
            var groupLink = groupRelativePath.Length > 0
                ? $"link:{groupRelativePath}/{outputFileName}[{g.Key}]"
                : g.Key;

            groupTable.AppendLine($"| {groupLink} | {groupSummary ?? "-"}");

            indexSections.AppendLine($"[[{anchor}]]");
            indexSections.AppendLine($"== {g.Key}");
            indexSections.AppendLine();
            if (groupSummary is not null)
            {
                indexSections.AppendLine(groupSummary);
                indexSections.AppendLine();
            }

            indexSections.AppendLine("[cols=\"1,3\",options=\"header\"]");
            indexSections.AppendLine("|===");
            indexSections.AppendLine("| Component | Summary");
            foreach (var (_, subLevel, title, summary, relativePath, _) in g.OrderBy(x => x.subLevel))
            {
                var label = string.IsNullOrEmpty(subLevel) ? title : subLevel;
                var readmePath = relativePath.Length > 0
                    ? $"link:{relativePath.Replace(Path.DirectorySeparatorChar, '/')}/{outputFileName}[{label}]"
                    : label;
                indexSections.AppendLine($"| {readmePath} | {summary}");
            }

            indexSections.AppendLine("|===");
            indexSections.AppendLine();
        }

        var indexDoc = indexTemplate
            .Replace("{{group_table}}", groupTable.ToString().TrimEnd())
            .Replace("{{components}}", indexSections.ToString().TrimEnd());
        var indexPath = Path.Combine(sourceRoot, outputFileName);
        File.WriteAllText(indexPath, indexDoc);
        Console.WriteLine($"Written: {indexPath}");

        return 0;
    }

    private static string BuildParameterRows(Type componentType)
    {
        var sb = new StringBuilder();
        foreach (var member in AutodocHelpers.GetAnnotatedMembers(componentType, typeof(SettingAttribute)))
        {
            var a = member.GetCustomAttribute<SettingAttribute>()!;
            sb.AppendLine($"| {member.Name} | {a.Unit} | {a.Purpose}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
    }

    private static string BuildStateRows(Type componentType)
    {
        var sb = new StringBuilder();
        foreach (var member in AutodocHelpers.GetAnnotatedMembers(componentType, typeof(StateAttribute)))
        {
            var a = member.GetCustomAttribute<StateAttribute>()!;
            sb.AppendLine($"| {member.Name} | {a.Unit} | {a.Purpose}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
    }

    private static string BuildForcingRows(Assembly assembly, string ns)
    {
        var sb = new StringBuilder();
        var forcings = assembly.GetTypes()
            .Where(t => t.Namespace == ns && t.GetCustomAttribute<ForcingAttribute>() is not null);
        foreach (var f in forcings)
        {
            var summary = AutodocHelpers.GetXmlSummary(f) ?? "-";
            sb.AppendLine($"| {f.Name} | {summary}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
    }
}
