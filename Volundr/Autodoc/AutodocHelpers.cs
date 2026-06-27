using System.Reflection;
using System.Xml.Linq;

internal static class AutodocHelpers
{
    public static string? GetXmlSummary(Type type)
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

    public static IEnumerable<MemberInfo> GetAnnotatedMembers(Type type, Type attributeType) =>
        type.GetProperties().Cast<MemberInfo>()
            .Concat(type.GetFields())
            .Where(m => m.GetCustomAttribute(attributeType) is not null);
}
