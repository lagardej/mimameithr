using Autodoc;

const string outputFileName = "index.adoc";

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: Autodoc <nornir|kvasir> <project-root>");
    return 1;
}

var command = args[0].ToLowerInvariant();
var projectRoot = args[1];

return command switch
{
    "nornir" => NornirAutodoc.Run(projectRoot, outputFileName),
    "kvasir" => KvasirAutodoc.Run(projectRoot, outputFileName),
    _ => Error($"Unknown command '{command}'. Expected 'nornir' or 'kvasir'.")
};

static int Error(string message)
{
    Console.Error.WriteLine(message);
    return 1;
}
