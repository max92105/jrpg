using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests;

public sealed class EditorAssemblyTests
{
    [Fact]
    public void EditorAssembly_ReferencedAssemblies_DoNotIncludeJrpgGame()
    {
        var referenced = typeof(MainViewModel).Assembly.GetReferencedAssemblies().Select(name => name.Name);

        Assert.DoesNotContain("Jrpg.Game", referenced);
    }
}
