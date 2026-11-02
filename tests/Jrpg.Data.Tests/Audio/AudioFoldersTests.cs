using Jrpg.Data.Audio;

namespace Jrpg.Data.Tests.Audio;

public sealed class AudioFoldersTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void FilesIn_RepositoryMusic_ListsAPlaceholderTrack()
    {
        Assert.Contains("PlaceholderTown.wav", AudioFolders.FilesIn(_audioRoot, AudioFolders.Music));
    }

    // Ordinal, so capitals come first whatever the file system returns; other kinds of file aren't tracks.
    [Fact]
    public void FilesIn_MixedFolder_ListsOnlyWavsInOrdinalOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"jrpg-audio-{Guid.NewGuid():N}");
        var folder = Directory.CreateDirectory(Path.Combine(root, AudioFolders.Music)).FullName;
        try
        {
            foreach (var file in new[] { "a.wav", "B.wav", "notes.txt" })
            {
                File.WriteAllText(Path.Combine(folder, file), "");
            }

            Assert.Equal(["B.wav", "a.wav"], AudioFolders.FilesIn(root, AudioFolders.Music));
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
