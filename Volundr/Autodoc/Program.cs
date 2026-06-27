using Brunnr.Autodoc;
using Kvasir;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

const string indexFileName = "index.adoc";

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
    var outputPath = Path.Combine(outputDir, indexFileName);

    Directory.CreateDirectory(outputDir);

    var doc = template
        .Replace("{{title}}", title)
        .Replace("{{breadcrumb}}", $"xref:../../{indexFileName}[Nornir] > xref:../{indexFileName}[{Path.GetDirectoryName(relativePath)}] > {title}")
        .Replace("{{summary}}", summary)
        .Replace("{{parameters}}", BuildParameterRows(componentType))
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
const string nsPrefix2 = "Nornir.";
var grouped = componentTypes
    .Select(t =>
    {
        var attr = t.GetCustomAttribute<ComponentAttribute>()!;
        var group = attr.Group;
        var slash = group.IndexOf('/');
        var topLevel = slash >= 0 ? group[..slash] : group;
        var subLevel = slash >= 0 ? group[(slash + 1)..] : string.Empty;
        var ns = t.Namespace ?? string.Empty;
        var relativePath = ns.StartsWith(nsPrefix2)
            ? ns[nsPrefix2.Length..].Replace('.', Path.DirectorySeparatorChar)
            : string.Empty;
        var title = attr.Title ?? (relativePath.Length > 0 ? Path.GetFileName(relativePath) : t.Name);
        var summary = attr.Summary ?? "-";
        return (topLevel, subLevel, title, summary, relativePath, Type: t);
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
    var groupRelativePath = groupNs.StartsWith(nsPrefix2)
        ? groupNs[nsPrefix2.Length..].Replace('.', Path.DirectorySeparatorChar)
        : string.Empty;
    var groupOutputDir = Path.Combine(sourceRoot, groupRelativePath);
    var groupOutputPath = Path.Combine(groupOutputDir, indexFileName);

    var componentRows = new StringBuilder();
    foreach (var (_, subLevel, title, _, relativePath, componentType) in g.OrderBy(x => x.subLevel))
    {
        var label = string.IsNullOrEmpty(subLevel) ? title : subLevel;
        var compSummary = GetXmlSummary(componentType) ?? "-";
        var relLink = Path.GetRelativePath(groupRelativePath, relativePath).Replace(Path.DirectorySeparatorChar, '/');
        componentRows.AppendLine($"| link:{relLink}/{indexFileName}[{label}] | {compSummary}");
    }

    var groupDoc = groupTemplate
        .Replace("{{title}}", g.Key)
        .Replace("{{breadcrumb}}", $"xref:../{indexFileName}[Nornir] > " + g.Key)
        .Replace("{{summary}}", groupAttr.Summary ?? GetXmlSummary(groupType) ?? "_No summary provided._")
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
    var groupRelativePath = groupNs.StartsWith(nsPrefix2)
        ? groupNs[nsPrefix2.Length..].Replace('.', '/')
        : string.Empty;
    var groupLink = groupRelativePath.Length > 0
        ? $"link:{groupRelativePath}/{indexFileName}[{g.Key}]"
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
            ? $"link:{relativePath.Replace(Path.DirectorySeparatorChar, '/')}/{indexFileName}[{label}]"
            : label;
        indexSections.AppendLine($"| {readmePath} | {summary}");
    }

    indexSections.AppendLine("|===");
    indexSections.AppendLine();
}

var indexDoc = indexTemplate
    .Replace("{{group_table}}", groupTable.ToString().TrimEnd())
    .Replace("{{components}}", indexSections.ToString().TrimEnd());
File.WriteAllText(Path.Combine(sourceRoot, indexFileName), indexDoc);
Console.WriteLine($"Written: {Path.Combine(sourceRoot, indexFileName)}");

// ── Kvasir pass ─────────────────────────────────────────────────────────────

