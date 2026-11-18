using Jrpg.Data.Art;
using Jrpg.Editor.Animation;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Encounters;
using Jrpg.Editor.Items;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Party;
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
/// Edit... shows the Drop tables page on the enemy's table, the Enemies page applied first (decision 928).
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
        pageViews.Register<ClassCatalogViewModel>(edited => new ClassCatalogPage(edited));
        RegisterPicturePages(pageViews);
        RegisterPagesOnTheAnimationClock(pageViews, animationTime);
        return pageViews;
    }

    // Items, Cinematics and Party members decode each picture at the size the page shows it: the icons and the 1920 px
    // stills are far bigger, a member's portrait and battle sprite at the 160 px of its preview. Effect strips and
    // Battle backgrounds decode whole: their frames are cut from the image at its own size.
    private void RegisterPicturePages(DatabasePageViews pageViews)
    {
        var artRoot = _viewModel.ArtRoot;
        pageViews.Register<PartyMemberCatalogViewModel>(edited => new PartyMemberCatalogPage(
            edited,
            _viewModel.ImportArtImages,
            (folder, file) => SpriteImageFile.LoadAtWidth(artRoot, folder, file, (int)PartyMemberCatalogPage.PreviewWidth)));
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
        var art = new EnemyArtSources(
            _viewModel.ImportArtImages,
            file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Battlers, file),
            animationTime);
        pageViews.Register<EnemyCatalogViewModel>(edited => new EnemyCatalogPage(
            edited,
            art,
            tableId => _viewModel.ShowDatabasePageOnRecord("Drop tables", tableId)));
        pageViews.Register<FieldSpriteCatalogViewModel>(edited => new FieldSpritePage(
            edited, artRoot, animationTime, _viewModel.ImportFieldSprites));
    }
}
