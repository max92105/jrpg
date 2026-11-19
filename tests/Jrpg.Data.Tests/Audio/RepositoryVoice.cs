using Jrpg.Data.Audio;

namespace Jrpg.Data.Tests.Audio;

/// <summary>
/// A voice line the repository's voices folder holds, for the tests that need one that is really there. No name is
/// pinned: the developer renames and replaces the voices, so the tests take the first by name.
/// </summary>
internal static class RepositoryVoice
{
    public static string AudioRoot { get; } = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <summary>Its name under the voices folder, its subfolders included.</summary>
    /// <exception cref="InvalidOperationException">The folder holds no voice line.</exception>
    public static string Name { get; } =
        AudioFolders.FilesIn(AudioRoot, AudioFolders.Voices) is [var first, ..]
            ? first
            : throw new InvalidOperationException("The repository's voices folder holds no .wav; these tests need one.");
}
