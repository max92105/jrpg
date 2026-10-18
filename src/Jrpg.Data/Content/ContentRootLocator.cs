using System.Reflection;

namespace Jrpg.Data.Content;

/// <summary>
/// Where an app reads <c>content/</c> from, by one rule for the game and the editor: the <c>--content</c> argument,
/// else the folder the build stamped into the app (Debug builds point at the repository, so an edit shows on the next
/// start without rebuilding), else the <c>content</c> folder next to the app (a published build).
/// </summary>
public static class ContentRootLocator
{
    private const string ContentArgument = "--content";
    private const string ContentRootMetadataKey = "ContentRoot";

    public static string Find(IReadOnlyList<string> args, Assembly app) => Choose(args, BuiltInContentRoot(app));

    internal static string Choose(IReadOnlyList<string> args, string? builtInContentRoot)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (args[index] == ContentArgument)
            {
                return Path.GetFullPath(args[index + 1]);
            }
        }

        return builtInContentRoot ?? Path.Combine(AppContext.BaseDirectory, "content");
    }

    private static string? BuiltInContentRoot(Assembly app) =>
        app.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(metadata => metadata.Key == ContentRootMetadataKey)?.Value;
}
