using System.Windows;
using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Encounters;
using Jrpg.Editor.Items;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Quests;
using Jrpg.Editor.Skills;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Trade;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database pages' views (decision 918): each catalog's page built over its edited catalog with the callbacks it
/// needs, registered for the Database mode, which the Database menu's entries show (decision 926). The Enemies page's
/// Edit... still opens the Drop tables page in the catalog dialog over the main window, applying its own undo step.
/// </summary>
internal sealed partial class MainWindow
{
    // Nothing to host when the content didn't load: the Database mode then shows no page.
    private void HostDatabasePages()
    {
        if (_viewModel is { DatabaseWorkspace: { } workspace, AnimationTime: { } animationTime })
        {
            DatabaseView.Host(workspace, CreatePageViews(animationTime));
        }
    }

    // Called once the content loaded: every editing view model and the animation clock are there.
    private DatabasePageViews CreatePageViews(TileAnimationTime animationTime)
    {
        var pageViews = new DatabasePageViews();
        pageViews.Register<QuestCatalogViewModel>(edited => new QuestCatalogPage(edited));
        pageViews.Register<ShopCatalogViewModel>(edited => new ShopCatalogPage(edited));
        pageViews.Register<AffixCatalogViewModel>(edited => new AffixCatalogPage(edited));
        pageViews.Register<DropTableCatalogViewModel>(edited => new DropTableCatalogPage(edited));
        pageViews.Register<SkillCatalogViewModel>(edited => new SkillCatalogPage(edited));
        pageViews.Register<TieredEffectCatalogViewModel>(edited => new TieredEffectCatalogPage(edited));
        pageViews.Register<StatusCatalogViewModel>(edited => new StatusCatalogPage(edited));
        pageViews.Register<EncounterCatalogViewModel>(edited => new EncounterPage(edited));
        RegisterPicturePages(pageViews);
        RegisterPagesOnTheAnimationClock(pageViews, animationTime);
        return pageViews;
    }

    // Items and Cinematics decode each picture at the size the page shows it: the icons and the 1920 px stills are far
    // bigger. Effect strips and Battle backgrounds decode whole: their frames are cut from the image at its own size.
    private void RegisterPicturePages(DatabasePageViews pageViews)
    {
        var artRoot = _viewModel.ArtRoot;
        pageViews.Register<ItemCatalogViewModel>(edited => new ItemCatalogPage(
            edited,
            file => SpriteImageFile.LoadAtWidth(artRoot, ArtFolders.Icons, file, (int)ItemCatalogPage.IconPictureSize)));
        pageViews.Register<CinematicCatalogViewModel>(edited => new CinematicCatalogPage(
            edited,
            file => SpriteImageFile.LoadAtWidth(artRoot, ArtFolders.Cinematics, file, (int)CinematicCatalogPage.PreviewWidth)));
        pageViews.Register<EffectStripCatalogViewModel>(edited => new EffectStripCatalogPage(
            edited,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Effects, file),
            (paths, options) => _viewModel.ImportArtImages(ArtFolders.Effects, paths, options),
            owner =>
            {
                var tool = new StripFromCellsWindow(_viewModel.ImportCellStrip) { Owner = owner };
                return tool.ShowDialog() == true ? tool.Written : null;
            }));
        pageViews.Register<BattleBackgroundCatalogViewModel>(edited => new BattleBackgroundPage(
            edited,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Backgrounds, file),
            (paths, options) => _viewModel.ImportArtImages(ArtFolders.Backgrounds, paths, options)));
    }

    // The two pages that play on the editor's shared animation clock, as the map does: the Enemies page's idle loop,
    // and the Character sprites page's walk. Its Import... copies images through the view model, whose import event
    // drops the replaced pictures as the tilesheet import's does (decision 569).
    private void RegisterPagesOnTheAnimationClock(DatabasePageViews pageViews, TileAnimationTime animationTime)
    {
        var artRoot = _viewModel.ArtRoot;
        var enemyEditing = _viewModel.EnemyEditing!;
        var art = new EnemyArtSources(
            _viewModel.ImportArtImages,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Battlers, file),
            animationTime);
        pageViews.Register<EnemyCatalogViewModel>(edited => new EnemyCatalogPage(
            edited,
            art,
            (owner, tableId) =>
            {
                if (EditDropTables(owner, tableId))
                {
                    enemyEditing.RefreshDropTables(edited);
                }
            }));
        pageViews.Register<FieldSpriteCatalogViewModel>(edited => new FieldSpritePage(
            edited, artRoot, animationTime, _viewModel.ImportFieldSprites));
    }

    // A dialog's OK changes the draft, which would start the shown Database page again under what was typed: applied
    // first, or the dialog doesn't open and the Database mode shows why (decision 919).
    private bool TryApplyShownPage() => _viewModel.DatabaseWorkspace?.TryApplyShown() ?? true;

    // The Drop tables page in the catalog dialog over the owner, on the chosen table; its OK applies its own undo step
    // at once, so a page under it can take the new tables (decision 602). Returns whether it applied.
    private bool EditDropTables(Window owner, string chosenTableId)
    {
        if (_viewModel.DropTableEditing is not { } dropTableEditing || !TryApplyShownPage())
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
}
