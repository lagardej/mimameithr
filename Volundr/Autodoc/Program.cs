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
        .Replace("{{parameters}}", BuildParameterRows(assembly, ns))
        .Replace("{{states}}", BuildStateRows(assembly, ns))
        .Replace("{{forcings}}", BuildForcingRows(assembly, ns));

    File.WriteAllText(outputPath, doc);
    Console.WriteLine($"Written: {outputPath}");
}

// Generate components index
var indexRows = new StringBuilder();
foreach (var componentType in componentTypes)
{
    var attr = componentType.GetCustomAttribute<ComponentAttribute>()!;
    var ns = componentType.Namespace ?? string.Empty;
    var nsPrefix = "Nornir.";
    var relativePath = ns.StartsWith(nsPrefix)
        ? ns[nsPrefix.Length..].Replace('.', Path.DirectorySeparatorChar)
        : string.Empty;
    var title = attr.Title ?? (relativePath.Length > 0 ? Path.GetFileName(relativePath) : componentType.Name);
    var summary = attr.Summary ?? "-";
    indexRows.AppendLine($"| {title} | {summary}");
}

var indexDoc = indexTemplate.Replace("{{components}}", indexRows.ToString().TrimEnd());
File.WriteAllText(Path.Combine(sourceRoot, "README.adoc"), indexDoc);
Console.WriteLine($"Written: {Path.Combine(sourceRoot, "README.adoc")}");

return 0;

static string BuildParameterRows(Assembly assembly, string ns)
{
    var sb = new StringBuilder();
    var holders = assembly.GetTypes()
        .Where(t => t.Namespace == ns && t.GetCustomAttribute<ParametersAttribute>() is not null);
    foreach (var holder in holders)
    foreach (var member in GetAnnotatedMembers(holder, typeof(ParameterAttribute)))
    {
        var a = member.GetCustomAttribute<ParameterAttribute>()!;
        sb.AppendLine($"| {member.Name} | {a.Unit} | {a.Purpose}");
    }

    return sb.Length > 0 ? sb.ToString().TrimEnd() : "_None declared._";
}

static string BuildStateRows(Assembly assembly, string ns)
{
    var sb = new StringBuilder();
    var holders = assembly.GetTypes()
        .Where(t => t.Namespace == ns && t.GetCustomAttribute<StatesAttribute>() is not null);
    foreach (var holder in holders)
    foreach (var member in GetAnnotatedMembers(holder, typeof(StateAttribute)))
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
