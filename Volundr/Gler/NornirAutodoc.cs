using Kjarni.Brunnr.Autodoc;
using System.Reflection;
using System.Text;

namespace Volundr.Gler;

internal static class NornirAutodoc
{
    public static int Run(string projectRoot, string outputFileName)
    {
        var assemblyPath = Path.Combine(projectRoot, "Nornir", "bin", "Debug", "net10.0", "Nornir.dll");
        var fragmentsRoot = Path.Combine(projectRoot, "_autodoc", "nornir");

        if (!File.Exists(assemblyPath))
        {
            Console.Error.WriteLine($"Assembly not found: {assemblyPath}");
            return 1;
        }

        var assembly = Assembly.LoadFrom(assemblyPath);

        var componentTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<ComponentAttribute>() is not null)
            .ToList();

        if (componentTypes.Count == 0)
        {
            Console.WriteLine("No [Component] types found.");
            return 0;
        }

        var byGroup = componentTypes
            .GroupBy(t => t.GetCustomAttribute<ComponentAttribute>()!.Group)
            .OrderBy(g => g.Key);

        Directory.CreateDirectory(fragmentsRoot);

        // Top-level group summaries from *Group classes
        var topLevelGroups = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<GroupAttribute>() is not null)
            .ToList();

        foreach (var groupType in topLevelGroups)
        {
            var attr = groupType.GetCustomAttribute<GroupAttribute>()!;
            var name = groupType.Name.Replace("Group", "");
            WriteFile(Path.Combine(fragmentsRoot, $"{name}.summary.adoc"), attr.Summary);
        }

        foreach (var group in byGroup)
        {
            var slug = group.Key.Replace('/', '-');

            // Summary fragment — XML doc of first component in group
            var firstType = group.OrderBy(t => t.Name).First();
            var summary = AutodocHelpers.GetXmlSummary(firstType) ?? "_No summary provided._";
            WriteFile(Path.Combine(fragmentsRoot, $"{slug}.summary.adoc"), summary);

            WriteFragment(
                Path.Combine(fragmentsRoot, $"{slug}.settings.adoc"),
                group,
                typeof(SettingAttribute),
                a => ((SettingAttribute) a).Unit,
                a => ((SettingAttribute) a).Purpose);

            WriteFragment(
                Path.Combine(fragmentsRoot, $"{slug}.states.adoc"),
                group,
                typeof(StateAttribute),
                a => ((StateAttribute) a).Unit,
                a => ((StateAttribute) a).Purpose);

            WriteForcingsFragment(
                Path.Combine(fragmentsRoot, $"{slug}.forcings.adoc"),
                group,
                assembly);
        }

        return 0;
    }

    private static void WriteFile(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content);
        Console.WriteLine($"Written: {outputPath}");
    }

    private static void WriteFragment(
        string outputPath,
        IGrouping<string, Type> group,
        Type attributeType,
        Func<Attribute, string> getUnit,
        Func<Attribute, string> getPurpose)
    {
        var sb = new StringBuilder();

        foreach (var componentType in group.OrderBy(t => t.Name))
        {
            foreach (var member in AutodocHelpers.GetAnnotatedMembers(componentType, attributeType))
            {
                var a = (Attribute) member.GetCustomAttribute(attributeType)!;
                sb.AppendLine($"| {member.Name} | {getUnit(a)} | {getPurpose(a)}");
            }
        }

        var content = sb.Length > 0 ? sb.ToString().TrimEnd() : "3+| _None declared._";
        WriteFile(outputPath, content);
    }

    private static void WriteForcingsFragment(
        string outputPath,
        IGrouping<string, Type> group,
        Assembly assembly)
    {
        var namespaces = group.Select(t => t.Namespace).ToHashSet();

        var rows = assembly.GetTypes()
            .Where(t => namespaces.Contains(t.Namespace) && t.GetCustomAttribute<ForcingAttribute>() is not null)
            .OrderBy(t => t.Name)
            .Select(t => $"| {t.Name} | {AutodocHelpers.GetXmlSummary(t) ?? "-"}")
            .ToList();

        var content = rows.Count > 0
            ? string.Join(Environment.NewLine, rows)
            : "2+| _None declared._";

        WriteFile(outputPath, content);
    }
}
