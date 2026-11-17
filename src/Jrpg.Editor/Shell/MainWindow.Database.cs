using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Items;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Quests;
using Jrpg.Editor.Skills;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Trade;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's dispatch: an entry's title runs its opener (decision 938), which edits its catalog of the draft
/// at the main window's size (decision 930); OK applies the whole session as one undo step (ADR 0045, decision 591).
/// Each entry opens a page in the catalog dialog, as NPCs > Character sprites... does from outside the menu.
/// </summary>
internal sealed partial class MainWindow
{
    private Dictionary<string, Action> CreateDatabaseOpeners() => new()
    {
        ["Quests"] = OpenQuests,
        ["Shops"] = OpenShops,
        ["Items"] = OpenItems,
        ["Drop tables"] = () => _ = EditDropTables(this, ""),
        ["Affixes"] = OpenAffixes,
        ["Effect strips"] = OpenEffectStrips,
        ["Battle backgrounds"] = OpenBattleBackgrounds,
        ["Skills"] = OpenSkills,
        ["Effects"] = OpenTieredEffects,
        ["Statuses"] = OpenStatuses,
        ["Cinematics"] = OpenCinematics,
        ["Enemies"] = OpenEnemies,
    };

    // Only an entry raises Click, never the Database header. An entry without a known title is a typo in
    // DatabaseMenu.xaml: thrown, so the smoke script shows it as an ERROR.
    private void OnDatabaseMenuClicked(object sender, RoutedEventArgs e)
    {
        var entry = (MenuItem)e.OriginalSource;
        if (entry.Tag is not string title || !_databaseOpeners.TryGetValue(title, out var open))
        {
            throw new InvalidOperationException($"The Database menu's '{entry.Header}' entry has no known title.");
        }

        open();
    }

    private void OpenQuests()
    {
        if (_viewModel.QuestEditing is not { } questEditing)
        {
            return;
        }

        OpenInDialog("Quests", questEditing.Start, questEditing.Apply, edited => new QuestCatalogPage(edited));
    }

    private void OpenShops()
    {
        if (_viewModel.ShopEditing is not { } shopEditing)
        {
            return;
        }

        OpenInDialog("Shops", shopEditing.Start, shopEditing.Apply, edited => new ShopCatalogPage(edited));
    }

    // The icon is decoded at the size the page shows it: the icons folder's images are far bigger.
    private void OpenItems()
    {
        if (_viewModel.ItemEditing is not { } itemEditing)
        {
            return;
        }

        var artRoot = _viewModel.ArtRoot;
        OpenInDialog("Items", itemEditing.Start, itemEditing.Apply, edited => new ItemCatalogPage(edited, LoadIcon));

        BitmapSource? LoadIcon(string file) =>
            SpriteImageFile.LoadAtWidth(artRoot, ArtFolders.Icons, file, (int)ItemCatalogPage.IconPictureSize);
    }

    private void OpenAffixes()
    {
        if (_viewModel.AffixEditing is not { } affixEditing)
        {
            return;
        }

        OpenInDialog("Affixes", affixEditing.Start, affixEditing.Apply, edited => new AffixCatalogPage(edited));
    }

    // Starts the page's session, shows it in the dialog over the main window, applies it on OK.
    private void OpenInDialog<TEdited>(
        string title, Func<TEdited> start, Action<TEdited> apply, Func<TEdited, FrameworkElement> buildPage)
        where TEdited : IEditedCatalog
    {
        var edited = start();
        var dialog = new CatalogDialog(title, buildPage(edited), edited) { Owner = this };
        DialogBounds.FitTo(dialog, this);
        if (dialog.ShowDialog() == true)
        {
            apply(edited);
        }
    }

    // The Drop tables page in the catalog dialog over the owner, on the chosen table; its OK applies its own undo step
    // at once, so a page under it can take the new tables (decision 602). Returns whether it applied.
    private bool EditDropTables(Window owner, string chosenTableId)
    {
        if (_viewModel.DropTableEditing is not { } dropTableEditing)
        {
            return false;
        }

        var edited = dropTableEditing.Start();
        edited.Choose(chosenTableId);
        var dialog = new CatalogDialog("Drop tables", new DropTableCatalogPage(edited), edited) { Owner = owner };
        DialogBounds.FitTo(dialog, this);
        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        dropTableEditing.Apply(edited);
        return true;
    }

