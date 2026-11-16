using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Sprites;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// The Character sprites page's state (decisions 569, 570): every image in the field-sprite folder, set up or not, then
/// every record whose image is missing; the chosen image's setup, each opened setup kept while the page is open; Copy
/// setup from another set-up image; the problem that greys its host's OK; and the records changed, for one undo step. A
/// setup counts (for OK and the records) once it was edited, or its image imported in this page with a guess that fits:
/// choosing an image already in the folder and leaving it makes no record, but an import means "set this up". Reads no
/// file itself: the folder's listing and an image's size come in as lookups. An image is chosen by its file, a record
/// whose image is missing included.
/// </summary>
internal sealed partial class FieldSpriteCatalogViewModel : ObservableObject, IEditedCatalog
{
    private readonly CatalogDraft _catalog;
    private readonly Func<IReadOnlyList<string>> _listFiles;
    private readonly Func<string, (int Width, int Height)?> _sizeOf;
    private readonly Dictionary<string, FieldSpriteSetupViewModel> _opened = new(StringComparer.Ordinal);
    private readonly HashSet<string> _edited = new(StringComparer.Ordinal);

    // A copy refused because the image doesn't split into the source's frames; shown until the next edit.
    private string? _copyProblem;

    /// <param name="catalog">The records as they are now.</param>
    /// <param name="listFiles">
    /// The readable images in the field-sprite folder, in the order the list shows them.
    /// </param>
    /// <param name="sizeOf">An image's size by file name; null when the folder lacks it.</param>
    public FieldSpriteCatalogViewModel(
        CatalogDraft catalog, Func<IReadOnlyList<string>> listFiles, Func<string, (int Width, int Height)?> sizeOf)
    {
        _catalog = catalog;
        _listFiles = listFiles;
        _sizeOf = sizeOf;
        Choices = BuildChoices();
    }

    [ObservableProperty]
    public partial IReadOnlyList<FieldSpriteChoice> Choices { get; private set; }

    /// <summary>The chosen row; its setup opens, or stays open with its edits when chosen again.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Setup), nameof(CopySources), nameof(Problem))]
    public partial FieldSpriteChoice? Chosen { get; set; }

    /// <summary>The chosen image's setup; null with nothing chosen or an image the folder lacks.</summary>
    public FieldSpriteSetupViewModel? Setup => Chosen is { IsImageMissing: false } chosen ? OpenedSetup(chosen.File) : null;

    /// <summary>The other set-up images a setup may be copied from (decision 570).</summary>
    public IReadOnlyList<string> CopySources =>
        [.. _catalog.Current.FieldSprites.Keys.Where(file => file != Chosen?.File).Order(StringComparer.Ordinal)];

    /// <summary>
    /// What stops OK: a refused copy, then the chosen setup's problem once edited, then the first problem among the
    /// other edited setups, named by file.
    /// </summary>
    public string? Problem
    {
        get
        {
            if (_copyProblem is not null)
            {
                return _copyProblem;
            }

            if (Setup is { } own && _edited.Contains(own.File) && own.Problem is not null)
            {
                return own.Problem;
            }

            var other = EditedSetups().FirstOrDefault(setup => setup != Setup && setup.Problem is not null);
            return other is null ? null : $"{other.File}: {other.Problem}";
        }
    }

    public string? ChosenKey => Chosen?.File;

    public void Choose(string key) => Chosen = Choices.FirstOrDefault(choice => choice.File == key) ?? Chosen;

