using System.Reflection;
using Jrpg.Data.Audio;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class UiSoundFilesTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    // Compared with the folder's list, since File.Exists ignores case on Windows and the release may not.
    [Theory]
    [InlineData(typeof(UiSoundFiles.Music), AudioFolders.Music)]
    [InlineData(typeof(UiSoundFiles.Effects), AudioFolders.Effects)]
    [InlineData(typeof(UiSoundFiles.Battle), AudioFolders.Effects)]
    [InlineData(typeof(UiSoundFiles.Field), AudioFolders.Effects)]
    public void EveryConstant_ExistsInItsFolder(Type group, string folder)
    {
        var files = group.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToList();
        var present = AudioFolders.FilesIn(_audioRoot, folder);

        Assert.NotEmpty(files);
        Assert.All(files, file => Assert.True(present.Contains(file), $"content/audio/{folder} has no '{file}'."));
    }

    [Fact]
    public void EveryGroup_IsCheckedAbove()
    {
        Assert.Equal(4, typeof(UiSoundFiles).GetNestedTypes().Length);
    }
}
