using System.Reflection;
using System.Runtime.Versioning;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Architecture;

public sealed class CoreDependencyTests
{
    private static readonly string[] _forbiddenAssemblyPrefixes =
    [
        "PresentationCore",
        "PresentationFramework",
        "WindowsBase",
        "System.Windows",
        "MonoGame",
        "Microsoft.Xna",
    ];

    private static readonly Assembly _core = typeof(Vitals).Assembly;

    [Fact]
    public void CoreAssembly_ReferencedAssemblies_ContainNoUiOrEngineAssembly()
    {
        var forbiddenReferences = _core.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => _forbiddenAssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(forbiddenReferences);
    }

    // The compiler drops references to unused assemblies, so the test above can't see a Core project
    // switched to net10.0-windows with UseWPF. That switch stamps the assembly with a target platform.
    [Fact]
    public void CoreAssembly_TargetPlatform_IsPlatformNeutral()
    {
        var targetPlatform = _core.GetCustomAttribute<TargetPlatformAttribute>();

        Assert.Null(targetPlatform);
    }
}