    /// <summary>
    /// Copies another image's layout onto the chosen one (decision 570): its frame size as this image's columns and
    /// rows, its scale, feet and walk. An image the frame size doesn't split into takes nothing, and the refusal shows
    /// until the next edit.
    /// </summary>
    /// <param name="file">One of <see cref="CopySources"/>.</param>
    public void CopySetupFrom(string file)
    {
        if (Setup is not { } setup || _sizeOf(setup.File) is not { } size)
        {
            return;
        }

        var source = _catalog.Current.FieldSprites[file];
        if (size.Width % source.FrameWidth != 0 || size.Height % source.FrameHeight != 0)
        {
            _copyProblem =
                $"{size.Width} x {size.Height} px doesn't split into {source.FrameWidth} x {source.FrameHeight} px frames.";
            OnPropertyChanged(nameof(Problem));
            return;
        }

        setup.Columns = size.Width / source.FrameWidth;
        setup.Rows = size.Height / source.FrameHeight;
        setup.Scale = source.Scale;
        setup.FeetPixels = source.FeetPixels;
        setup.Walk.CopyFrom(source.Walk);
    }

    /// <summary>The edited records that differ from the catalog's, for <c>CatalogDraft.SetFieldSprites</c>.</summary>
    /// <exception cref="InvalidOperationException">
    /// A setup has a problem; OK is greyed by <see cref="Problem"/>.
    /// </exception>
    public IReadOnlyList<FieldSpriteSheet> ToSheets() =>
        [.. EditedSetups()
            .Select(setup => setup.ToSheet())
            .Where(sheet => !_catalog.Current.FieldSprites.TryGetValue(sheet.File, out var current) || current != sheet)];

    /// <summary>
    /// Lists the folder again after an import: the last image copied is chosen; a copied image's setup opens anew,
    /// since its size may have changed, and counts as edited while its guess has no problem, so OK keeps a right guess
    /// untouched (decision 594: an imported merchant saved nothing); a wrong guess shows its problem in the setup but
    /// greys nothing until edited, so it never traps OK; the others stay.
    /// </summary>
    /// <param name="imported">The files the import copied, in order; empty keeps the choice.</param>
    public void RefreshAfterImport(IReadOnlyList<string> imported)
    {
        foreach (var file in imported)
        {
            _opened.Remove(file);
            _edited.Remove(file);
            if (OpenedSetup(file) is { Problem: null })
            {
                _edited.Add(file);
            }
        }

        var chosenFile = imported.Count > 0 ? imported[^1] : Chosen?.File;
        Choices = BuildChoices();
        Chosen = Choices.FirstOrDefault(choice => choice.File == chosenFile);
    }

    // An imported image counts before anything reads its setup, so each counted file's setup is opened here.
    private List<FieldSpriteSetupViewModel> EditedSetups() =>
        [.. _edited.Order(StringComparer.Ordinal).Select(OpenedSetup).OfType<FieldSpriteSetupViewModel>()];

    private FieldSpriteSetupViewModel? OpenedSetup(string file)
    {
        if (_opened.TryGetValue(file, out var opened))
        {
            return opened;
        }

        if (_sizeOf(file) is not { } size)
        {
            return null;
        }

        var setup = _catalog.Current.FieldSprites.TryGetValue(file, out var sheet)
            ? FieldSpriteSetupViewModel.From(sheet, size, _catalog)
            : FieldSpriteSetupViewModel.ForNew(file, size, _catalog);
        setup.PropertyChanged += OnSetupChanged;
        _opened.Add(file, setup);
        return setup;
    }

    // Any change to a setup makes it count, ends a refused copy's line and feeds the page's problem.
    private void OnSetupChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is FieldSpriteSetupViewModel setup)
        {
            _edited.Add(setup.File);
        }

        _copyProblem = null;
        OnPropertyChanged(nameof(Problem));
    }

    // The folder's images in its order, then the records whose image is missing, by file.
    private List<FieldSpriteChoice> BuildChoices()
    {
        var files = _listFiles();
        var listed = files.ToHashSet(StringComparer.Ordinal);
        var records = _catalog.Current.FieldSprites.Keys;
        var withoutImages = records.Where(file => !listed.Contains(file)).Order(StringComparer.Ordinal);
        return
        [
            .. files.Select(file => new FieldSpriteChoice(file, records.Contains(file))),
            .. withoutImages.Select(file => new FieldSpriteChoice(file, IsSetUp: true, IsImageMissing: true)),
        ];
    }
}
