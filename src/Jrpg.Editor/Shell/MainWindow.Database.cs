using System.Windows;
using Jrpg.Data.Art;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Loot;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// Database > Shops..., Drop tables... and Enemies...: each window edits its catalog of the draft, and OK applies the
/// whole session as one undo step (ADR 0045, decision 591), as the Quests window does.
/// </summary>
internal sealed partial class MainWindow
{
    private void OnEditShopsClicked(object sender, RoutedEventArgs e)
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

    private void OnEditDropTablesClicked(object sender, RoutedEventArgs e) => _ = EditDropTables(this, "");

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

    private void OnEditEnemiesClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.EnemyEditing is not { } enemyEditing)
        {
            return;
        }

        var edited = enemyEditing.Start();
        var window = new EnemyCatalogWindow(
            edited,
            _viewModel.ImportArtImages,
            file => SpriteImageFile.LoadFrom(_viewModel.ArtRoot, ArtFolders.Battlers, file),
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
}
