using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Objects > Change NPC sprite...: the New NPC dialog on the Objects panel's chosen NPC, its id fixed, and OK swapping
/// its sprite as one undo step (ADR 0015's one-field change per NPC, now a click). Greyed unless the chosen object is
/// an NPC; re-checked when the choice or the catalog changes (an undo may remove the NPC).
/// </summary>
internal sealed partial class NpcSpriteChange : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly ObjectBrowserViewModel _browser;

    /// <param name="catalog">The objects and the set-up sheets.</param>
    /// <param name="browser">Whose chosen object is changed.</param>
    public NpcSpriteChange(CatalogDraft catalog, ObjectBrowserViewModel browser)
    {
        (_catalog, _browser) = (catalog, browser);
        browser.PropertyChanged += OnChanged;
        catalog.PropertyChanged += OnChanged;
    }

    /// <summary>Whether the chosen object is an NPC the catalog still holds.</summary>
    public bool CanChange =>
        _browser.SelectedObject is { FieldSpriteFile: not null } chosen && _catalog.Current.Objects.ContainsKey(chosen.Id);

    /// <summary>The dialog's state on the chosen NPC; null when <see cref="CanChange"/> is false.</summary>
    public NewNpcViewModel? Start() =>
        CanChange ? NewNpcViewModel.ForChange(ChosenNpc(), [.. _catalog.Current.FieldSprites.Values]) : null;

    /// <summary>Gives the NPC its new sprite as one undo step; nothing when it is the same.</summary>
    /// <returns>False when the sprite has a problem, so the dialog stays open.</returns>
    public bool Apply(NewNpcViewModel dialog)
    {
        if (dialog.SpriteProblem is not null)
        {
            return false;
        }

        _catalog.SetObject(dialog.ToDefinition());
        return true;
    }


    // The catalog's copy, which an undo may have changed since the browser chose it.
    private ObjectDefinition ChosenNpc() => _catalog.Current.Objects[_browser.SelectedObject!.Id];

    private void OnChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ObjectBrowserViewModel.SelectedObject) or nameof(CatalogDraft.Current))
        {
            OnPropertyChanged(nameof(CanChange));
        }
    }
}
