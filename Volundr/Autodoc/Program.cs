using Brunnr.Autodoc;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: Autodoc <project-root>");
    return 1;
}

var projectRoot = args[0];

var assemblyPath = Path.Combine(projectRoot, "Nornir", "bin", "Debug", "net10.0", "Nornir.dll");
var sourceRoot = Path.Combine(projectRoot, "Nornir");
var autodocRoot = Path.Combine(projectRoot, "Volundr", "Autodoc");
var templatePath = Path.Combine(autodocRoot, "component-template.adoc");
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

if (!File.Exists(indexTemplatePath))
{
    Console.Error.WriteLine($"Index template not found: {indexTemplatePath}");
    return 1;
}

var template = File.ReadAllText(templatePath);
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

foreach (var componentType in componentTypes)
{
    var attr = componentType.GetCustomAttribute<ComponentAttribute>()!;
    var ns = componentType.Namespace ?? string.Empty;

    var nsPrefix = "Nornir.";
    var relativePath = ns.StartsWith(nsPrefix)
        ? ns[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
        : string.Empty;

    var title = attr.Title ?? (relativePath.Length > 0 ? Path.GetFileName(relativePath) : componentType.Name);
    var summary = GetXmlSummary(componentType) ?? "_No summary provided._";
    var outputDir = Path.Combine(sourceRoot, relativePath);
    var outputPath = Path.Combine(outputDir, "README.adoc");

    Directory.CreateDirectory(outputDir);

    var doc = template
        .Replace("{{title}}", title)
        .Replace("{{summary}}", summary)
        .Replace("{{parameters}}", BuildParameterRows(componentType))
        .Replace("{{states}}", BuildStateRows(componentType))
        .Replace("{{forcings}}", BuildForcingRows(assembly, ns));

    File.WriteAllText(outputPath, doc);
    Console.WriteLine($"Written: {outputPath}");
}

// Build group summary lookup from [Group]-annotated types
var groupSummaries = assembly.GetTypes()
    .Where(t => t.GetCustomAttribute<GroupAttribute>() is not null)
    .ToDictionary(
        t => t.Name,
        t => t.GetCustomAttribute<GroupAttribute>()!.Summary);

// Generate grouped components index
var grouped = componentTypes
    .Select(t =>
    {
        var attr = t.GetCustomAttribute<ComponentAttribute>()!;
        var group = attr.Group ?? string.Empty;
        var slash = group.IndexOf('/');
        var topLevel = slash >= 0 ? group[..slash] : group;
        var subLevel = slash >= 0 ? group[(slash + 1)..] : string.Empty;
        var ns = t.Namespace ?? string.Empty;
        var nsPrefix = "Nornir.";
        var relativePath = ns.StartsWith(nsPrefix)
            ? ns[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
            : string.Empty;
        var title = attr.Title ?? (relativePath.Length > 0 ? Path.GetFileName(relativePath) : t.Name);
        var summary = attr.Summary ?? "-";
        return (topLevel, subLevel, title, summary, relativePath);
    })
    .GroupBy(x => x.topLevel)
    .OrderBy(g => g.Key);

var indexSections = new StringBuilder();
var groupList = new StringBuilder();
foreach (var g in grouped)
{
    var groupSummary = groupSummaries.GetValueOrDefault(g.Key + "Group");
    var anchor = $"_{g.Key.ToLowerInvariant()}";
    groupList.AppendLine(groupSummary is not null
        ? $"* <<{anchor},{g.Key}>> — {groupSummary}"
        : $"* <<{anchor},{g.Key}>>");
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
    foreach (var (_, subLevel, title, summary, relativePath) in g.OrderBy(x => x.subLevel))
    {
        var label = string.IsNullOrEmpty(subLevel) ? title : subLevel;
        var readmePath = relativePath.Length > 0
            ? $"link:{relativePath.Replace(Path.DirectorySeparatorChar, '/')}/README.adoc[{label}]"
            : label;
        indexSections.AppendLine($"| {readmePath} | {summary}");
    }

    indexSections.AppendLine("|===");
    indexSections.AppendLine();
}

var indexDoc = indexTemplate
    .Replace("{{group_list}}", groupList.ToString().TrimEnd())
    .Replace("{{components}}", indexSections.ToString().TrimEnd());
File.WriteAllText(Path.Combine(sourceRoot, "README.adoc"), indexDoc);
Console.WriteLine($"Written: {Path.Combine(sourceRoot, "README.adoc")}");

return 0;

static string BuildParameterRows(Type componentType)
{
    var sb = new StringBuilder();
    foreach (var member in GetAnnotatedMembers(componentType, typeof(SettingAttribute)))
    {
        var a = member.GetCustomAttribute<SettingAttribute>()!;
        sb.AppendLine($"| {member.Name} | {a.Unit} | {a.Purpose}");
    }

    return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
}

static string BuildStateRows(Type componentType)
{
    var sb = new StringBuilder();
    foreach (var member in GetAnnotatedMembers(componentType, typeof(StateAttribute)))
    {
        var a = member.GetCustomAttribute<StateAttribute>()!;
        sb.AppendLine($"| {member.Name} | {a.Unit} | {a.Purpose}");
    }

    return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
}

static string BuildForcingRows(Assembly assembly, string ns)
{
    var sb = new StringBuilder();
    var forcings = assembly.GetTypes()
        .Where(t => t.Namespace == ns && t.GetCustomAttribute<ForcingAttribute>() is not null);
    foreach (var f in forcings)
    {
        var summary = GetXmlSummary(f) ?? "-";
        sb.AppendLine($"| {f.Name} | {summary}");
    }

    return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
}

static IEnumerable<MemberInfo> GetAnnotatedMembers(Type type, Type attributeType) =>
    type.GetProperties().Cast<MemberInfo>()
        .Concat(type.GetFields())
        .Where(m => m.GetCustomAttribute(attributeType) is not null);

static string? GetXmlSummary(Type type)
{
    var xmlPath = Path.ChangeExtension(type.Assembly.Location, ".xml");
    if (!File.Exists(xmlPath))
    {
        return null;
    }

    var doc = XDocument.Load(xmlPath);
    var memberName = $"T:{type.FullName}";
    var summary = doc.Descendants("member")
        .FirstOrDefault(m => m.Attribute("name")?.Value == memberName)
        ?.Element("summary")
        ?.Value
        .Trim();

    if (summary is null)
    {
        return null;
    }

    return string.Join(" ", summary.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0));
}