    private void OpenEnemies()
    {
        if (_viewModel.EnemyEditing is not { } enemyEditing || _viewModel.AnimationTime is not { } animationTime)
        {
            return;
        }

        var art = new EnemyArtSources(
            _viewModel.ImportArtImages,
            file => SpriteImageFile.LoadFrom(_viewModel.ArtRoot, ArtFolders.Battlers, file),
            animationTime);
        OpenInDialog("Enemies", enemyEditing.Start, enemyEditing.Apply, edited => new EnemyCatalogPage(
            edited,
            art,
            (owner, tableId) =>
            {
                if (EditDropTables(owner, tableId))
                {
                    enemyEditing.RefreshDropTables(edited);
                }
            }));
    }

    // The preview decodes each still at the preview's width, a sixteenth of the pixels of a 1920 px picture.
    private void OpenCinematics()
    {
        if (_viewModel.CinematicEditing is not { } cinematicEditing)
        {
            return;
        }

        var artRoot = _viewModel.ArtRoot;
        OpenInDialog(
            "Cinematics",
            cinematicEditing.Start,
            cinematicEditing.Apply,
            edited => new CinematicCatalogPage(edited, LoadPicture));

        BitmapSource? LoadPicture(string file) =>
            SpriteImageFile.LoadAtWidth(artRoot, ArtFolders.Cinematics, file, (int)CinematicCatalogPage.PreviewWidth);
    }

    private void OpenEffectStrips()
    {
        if (_viewModel.EffectStripEditing is not { } effectStripEditing)
        {
            return;
        }

        var artRoot = _viewModel.ArtRoot;
        OpenInDialog("Effect strips", effectStripEditing.Start, effectStripEditing.Apply, edited => new EffectStripCatalogPage(
            edited,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Effects, file),
            (paths, options) => _viewModel.ImportArtImages(ArtFolders.Effects, paths, options),
            owner =>
            {
                var tool = new StripFromCellsWindow(_viewModel.ImportCellStrip) { Owner = owner };
                return tool.ShowDialog() == true ? tool.Written : null;
            }));
    }

    // Decoded whole: an animated background's frames are cut from its sheet at the sheet's own size.
    private void OpenBattleBackgrounds()
    {
        if (_viewModel.BattleBackgroundEditing is not { } backgroundEditing)
        {
            return;
        }

        var artRoot = _viewModel.ArtRoot;
        OpenInDialog("Battle backgrounds", backgroundEditing.Start, backgroundEditing.Apply, edited => new BattleBackgroundPage(
            edited,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Backgrounds, file),
            (paths, options) => _viewModel.ImportArtImages(ArtFolders.Backgrounds, paths, options)));
    }

    private void OpenSkills()
    {
        if (_viewModel.SkillEditing is not { } skillEditing)
        {
            return;
        }

        OpenInDialog("Skills", skillEditing.Start, skillEditing.Apply, edited => new SkillCatalogPage(edited));
    }

    private void OpenTieredEffects()
    {
        if (_viewModel.TieredEffectEditing is not { } effectEditing)
        {
            return;
        }

        OpenInDialog("Effects", effectEditing.Start, effectEditing.Apply, edited => new TieredEffectCatalogPage(edited));
    }

    private void OpenStatuses()
    {
        if (_viewModel.StatusEditing is not { } statusEditing)
        {
            return;
        }

        OpenInDialog("Statuses", statusEditing.Start, statusEditing.Apply, edited => new StatusCatalogPage(edited));
    }

    // The sprite records (decision 569); the page's Import... copies images through the view model, whose import event
    // drops the replaced pictures as the tilesheet import's does.
    private void OnCharacterSpritesClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.FieldSpriteEditing is not { } fieldSpriteEditing || _viewModel.AnimationTime is not { } animationTime)
        {
            return;
        }

        var artRoot = _viewModel.ArtRoot;
        OpenInDialog("Character sprites", fieldSpriteEditing.Start, fieldSpriteEditing.Apply, edited => new FieldSpritePage(
            edited, artRoot, animationTime, _viewModel.ImportFieldSprites));
    }
}
