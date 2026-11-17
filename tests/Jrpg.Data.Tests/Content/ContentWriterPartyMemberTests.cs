using System.Text.Json.Nodes;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// party-members.json is written with the other catalogs (decision 952), on a copy of the test world: checked against
// the classes as the loader checks it, sorted by id.
public sealed class ContentWriterPartyMemberTests : IDisposable
{
    private const string FileName = "party-members.json";

    private static readonly ClassDefinition _ranger =
        new("ranger", "Ranger", "A hunter who waits for the shot.", new ClassStats(160, 14), new ClassStats(15, 1));

    private readonly CopiedContentRoot _content = new("writer-party-members", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterPartyMemberTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_PartyMembers_LoadTheSameAfterRelay()
    {
        var written = With(Member("aaa") with { Name = "Ilsa", Summary = "A scout who never sleeps twice in one place." });

        _writer.WriteMapCatalogs(written);

        var reloaded = ContentCatalogLoader.Load(_content.Path).PartyMembers;
        Assert.Equal(ById(written.PartyMembers.Values), ById(reloaded.Values));
    }

    // Added after the test world's, the later id first: unsorted, the file would end on "zzz", "aaa".
    [Fact]
    public void WriteMapCatalogs_NewMembers_WritesThemSortedById()
    {
        _writer.WriteMapCatalogs(With(Member("zzz"), Member("aaa")));

        var ids = JsonNode.Parse(File.ReadAllText(_content.PathOf(FileName)))!.AsArray()
            .Select(member => (string?)member!["id"])
            .ToList();
        Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
        Assert.Equal("aaa", ids[0]);
        Assert.Equal("zzz", ids[^1]);
    }

    [Fact]
    public void WriteMapCatalogs_MemberNamingUnknownClass_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(Member("aaa") with { ClassId = "sniper" })));

        Assert.Equal("party-members.json: party member 'aaa': classId 'sniper' is not a known class.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // The members as saved, only the ranger class gone: the draft's classes, not a member's edit, refuse the save.
    [Fact]
    public void WriteMapCatalogs_ClassRemovedWhileAMemberNamesIt_WritesNothing()
    {
        var saved = With(Member("aaa"));
        _writer.WriteMapCatalogs(saved);
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(saved with { Classes = _catalog.Classes }));

        Assert.Equal("party-members.json: party member 'aaa': classId 'ranger' is not a known class.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    private static PartyMemberDefinition Member(string id) =>
        new()
        {
            Id = id,
            Name = "Ranger",
            ClassId = _ranger.Id,
            Summary = "A hunter who waits for the shot.",
            PortraitFile = "Ranger.png",
            BattleSpriteFile = "Ranger.png",
            AttackEffectFile = "Arrow.png",
        };

    private static List<PartyMemberDefinition> ById(IEnumerable<PartyMemberDefinition> members) =>
        [.. members.OrderBy(member => member.Id, StringComparer.Ordinal)];

    // The test world's members and classes stay, with the ranger the added members name.
    private ContentCatalog With(params PartyMemberDefinition[] members)
    {
        var all = new Dictionary<string, PartyMemberDefinition>(_catalog.PartyMembers);
        foreach (var member in members)
        {
            all.Add(member.Id, member);
        }

        var classes = new Dictionary<string, ClassDefinition>(_catalog.Classes) { [_ranger.Id] = _ranger };
        return _catalog with { Classes = classes, PartyMembers = all };
    }
}