var kvasirAssemblyPath = Path.Combine(projectRoot, "Kvasir", "bin", "Debug", "net10.0", "Kvasir.dll");
var kvasirSourceRoot = Path.Combine(projectRoot, "Kvasir");
var moduleTemplatePath = Path.Combine(autodocRoot, "module-template.adoc");
var kvasirIndexTemplatePath = Path.Combine(autodocRoot, "kvasir-index-template.adoc");

if (!File.Exists(kvasirAssemblyPath))
{
    Console.Error.WriteLine($"Kvasir assembly not found: {kvasirAssemblyPath}");
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
    var xmlSummary = GetXmlSummary(moduleType);
    var summary = attr.Summary;
    var model = xmlSummary ?? "_No model description provided._";

    // Breadcrumb: depth = number of segments in domain path
    var segments = domain.Split('/');
    var breadcrumb = BuildModuleBreadcrumb(segments, title);

    var outputDir = Path.Combine(kvasirSourceRoot, relativePath);
    var outputPath = Path.Combine(outputDir, indexFileName);

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

// Top-level table: unique first segments
foreach (var topLevel in kvasirModules.Select(m => m.segments[0]).Distinct().Order())
{
    var anchor = $"_{topLevel.ToLowerInvariant()}";
    kvasirDomainTable.AppendLine($"| <<{anchor},{topLevel}>> | -");
}

// Recursive section builder
BuildSections(kvasirModules, depth: 0, sectionDepth: 2, prefix: [], kvasirSections);

void BuildSections(
    List<(string[] segments, string name, string summary, string domain)> modules,
    int depth,
    int sectionDepth,
    string[] prefix,
    StringBuilder sb)
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

        // Modules whose domain ends exactly at this depth
        var leaves = g.Where(m => m.segments.Length == depth + 1).ToList();
        if (leaves.Count > 0)
        {
            sb.AppendLine("[cols=\"1,3\",options=\"header\"]");
            sb.AppendLine("|===");
            sb.AppendLine("| Module | Summary");
            foreach (var (_, name, summary, domain) in leaves)
            {
                sb.AppendLine($"| link:{domain.Replace('/', '/')}/{indexFileName}[{name}] | {summary}");
            }

            sb.AppendLine("|===");
            sb.AppendLine();
        }

        // Recurse into children
        var children = g.Where(m => m.segments.Length > depth + 1).ToList();
        if (children.Count > 0)
        {
            BuildSections(children, depth + 1, sectionDepth + 1, [..prefix, g.Key], sb);
        }
    }
}

var kvasirIndexDoc = kvasirIndexTemplate
    .Replace("{{domain_table}}", kvasirDomainTable.ToString().TrimEnd())
    .Replace("{{modules}}", kvasirSections.ToString().TrimEnd());
File.WriteAllText(Path.Combine(kvasirSourceRoot, indexFileName), kvasirIndexDoc);
Console.WriteLine($"Written: {Path.Combine(kvasirSourceRoot, indexFileName)}");

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

static string BuildModuleBreadcrumb(string[] segments, string title)
{
    // e.g. segments = ["Natural", "Physical", "Geology"], title = "MantlePhysics"
    // depth from module README to Kvasir root = segments.Length
    var ups = string.Join("", Enumerable.Repeat("../", segments.Length));
    var sb = new StringBuilder();
    sb.Append($"xref:{ups}{indexFileName}[Kvasir]");
    for (var i = 0; i < segments.Length; i++)
    {
        var segUps = string.Join("", Enumerable.Repeat("../", segments.Length - i - 1));
        sb.Append($" > xref:{segUps}{indexFileName}[{segments[i]}]");
    }
    sb.Append($" > {title}");
    return sb.ToString();
}

static string BuildMethodRows(Type moduleType, Assembly assembly)
{
    var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
    XDocument? xmlDoc = File.Exists(xmlPath) ? XDocument.Load(xmlPath) : null;

    var sb = new StringBuilder();
    var methods = moduleType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
    foreach (var method in methods.OrderBy(m => m.Name))
    {
        var paramList = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        var returnType = method.ReturnType.Name;

        // Build XML member name: M:Namespace.Type.Method(ParamTypes)
        var paramTypeNames = string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
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
