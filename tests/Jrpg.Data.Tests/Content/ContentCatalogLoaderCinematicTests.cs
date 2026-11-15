using System.Text.Json.Nodes;
using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// cinematics.json is loaded with the catalog (ADR 0046), on a copy of the test world whose file gains the test's
// cinematics beside the frozen intros' ones, which its intro map plays.
public sealed class ContentCatalogLoaderCinematicTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("cinematics", "test-world");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_NoCinematicsFile_ThrowsNamingIt()
    {
        File.Delete(_content.PathOf("cinematics.json"));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("cinematics.json: the file is missing.", exception.Message, StringComparison.Ordinal);
    }

    // The second still is named before the first in the alphabet: the order read is the file's.
    [Fact]
    public void Load_CinematicsFile_ReadsStillsInOrder()
    {
        WriteCinematics("""
            [
              {
                "id": "intro",
                "musicFile": "town/loop.wav",
                "stills": [
                  {
                    "imageFile": "intro/b.png",
                    "seconds": 4.5,
                    "caption": "Long ago.",
                    "voiceFile": "intro_1.wav",
                    "zoom": {
                      "startPercent": 110,
                      "endPercent": 130
                    }
                  },
                  {
                    "imageFile": "intro/a.png",
                    "seconds": 3
                  }
                ]
              }
            ]
            """);

        var intro = ContentCatalogLoader.Load(_content.Path).Cinematics["intro"];

        Assert.Equal("town/loop.wav", intro.MusicFile);
        Assert.Equal(["intro/b.png", "intro/a.png"], intro.Stills.Select(still => still.ImageFile));
        var first = intro.Stills[0];
        Assert.Equal((4.5, "Long ago.", "intro_1.wav"), (first.Seconds, first.Caption, first.VoiceFile));
        Assert.Equal(new StillZoom { StartPercent = 110, EndPercent = 130 }, first.Zoom);
    }

    [Fact]
    public void Load_StillWithoutOptionalFields_ReadsThemAsNull()
    {
        WriteCinematics("""[ { "id": "intro", "stills": [ { "imageFile": "intro/a.png", "seconds": 3 } ] } ]""");

        var intro = ContentCatalogLoader.Load(_content.Path).Cinematics["intro"];

        var still = Assert.Single(intro.Stills);
        Assert.Null(intro.MusicFile);
        Assert.Equal((null, null, null), (still.Caption, still.VoiceFile, still.Zoom));
    }

    [Fact]
    public void Load_InvalidStill_ThrowsNamingTheFile()
    {
        WriteCinematics("""[ { "id": "intro", "stills": [ { "imageFile": "intro/a.png", "seconds": 0 } ] } ]""");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("cinematics.json: cinematic 'intro': stills[0]: seconds must be above 0 (was 0).", exception.Message);
    }

    // Decision 621: the still step left; content still holding one is refused by the file that holds it.
    [Fact]
    public void Load_LayoutWithAStillStep_IsRefusedAsUnknownType()
    {
        _content.Replace(Path.Combine("map-layouts", "intro.json"), "\"type\": \"cinematic\"", "\"type\": \"still\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("intro.json at $.cellEvents[0].event.steps[1]", exception.Message, StringComparison.Ordinal);
        Assert.Contains("still", exception.Message, StringComparison.Ordinal);
    }

    // Beside the frozen intros' cinematics, which the test world's intro map plays.
    private void WriteCinematics(string json)
    {
        var all = JsonNode.Parse(File.ReadAllText(_content.PathOf("cinematics.json")))!.AsArray();
        foreach (var cinematic in JsonNode.Parse(json)!.AsArray().ToList())
        {
            all.Add(cinematic!.DeepClone());
        }

        File.WriteAllText(_content.PathOf("cinematics.json"), all.ToJsonString());
    }
}
