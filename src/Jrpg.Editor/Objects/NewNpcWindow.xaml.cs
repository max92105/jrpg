using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The New NPC dialog. Create hands the id and sprite to the window's create step, which returns false when no NPC was
/// made (a field has a problem), and the dialog then stays open.
/// </summary>
internal sealed partial class NewNpcWindow : Window
{
    private readonly NewNpcViewModel _newNpc;
    private readonly Func<NewNpcViewModel, bool> _create;
    private readonly Func<ObjectDefinition, ImageSource> _draw;

    /// <param name="newNpc">The id and sprite, edited here.</param>
    /// <param name="create">Makes the NPC; false when it didn't.</param>
    /// <param name="draw">Draws an object as the map does, for the sprite's preview.</param>
    public NewNpcWindow(NewNpcViewModel newNpc, Func<NewNpcViewModel, bool> create, Func<ObjectDefinition, ImageSource> draw)
    {
        InitializeComponent();
        DataContext = _newNpc = newNpc;
        _create = create;
        _draw = draw;
        newNpc.PropertyChanged += OnNewNpcChanged;
        ShowPreview();
        Loaded += (_, _) => IdBox.Focus();
    }

    private void OnNewNpcChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NewNpcViewModel.SpriteFile))
        {
            ShowPreview();
        }
    }

    // Any PNG in the folder is listed, read or not: one that can't be read shows a line, not a crash.
    private void ShowPreview()
    {
        PreviewProblem.Text = "";
        try
        {
            Preview.Source = _newNpc.SpriteProblem is null ? _draw(_newNpc.ToDefinition()) : null;
        }
        catch (Exception exception) when (exception is NotSupportedException or FileFormatException or IOException)
        {
            Preview.Source = null;
            PreviewProblem.Text = "Can't read this image.";
        }
    }

    private void OnCreateClicked(object sender, RoutedEventArgs e)
    {
        if (_create(_newNpc))
        {
            DialogResult = true;
        }
    }
}
