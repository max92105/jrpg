using System.ComponentModel;
using Jrpg.Core.Party;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Party;
using Jrpg.Editor.Tiles;
using static Jrpg.Editor.Tests.Party.PartyMemberFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Party members page's list (decision 953): the members in id order, the added ones after, only an added member
// removable, the class box's choices, the problem line: the chosen member's own, then another's, then Data's message
// for the list; the missing art note, and an Import... taken into the chosen member's box.
public sealed class PartyMemberCatalogViewModelTests
{
    // Added after the scholar, its id first: a list in id order is sorted, not read in the dictionary's order.
    private static readonly PartyMemberDefinition _bram = Scholar with { Id = "bram", Name = "Bram" };

    private readonly PartyMemberCatalogViewModel _catalog = new(
        World with
        {
            PartyMembers = new Dictionary<string, PartyMemberDefinition> { [Scholar.Id] = Scholar, [_bram.Id] = _bram },
        },
        ImagesIn);

    [Fact]
    public void Constructor_Members_ListsThemInIdOrder()
    {
        Assert.Equal(["bram", "ilsa"], _catalog.Members.Select(listed => listed.Id));
        Assert.Same(_catalog.Members[0], _catalog.SelectedMember);
    }

    // The warden's name sorts after the scholar's while its id sorts before it: a list by id would differ.
    [Fact]
    public void Classes_ListsTheDraftsClassesByName()
    {
        var classes = new Dictionary<string, ClassDefinition>
        {
            [ClassFixtures.Archer.Id] = ClassFixtures.Archer with { Name = "Warden" },
            [ClassFixtures.Scholar.Id] = ClassFixtures.Scholar,
        };

        var catalog = new PartyMemberCatalogViewModel(World with { Classes = classes }, ImagesIn);

        Assert.Equal([new("scholar", "Scholar"), new RecordChoice("archer", "Warden")], catalog.Classes);
    }

    // Two members name the bard: it is listed once.
    [Fact]
    public void Classes_MembersNamingClassTheDraftLacks_ListsItOnceAsMissing()
    {
        var ilsa = Scholar with { ClassId = "bard" };
        var bram = _bram with { ClassId = "bard" };

        var catalog = new PartyMemberCatalogViewModel(World with
        {
            PartyMembers = new Dictionary<string, PartyMemberDefinition> { [ilsa.Id] = ilsa, [bram.Id] = bram },
        }, ImagesIn);

        Assert.Equal(["Archer (archer)", "Scholar (scholar)", "(missing) bard"], catalog.Classes.Select(choice => choice.Label));
    }

    [Fact]
    public void AddMember_AddsBlankLastAndChoosesIt()
    {
        _catalog.AddMemberCommand.Execute(null);

        var added = _catalog.Members[^1];
        Assert.Equal(3, _catalog.Members.Count);
        Assert.Same(added, _catalog.SelectedMember);
        Assert.Equal((true, "", "", ""), (added.IsNew, added.Id, added.Name, added.ClassId));
    }

    [Fact]
    public void RemoveMember_SavedMember_IsGreyed() => Assert.False(_catalog.RemoveMemberCommand.CanExecute(null));

    [Fact]
    public void RemoveMember_AddedMember_RemovesIt()
    {
        _catalog.AddMemberCommand.Execute(null);

        _catalog.RemoveMemberCommand.Execute(null);

        Assert.Equal(["bram", "ilsa"], _catalog.Members.Select(listed => listed.Id));
    }

    [Fact]
    public void Problem_Constructed_IsNull() => Assert.Null(_catalog.Problem);

    [Fact]
    public void Problem_ChosenMemberWithoutClass_ShowsItsOwn()
    {
        _catalog.Members[0].ClassId = "";

        Assert.Equal("Class must be chosen.", _catalog.Problem);
    }

    [Fact]
    public void Problem_AnotherMemberWithoutClass_NamesIt()
    {
        _catalog.Members[1].ClassId = "";

        Assert.Equal("Member ilsa: Class must be chosen.", _catalog.Problem);
    }

    [Fact]
    public void Problem_EmptySummary_ShowsDataMessage()
    {
        _catalog.Members[1].Summary = " ";

        Assert.Equal("party-members.json: party member 'ilsa': summary must not be empty.", _catalog.Problem);
    }

    [Fact]
    public void Problem_TypedChange_RaisesProblemChanged()
    {
        List<string?> raised = [];
        ((INotifyPropertyChanged)_catalog).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Members[1].Summary = "Reads in the dark.";

        Assert.Equal([nameof(PartyMemberCatalogViewModel.Problem), nameof(PartyMemberCatalogViewModel.MissingArtNote)], raised);
    }

