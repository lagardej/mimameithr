using Kvasir;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Gler;

internal static class KvasirAutodoc
{
    public static int Run(string projectRoot, string readmeFileName)
    {
        var kvasirAssemblyPath = Path.Combine(projectRoot, "Kvasir", "bin", "Debug", "net10.0", "Kvasir.dll");
        var kvasirSourceRoot = Path.Combine(projectRoot, "Kvasir");
        var autodocRoot = Path.Combine(projectRoot, "Volundr", "Gler");
        var moduleTemplatePath = Path.Combine(autodocRoot, "module-template.adoc");
        var kvasirIndexTemplatePath = Path.Combine(autodocRoot, "kvasir-index-template.adoc");

        if (!File.Exists(kvasirAssemblyPath))
        {
            Console.Error.WriteLine($"Kvasir assembly not found: {kvasirAssemblyPath}");
            return 1;
        }

        if (!File.Exists(moduleTemplatePath))
        {
            Console.Error.WriteLine($"Module template not found: {moduleTemplatePath}");
            return 1;
        }

        if (!File.Exists(kvasirIndexTemplatePath))
        {
            Console.Error.WriteLine($"Index template not found: {kvasirIndexTemplatePath}");
            return 1;
        }

        var moduleTemplate = File.ReadAllText(moduleTemplatePath);
        var kvasirIndexTemplate = File.ReadAllText(kvasirIndexTemplatePath);
        var kvasirAssembly = Assembly.LoadFrom(kvasirAssemblyPath);

        var moduleTypes = kvasirAssembly.GetTypes()
            .Where(t => t.GetCustomAttribute<ModuleAttribute>() is not null)
            .ToList();

        foreach (var moduleType in moduleTypes)
        {
            var attr = moduleType.GetCustomAttribute<ModuleAttribute>()!;
            var domain = attr.Domain;
            var relativePath = domain.Replace('/', Path.DirectorySeparatorChar);
            var title = moduleType.Name;
            var xmlSummary = AutodocHelpers.GetXmlSummary(moduleType);
            var summary = attr.Summary;
            var model = xmlSummary ?? "_No model description provided._";

            var segments = domain.Split('/');
            var breadcrumb = BuildModuleBreadcrumb(segments, title, readmeFileName);

            var outputDir = Path.Combine(kvasirSourceRoot, relativePath);
            var outputPath = Path.Combine(outputDir, readmeFileName);

            Directory.CreateDirectory(outputDir);

            var doc = moduleTemplate
                .Replace("{{title}}", title)
                .Replace("{{breadcrumb}}", breadcrumb)
                .Replace("{{summary}}", summary)
                .Replace("{{model}}", model)
                .Replace("{{methods}}", BuildMethodRows(moduleType, kvasirAssembly));

            File.WriteAllText(outputPath, doc);
            Console.WriteLine($"Written: {outputPath}");
        }

        // Kvasir index
        var kvasirModules = moduleTypes
            .Select(t =>
            {
                var attr = t.GetCustomAttribute<ModuleAttribute>()!;
                return (segments: attr.Domain.Split('/'), name: t.Name, summary: attr.Summary, domain: attr.Domain);
            })
            .OrderBy(x => x.domain)
            .ToList();

        var kvasirDomainTable = new StringBuilder();
        var kvasirSections = new StringBuilder();

        foreach (var topLevel in kvasirModules.Select(m => m.segments[0]).Distinct().Order())
        {
            var anchor = $"_{topLevel.ToLowerInvariant()}";
            kvasirDomainTable.AppendLine($"| <<{anchor},{topLevel}>> | -");
        }

        BuildSections(kvasirModules, 0, 2, [], kvasirSections, readmeFileName);

        var kvasirIndexDoc = kvasirIndexTemplate
            .Replace("{{domain_table}}", kvasirDomainTable.ToString().TrimEnd())
            .Replace("{{modules}}", kvasirSections.ToString().TrimEnd());
        var indexPath = Path.Combine(kvasirSourceRoot, readmeFileName);
        File.WriteAllText(indexPath, kvasirIndexDoc);
        Console.WriteLine($"Written: {indexPath}");

        return 0;
    }

    private static void BuildSections(
        List<(string[] segments, string name, string summary, string domain)> modules,
        int depth,
        int sectionDepth,
        string[] prefix,
        StringBuilder sb,
        string readmeFileName)
    {
        var groups = modules
            .Where(m => m.segments.Length > depth)
            .GroupBy(m => m.segments[depth])
            .OrderBy(g => g.Key);

        foreach (var g in groups)
        {
            var anchor = "_" + string.Join("_", prefix.Append(g.Key).Select(s => s.ToLowerInvariant()));
            var heading = new string('=', sectionDepth);
            sb.AppendLine($"[[{anchor}]]");
            sb.AppendLine($"{heading} {g.Key}");
            sb.AppendLine();

            var leaves = g.Where(m => m.segments.Length == depth + 1).ToList();
            if (leaves.Count > 0)
            {
                sb.AppendLine("[cols=\"1,3\",options=\"header\"]");
                sb.AppendLine("|===");
                sb.AppendLine("| Module | Summary");
                foreach (var (_, name, summary, domain) in leaves)
                {
                    sb.AppendLine($"| link:{domain}/{readmeFileName}[{name}] | {summary}");
                }

                sb.AppendLine("|===");
                sb.AppendLine();
            }

            var children = g.Where(m => m.segments.Length > depth + 1).ToList();
            if (children.Count > 0)
            {
                BuildSections(children, depth + 1, sectionDepth + 1, [.. prefix, g.Key], sb, readmeFileName);
            }
        }
    }

    private static string BuildModuleBreadcrumb(string[] segments, string title, string readmeFileName)
    {
        var ups = string.Join("", Enumerable.Repeat("../", segments.Length));
        var sb = new StringBuilder();
        sb.Append($"xref:{ups}{readmeFileName}[Kvasir]");
        for (var i = 0; i < segments.Length; i++)
        {
            var segUps = string.Join("", Enumerable.Repeat("../", segments.Length - i - 1));
            sb.Append($" > xref:{segUps}{readmeFileName}[{segments[i]}]");
        }

        sb.Append($" > {title}");
        return sb.ToString();
    }

    private static string BuildMethodRows(Type moduleType, Assembly assembly)
    {
        var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
        var xmlDoc = File.Exists(xmlPath) ? XDocument.Load(xmlPath) : null;

        var sb = new StringBuilder();
        var methods = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var method in methods.OrderBy(m => m.Name))
        {
            var paramList = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            var returnType = method.ReturnType.Name;

            var paramTypeNames = string.Join(",",
                method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
            var memberName = paramTypeNames.Length > 0
                ? $"M:{moduleType.FullName}.{method.Name}({paramTypeNames})"
                : $"M:{moduleType.FullName}.{method.Name}";

            var description = xmlDoc?.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == memberName)
                ?.Element("summary")?.Value.Trim();
            description = description is not null
                ? string.Join(" ", description.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))
                : "-";

            sb.AppendLine($"| {method.Name} | {paramList} | {returnType} | {description}");
        }

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
    }
}
