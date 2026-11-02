using System.Text.Json;
using System.Text.Json.Serialization;
using Jrpg.Data.Audio;

namespace Jrpg.Data.Tests.Audio;

// The placeholder effects' manifest (ADR 0036) against the effects folder and the generated credits: the game never
// reads the manifest, so these tests are what keeps the three in step. What the fetch script itself refuses (a repeated
// key) and what freesound doesn't promise (its URL layouts) aren't checked here.
public sealed class SoundSourcesTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));

    private static readonly string[] _allowedLicenses = ["CC0-1.0", "CC-BY-3.0", "CC-BY-4.0"];

    // Unmapped members refused, so a misspelt field fails here rather than being ignored by the script.
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    };

    // Compared with the folder's listing, since File.Exists ignores case on Windows and the release may not.
    [Fact]
    public void Manifest_EveryEffectFile_IsSourcedOrATune()
    {
        var manifest = ReadManifest();
        var named = manifest.Sounds.Select(sound => sound.File).Concat(manifest.Tunes).ToHashSet(StringComparer.Ordinal);
        var effects = Directory.GetFiles(Path.Combine(_audioRoot, AudioFolders.Effects))
            .Select(path => $"{AudioFolders.Effects}/{Path.GetFileName(path)}");

        Assert.All(effects, file => Assert.True(named.Contains(file), $"{file} has no entry in sound-sources.json."));
    }

    // The other side: an entry whose file was deleted would still be credited for a sound the game doesn't ship.
    [Fact]
    public void Manifest_EveryNamedFile_Exists()
    {
        var manifest = ReadManifest();
        var named = manifest.Sounds.Select(sound => sound.File).Concat(manifest.Tunes);

        Assert.All(named, file => Assert.True(
            Directory.GetFiles(Path.GetDirectoryName(Path.Combine(_audioRoot, file))!).Select(Path.GetFileName)
                .Contains(Path.GetFileName(file)),
            $"sound-sources.json names {file}, which content/audio doesn't hold."));
    }

    [Fact]
    public void Manifest_EveryLicense_IsAllowed()
    {
        Assert.All(ReadManifest().Sounds, sound => Assert.True(
            _allowedLicenses.Contains(sound.License),
            $"{sound.File}: license '{sound.License}' isn't allowed (decision 427)."));
    }

    // A manifest edit committed without rerunning the script leaves the credits behind; the row is matched up to the
    // license, so a hand-edited author or license shows too, the attribution a CC-BY sound needs right.
    [Fact]
    public void Credits_EveryEntry_IsListed()
    {
        var credits = File.ReadAllText(Path.Combine(_audioRoot, "CREDITS.md"));

        Assert.All(ReadManifest().Sounds, sound => Assert.True(
            credits.Contains(CreditsRowStart(sound), StringComparison.Ordinal),
            $"CREDITS.md doesn't list {sound.File} as the manifest says: rerun fetch-placeholder-sounds.ps1."));
    }

    // As fetch-placeholder-sounds.ps1's Write-Credits writes it.
    private static string CreditsRowStart(SoundSource sound) =>
        $"| `{sound.File}` | [{sound.Name.Replace("|", "\\|")}]({sound.Page}) | {sound.Author} | [{sound.License}](";

    private static SoundSources ReadManifest()
    {
        var text = File.ReadAllText(Path.Combine(_audioRoot, "sound-sources.json"));
        return JsonSerializer.Deserialize<SoundSources>(text, _options)!;
    }

    private sealed record SoundSources(double PeakDb, IReadOnlyList<string> Tunes, IReadOnlyList<SoundSource> Sounds);

    private sealed record SoundSource(
        string File,
        int FreesoundId,
        string Name,
        string Author,
        string License,
        string Page,
        string Preview,
        string Fetched,
        double MaxSeconds,
        double? StartSeconds,
        double? FadeOutSeconds,
        double? GainDb);
}