    [Fact]
    public void MissingArtNote_EveryPictureInItsFolder_IsEmpty() => Assert.Equal("", _catalog.MissingArtNote);

    // Bram is chosen at start; the battle sprite and effect stay the folders' own.
    [Fact]
    public void MissingArtNote_PortraitNotInFolder_NamesIt()
    {
        _catalog.Members[0].PortraitFile = "BramFace.png";

        Assert.Equal("Not in the art folders: portrait BramFace.png.", _catalog.MissingArtNote);
    }

    [Fact]
    public void MissingArtNote_ArtNameTyped_RaisesChanged()
    {
        List<string?> raised = [];
        ((INotifyPropertyChanged)_catalog).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Members[0].AttackEffectFile = "Ink.png";

        Assert.Contains(nameof(PartyMemberCatalogViewModel.MissingArtNote), raised);
    }

    [Fact]
    public void MissingArtNote_OtherMemberChosen_RaisesChanged()
    {
        List<string?> raised = [];
        ((INotifyPropertyChanged)_catalog).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.SelectedMember = _catalog.Members[1];

        Assert.Contains(nameof(PartyMemberCatalogViewModel.MissingArtNote), raised);
    }

    // Bram is chosen at start; the import is the portrait's only.
    [Fact]
    public void TakeImport_CopiedPortrait_PicksItInThePortraitBox()
    {
        _catalog.TakeImport(ArtFolders.Portraits, new SheetImportResult(["Knight.png"], []));

        var bram = _catalog.Members[0];
        Assert.Equal(("Knight.png", _bram.BattleSpriteFile), (bram.PortraitFile, bram.BattleSpriteFile));
    }

    [Theory]
    [InlineData(ArtFolders.Battlers)]
    [InlineData(ArtFolders.Effects)]
    public void TakeImport_CopiedIntoAnotherFolder_PicksItInThatFoldersBox(string artFolder)
    {
        _catalog.TakeImport(artFolder, new SheetImportResult(["Knight.png"], []));

        var bram = _catalog.Members[0].ToMember();
        var picked = artFolder == ArtFolders.Battlers ? bram.BattleSpriteFile : bram.AttackEffectFile;
        Assert.Equal(("Knight.png", _bram.PortraitFile), (picked, bram.PortraitFile));
    }

    [Fact]
    public void TakeImport_NothingCopied_ChangesNothing()
    {
        List<string?> raised = [];
        ((INotifyPropertyChanged)_catalog).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.TakeImport(ArtFolders.Portraits, new SheetImportResult([], ["Bram.gif: not a PNG image."]));

        Assert.Equal((_bram, 0), (_catalog.Members[0].ToMember(), raised.Count));
    }

    [Fact]
    public void TakeImport_CopiedPortrait_RefreshesTheFolders()
    {
        List<string> portraits = ["IlsaPortrait.png"];
        var catalog = new PartyMemberCatalogViewModel(
            World,
            folder => folder == ArtFolders.Portraits ? [.. portraits] : ImagesIn(folder));
        portraits.Add("IlsaFace.png");

        catalog.TakeImport(ArtFolders.Portraits, new SheetImportResult(["IlsaFace.png"], []));

        Assert.Equal(["IlsaPortrait.png", "IlsaFace.png"], catalog.ArtChoices.Portraits);
        Assert.Equal("", catalog.MissingArtNote);
    }

    // The member already names the copied file, so no box changes: the note is read again all the same.
    [Fact]
    public void TakeImport_FileTheMemberAlreadyNames_RaisesMissingArtNote()
    {
        _catalog.Members[0].PortraitFile = "Knight.png";
        List<string?> raised = [];
        ((INotifyPropertyChanged)_catalog).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.TakeImport(ArtFolders.Portraits, new SheetImportResult(["Knight.png"], []));

        Assert.Equal([nameof(PartyMemberCatalogViewModel.MissingArtNote)], raised);
    }

    [Fact]
    public void ToMembers_Untouched_AreTheDraftsInIdOrder() => Assert.Equal([_bram, Scholar], _catalog.ToMembers());

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        _catalog.Choose("ilsa");

        Assert.Equal(("ilsa", "ilsa"), (_catalog.SelectedMember!.Id, _catalog.ChosenKey));
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        _catalog.Choose("ilsa");

        _catalog.Choose("nobody");

        Assert.Equal("ilsa", _catalog.ChosenKey);
    }

    [Fact]
    public void ChosenKey_NothingChosen_IsNull()
    {
        var noMembers = World with { PartyMembers = new Dictionary<string, PartyMemberDefinition>() };

        var empty = new PartyMemberCatalogViewModel(noMembers, ImagesIn);

        Assert.Null(empty.ChosenKey);
    }
}
