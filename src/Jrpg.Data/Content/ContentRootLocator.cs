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
    private const string ArtRootMetadataKey = "ArtRoot";

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

    /// <summary>
    /// Where art is read from (ADR 0009): until the content cutover (slice 10) a Debug build points at the repository's
    /// legacy <c>Assets/</c>; otherwise <c>{content}/art</c>.
    /// </summary>
    public static string FindArtRoot(Assembly app, string contentRoot) => ChooseArtRoot(BuiltIn(app, ArtRootMetadataKey), contentRoot);

    internal static string ChooseArtRoot(string? builtInArtRoot, string contentRoot) => builtInArtRoot ?? Path.Combine(contentRoot, "art");

    private static string? BuiltInContentRoot(Assembly app) => BuiltIn(app, ContentRootMetadataKey);

    private static string? BuiltIn(Assembly app, string key) =>
        app.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(metadata => metadata.Key == key)?.Value;
}
