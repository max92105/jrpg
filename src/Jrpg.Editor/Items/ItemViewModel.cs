using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Items;

/// <summary>
/// One item as the Items page edits it (decision 879): its id (typed while the item is new, fixed once the draft holds
/// it, decision 582), name, description, icon, max stack, price, menu use and own strip as typed, and its category's
/// form, an equipment's (decision 880) or a consumable's effect (decision 881), written over the record it came from,
/// so what no box edits (the fixed affixes) is carried; its category is the record's and never typed (decision 878).
/// <see cref="Problem"/> is only the typed id's, the number boxes' and the form's (a box that can't be read, a stat
/// named twice): the rest is Data's rules, which the list shows for every item at once.
/// </summary>
internal sealed partial class ItemViewModel : ObservableObject
{
    private readonly ItemDefinition _source;

    // rules.json's one sale share (decision 864), which the sale line reads: the page doesn't edit it.
    private readonly int _sellPercent;

    private readonly ItemForms _forms;

    // The boxes start on the source's fields: a new item's id is empty.
    private ItemViewModel(ItemDefinition source, int sellPercent, ItemForms forms)
    {
        _source = source;
        _sellPercent = sellPercent;
        _forms = forms;
        if (forms.Equipment is { } equipment)
        {
            equipment.Changed += OnFormChanged;
        }

        if (forms.Effect is { } effect)
        {
            effect.Changed += OnFormChanged;
        }

        Category = ItemCategories.Of(source);
        Id = source.Id;
        Name = source.Name;
        Description = source.Description;
        IconFile = source.IconFile;
        MaxStackText = TypedNumber.Text(source.MaxStack);
        PriceText = source.Price is { } price ? TypedNumber.Text(price) : "";
        IsUsableFromMenu = source.IsUsableFromMenu;
        EffectFile = source.EffectFile ?? "";
    }

    /// <summary>
    /// A typed box changed, the form's included: what the list's problem line listens to, never the label's, the sale
    /// line's or the problem's own notices, so the list runs Data's check once per change.
    /// </summary>
    public event EventHandler? Changed;

    /// <param name="item">The record as the draft holds it.</param>
    /// <param name="sellPercent">The share of a price a sale pays (<c>rules.json</c>, decision 864).</param>
    public static ItemViewModel From(ItemDefinition item, int sellPercent) => new(item, sellPercent, ItemForms.Of(item));

    /// <summary>
    /// An item added this session: its id left to type, the rest as its category's starting record has it.
    /// </summary>
    /// <param name="startingRecord">The record whose fields the new item starts with; its id is not used.</param>
    /// <param name="sellPercent">The share of a price a sale pays (<c>rules.json</c>, decision 864).</param>
    public static ItemViewModel New(ItemDefinition startingRecord, int sellPercent) =>
        new(startingRecord with { Id = "" }, sellPercent, ItemForms.Of(startingRecord)) { IsNew = true };

    /// <summary>
    /// True while the item was added this session: the only time its id can be typed (decision 582).
    /// </summary>
    public bool IsNew { get; private init; }

    /// <summary>The page shows it but never changes it: another category is another item (decision 878).</summary>
    public ItemCategory Category { get; }

    /// <summary>
    /// The equipment's form (decision 880): an equipment item's or a unique's, null for anything else.
    /// </summary>
    public ItemEquipmentViewModel? Equipment => _forms.Equipment;

    /// <summary>The effect's form (decision 881): a consumable's, null for anything else.</summary>
    public ItemEffectViewModel? Effect => _forms.Effect;

