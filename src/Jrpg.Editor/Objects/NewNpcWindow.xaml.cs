using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Hosting;
using Jrpg.Editor.Sprites;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The New NPC and Change NPC sprite dialog. OK hands the id and sprite to the window's apply step, which returns false
/// when nothing was made (a field has a problem), and the dialog then stays open. The chosen sprite walks its four
/// facings at the editor's animation time, as the Character sprites window shows it.
/// </summary>
internal sealed partial class NewNpcWindow : Window
{
    private readonly RenderTick _renderTick;
    private readonly NewNpcViewModel _npc;
    private readonly Func<NewNpcViewModel, bool> _apply;
    private readonly string _artRoot;
    private readonly TileAnimationTime _animationTime;
    private readonly WalkPreview _preview = new();
    private BitmapSource? _sheet;

    /// <param name="npc">The id and sprite, edited here.</param>
    /// <param name="apply">Makes or changes the NPC; false when it didn't.</param>
    /// <param name="artRoot">The <c>art</c> folder, where the sprite image is read from.</param>
    /// <param name="animationTime">The editor's animation time, which the preview walks on.</param>
    public NewNpcWindow(NewNpcViewModel npc, Func<NewNpcViewModel, bool> apply, string artRoot, TileAnimationTime animationTime)
    {
        InitializeComponent();
        _renderTick = new RenderTick(OnRendering);
        DataContext = _npc = npc;
        (_apply, _artRoot, _animationTime) = (apply, artRoot, animationTime);
        Title = npc.IsIdFixed ? "Change NPC sprite" : "New NPC";
        OkButton.Content = npc.IsIdFixed ? "Change" : "Create";
        npc.PropertyChanged += OnNpcChanged;
        _renderTick.Start();
        Closed += OnClosed;
        ShowPreview();
        Loaded += (_, _) => ((UIElement)(npc.IsIdFixed ? SpriteBox : IdBox)).Focus();
    }

    private void OnNpcChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NewNpcViewModel.SpriteFile))
        {
            ShowPreview();
        }
    }

    private void ShowPreview()
    {
        _sheet = _npc.ChosenSheet is null ? null : SpriteImageFile.Load(_artRoot, _npc.SpriteFile);
        PreviewProblem.Text = _npc.ChosenSheet is not null && _sheet is null ? "The image is missing from the folder." : "";
        DrawPreview();
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (_npc.ChosenSheet is { } sheet && _sheet is not null && _preview.HasMovedOn(sheet, _animationTime.Elapsed))
        {
            DrawPreview();
        }
    }

    private void DrawPreview() =>
        Preview.Source = _npc.ChosenSheet is { } sheet && _sheet is not null
            ? _preview.Draw(sheet, _sheet, _animationTime.Elapsed)
            : null;

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_apply(_npc))
        {
            DialogResult = true;
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _renderTick.Stop();
        _npc.PropertyChanged -= OnNpcChanged;
    }
}
