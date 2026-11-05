using Jrpg.Data.Audio;

namespace Jrpg.Data.Tests.Audio;

public sealed class AudioFoldersTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void FilesIn_RepositoryMusic_ListsTheTownLoopUnderItsFolder()
    {
        Assert.Contains("town/loop.wav", AudioFolders.FilesIn(_audioRoot, AudioFolders.Music));
    }

    [Fact]
    public void TracksIn_RepositoryMusic_ListsTheTitleLoopWithoutItsIntroAndOutro()
    {
        var tracks = AudioFolders.TracksIn(_audioRoot);

        Assert.Contains("titlescreen/loop.wav", tracks);
        Assert.DoesNotContain("titlescreen/intro.wav", tracks);
        Assert.DoesNotContain("titlescreen/outro.wav", tracks);
    }

    // Ordinal, so capitals come first whatever the file system returns; other kinds of file aren't tracks; a track in a
    // folder of its own is named under it with '/' (decision 496).
    [Fact]
    public void FilesIn_MixedFolder_ListsOnlyWavsInOrdinalOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"jrpg-audio-{Guid.NewGuid():N}");
        var folder = Directory.CreateDirectory(Path.Combine(root, AudioFolders.Music)).FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(folder, "town"));
            foreach (var file in new[] { "a.wav", "B.wav", "notes.txt", Path.Combine("town", "loop.wav") })
            {
                File.WriteAllText(Path.Combine(folder, file), "");
            }

            Assert.Equal(["B.wav", "a.wav", "town/loop.wav"], AudioFolders.FilesIn(root, AudioFolders.Music));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void FilesIn_MissingFolder_IsEmpty()
    {
        Assert.Empty(AudioFolders.FilesIn(_audioRoot, "no_such_folder"));
    }
}