    /// <summary>The id as typed; the page lets it be typed only while <see cref="IsNew"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label), nameof(Problem))]
    public partial string Id { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Label))]
    public partial string Name { get; set; }

    /// <summary>Shown while choosing the item; it may be empty.</summary>
    [ObservableProperty]
    public partial string Description { get; set; }

    /// <summary>The image in the icons folder.</summary>
    [ObservableProperty]
    public partial string IconFile { get; set; }

    /// <summary>The most copies the inventory holds, as typed; equipment and uniques keep their record's.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string MaxStackText { get; set; }

    /// <summary>The buying price as typed; empty when the item is never bought nor sold (ADR 0031).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SaleLine), nameof(Problem))]
    public partial string PriceText { get; set; }

    /// <summary>Whether the menu's Items screen offers it; only a consumable's box is shown.</summary>
    [ObservableProperty]
    public partial bool IsUsableFromMenu { get; set; }

    /// <summary>
    /// The strip in the effects folder it plays in battle instead of its user's attack strip (decision 678); empty when
    /// it plays its user's. Data takes one only on a damage (decision 707).
    /// </summary>
    [ObservableProperty]
    public partial string EffectFile { get; set; }

    /// <summary>
    /// The list's line: "Unique · Chief's Cleaver (chiefs_cleaver)", "(new unique)" while it has no id.
    /// </summary>
    public string Label => Id.Length > 0 ? $"{Category} · {Name} ({Id})" : $"(new {Category.ToString().ToLowerInvariant()})";

    /// <summary>
    /// "Sells for 50 (50 % share)" through Core's sale formula, "Never bought nor sold" with the price box empty; empty
    /// while the price box isn't a whole number.
    /// </summary>
    public string SaleLine =>
        IsPriceEmpty ? "Never bought nor sold"
        : TypedNumber.Parse(PriceText) is { } price ? SellsFor(price)
        : "";

    /// <summary>Equipment and uniques stack to their record's max (99, decision 879), so the box is hidden.</summary>
    public bool ShowsMaxStack => Category is ItemCategory.Consumable or ItemCategory.Material;

    public bool ShowsUsableFromMenu => Category == ItemCategory.Consumable;

    /// <summary>
    /// Why the item can't be built: the typed id while new, then the max stack box, then the price box, then the
    /// category's form; null when it can. Data's rules (a name, the ranges) are the list's.
    /// </summary>
    public string? Problem =>
        (IsNew ? ObjectIds.ProblemOf(Id, []) : null)
        ?? TypedNumber.ProblemOf("Max stack", MaxStackText)
        ?? PriceProblem
        ?? Equipment?.Problem
        ?? Effect?.Problem;

    private bool IsPriceEmpty => PriceText.Trim().Length == 0;

    private string? PriceProblem =>
        IsPriceEmpty || TypedNumber.Parse(PriceText) is not null
            ? null
            : $"Price must be a whole number or empty (was '{PriceText.Trim()}').";

    private string SellsFor(int price) => $"Sells for {PriceFormula.SalePrice(price, _sellPercent)} ({_sellPercent} % share)";

    /// <summary>
    /// A new item copying this one's boxes as typed, a half-typed number included, its category too, its id left to
    /// type (decision 585).
    /// </summary>
    public ItemViewModel Duplicate() =>
        new(TypedRecord() with { Id = "" }, _sellPercent, _forms.CopyAsTyped())
        {
            IsNew = true,
            MaxStackText = MaxStackText,
            PriceText = PriceText,
        };

    /// <summary>
    /// The record it came from with the typed fields over it: the name trimmed, no price for an empty box, the
    /// equipment and the effect as their forms make them.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is a <see cref="Problem"/>.</exception>
    public ItemDefinition ToItem() =>
        Problem is { } problem
            ? throw new InvalidOperationException(problem)
            : TypedRecord() with
            {
                MaxStack = TypedNumber.Parse(MaxStackText)!.Value,
                Price = IsPriceEmpty ? null : TypedNumber.Parse(PriceText),
                Equipment = Equipment?.ToEquipment(),
                Effect = Effect?.ToEffect(),
            };

    // The number boxes and the forms are left out: a duplicate copies them as typed, and only ToItem reads the number
    // each of them holds.
    private ItemDefinition TypedRecord() =>
        _source with
        {
            Id = Id,
            Name = Name.Trim(),
            Description = Description,
            IconFile = IconFile,
            IsUsableFromMenu = IsUsableFromMenu,
            EffectFile = EffectFile.Trim() is { Length: > 0 } strip ? strip : null,
        };

    private void OnFormChanged(object? sender, EventArgs e) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIdChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnNameChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnDescriptionChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIconFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnMaxStackTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnPriceTextChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnIsUsableFromMenuChanged(bool value) => Changed?.Invoke(this, EventArgs.Empty);

    partial void OnEffectFileChanged(string value) => Changed?.Invoke(this, EventArgs.Empty);
}
