using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Jrpg.Data.Art;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Cinematics;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's dispatch: an entry's title runs its opener (decision 938). Shops..., Drop tables..., Effect
/// strips..., Enemies... and Cinematics...: each window edits its catalog of the draft, and OK applies the whole
/// session as one undo step (ADR 0045, decision 591), as the Quests window does.
/// </summary>
internal sealed partial class MainWindow
{
    private Dictionary<string, Action> CreateDatabaseOpeners() => new()
    {
        ["Quests"] = OpenQuests,
        ["Shops"] = OpenShops,
        ["Drop tables"] = () => _ = EditDropTables(this, ""),
        ["Effect strips"] = OpenEffectStrips,
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

    private void OpenShops()
    {
        if (_viewModel.ShopEditing is not { } shopEditing)
        {
            return;
        }

        var edited = shopEditing.Start();
        if (new ShopCatalogWindow(edited) { Owner = this }.ShowDialog() == true)
        {
            shopEditing.Apply(edited);
        }
    }

    // The Drop tables window over the owner, on the chosen table; its OK applies its own undo step at once, so a window
    // under it can take the new tables (decision 602). Returns whether it applied.
    private bool EditDropTables(Window owner, string chosenTableId)
    {
        if (_viewModel.DropTableEditing is not { } dropTableEditing)
        {
            return false;
        }

        var edited = dropTableEditing.Start();
        edited.ChooseTable(chosenTableId);
        if (new DropTableCatalogWindow(edited) { Owner = owner }.ShowDialog() != true)
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

        var edited = enemyEditing.Start();
        var window = new EnemyCatalogWindow(
            edited,
            new EnemyArtSources(
                _viewModel.ImportArtImages,
                file => SpriteImageFile.LoadFrom(_viewModel.ArtRoot, ArtFolders.Battlers, file),
                animationTime),
            (owner, tableId) =>
            {
                if (EditDropTables(owner, tableId))
                {
                    enemyEditing.RefreshDropTables(edited);
                }
            })
        {
            Owner = this,
        };
        if (window.ShowDialog() == true)
        {
            enemyEditing.Apply(edited);
        }
    }

    // The preview decodes each still at the preview's width, a sixteenth of the pixels of a 1920 px picture.
    private void OpenCinematics()
    {
        if (_viewModel.CinematicEditing is not { } cinematicEditing)
        {
            return;
        }

        var edited = cinematicEditing.Start();
        var artRoot = _viewModel.ArtRoot;
        var window = new CinematicCatalogWindow(edited, LoadPicture) { Owner = this };
        if (window.ShowDialog() == true)
        {
            cinematicEditing.Apply(edited);
        }

        BitmapSource? LoadPicture(string file) =>
            SpriteImageFile.LoadAtWidth(artRoot, ArtFolders.Cinematics, file, (int)CinematicCatalogWindow.PreviewWidth);
    }

    private void OpenEffectStrips()
    {
        if (_viewModel.EffectStripEditing is not { } effectStripEditing || _viewModel.AnimationTime is not { } animationTime)
        {
            return;
        }

        var edited = effectStripEditing.Start();
        var artRoot = _viewModel.ArtRoot;
        var window = new EffectStripCatalogWindow(
            edited, file => SpriteImageFile.LoadFrom(artRoot, ArtFolders.Effects, file), animationTime)
        { Owner = this };
        if (window.ShowDialog() == true)
        {
            effectStripEditing.Apply(edited);
        }
    }
}
