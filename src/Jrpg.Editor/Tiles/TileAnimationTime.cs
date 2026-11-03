using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The editor's animation time (decision 470): the window moves it on from WPF's render tick, which stays view code,
/// and a frame comes from Core's <see cref="TileAnimation.FrameAt"/>. A picture is redrawn only when one of the
/// catalog's animated tiles shows another frame, not on every tick.
/// </summary>
internal sealed class TileAnimationTime
{
    private readonly CatalogDraft _catalog;

    // Equal animations show equal frames, so each is checked once; worked out per catalog change, not per tick.
    private IReadOnlyList<TileAnimation> _animations;

    /// <param name="catalog">Whose tiles' animations are followed.</param>
    public TileAnimationTime(CatalogDraft catalog)
    {
        _catalog = catalog;
        _animations = DistinctAnimations();
        catalog.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CatalogDraft.Current))
            {
                _animations = DistinctAnimations();
            }
        };
    }

    public TimeSpan Elapsed { get; private set; }

    /// <summary>Raised once per <see cref="Advance"/> in which some animated tile's frame changed.</summary>
    public event EventHandler? FramesChanged;

    /// <exception cref="ArgumentOutOfRangeException">A negative delta: the time only moves on.</exception>
    public void Advance(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        var before = Elapsed;
        Elapsed += delta;
        if (_animations.Any(animation => animation.FrameAt(before) != animation.FrameAt(Elapsed)))
        {
            FramesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private List<TileAnimation> DistinctAnimations() => [.. _catalog.Current.Tiles.Values.Select(tile => tile.Animation).OfType<TileAnimation>().Distinct()];
}
