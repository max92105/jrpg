using System.Windows;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Shell;

/// <summary>
/// NPCs > Character sprites...: the window edits the draft's sprite records, and OK applies the whole session as one
/// undo step (decision 569); its Import... copies images through the view model, whose import event drops the replaced
/// pictures as the tilesheet import's does.
/// </summary>
internal sealed partial class MainWindow
{
    private void OnCharacterSpritesClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.FieldSpriteEditing is not { } editing || _viewModel.AnimationTime is not { } animationTime)
        {
            return;
        }

        var edited = editing.Start();
        var window = new FieldSpriteWindow(edited, _viewModel.ArtRoot, animationTime, _viewModel.ImportFieldSprites)
        {
            Owner = this,
        };
        DialogBounds.FitTo(window, this);
        if (window.ShowDialog() == true)
        {
            editing.Apply(edited);
        }
    }
}
