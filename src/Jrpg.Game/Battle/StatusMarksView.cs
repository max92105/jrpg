using Jrpg.Core.Statuses;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant's statuses as a row of small diamond marks, in the order applied, like the reference UI's buff and
/// weakness icons: the status's first letter, its turns left small at the corner, and a rim, warm red for a harmful
/// status and cool blue for a helpful one. A new mark pops in; an ended one fades out. Text is formatted at refreshes.
/// </summary>
internal sealed class StatusMarksView
{
    // Placeholders on the 1920 x 1080 virtual screen (design.md, StatusMarkSize).
    public const int MarkSize = 22;
    private const int MarkGap = 4;
    private const int LetterPixels = 13;
    private const int TurnsPixels = 11;
    private const float RimInnerReach = 0.78f;
    private const float PopStartScale = 1.3f;

    private static readonly TimeSpan _popTime = TimeSpan.FromSeconds(0.2);
    private static readonly TimeSpan _fadeTime = TimeSpan.FromSeconds(0.2);
    private static readonly DiamondShape _fill = new(MarkSize, 0, 1, UiTheme.TurnBackdrop, UiTheme.TurnBackdrop);
    private static readonly DiamondShape _rim = new(MarkSize, RimInnerReach, 1, Color.White, Color.White);

    private readonly StatusSet _statuses;
    private readonly UiMessages _messages;
    private readonly UiTextures _ui;
    private readonly OutlinedFont _letterFont;
    private readonly OutlinedFont _turnsFont;
    private readonly List<StatusMark> _marks = [];

    /// <param name="statuses">The combatant's live statuses, read on every refresh (ADR 0013).</param>
    /// <param name="text">The status names and the marks' fonts.</param>
    /// <param name="ui">Where the mark's diamonds are baked, once.</param>
    public StatusMarksView(StatusSet statuses, ScreenText text, UiTextures ui)
    {
        _statuses = statuses;
        _messages = text.Messages;
        _ui = ui;
        _letterFont = new OutlinedFont(text.Fonts.FontAt(LetterPixels));
        _turnsFont = new OutlinedFont(text.Fonts.FontAt(TurnsPixels));
        Refresh();
    }

    /// <summary>The marks shown, fading ones included, in the order their statuses were applied.</summary>
    public IReadOnlyList<StatusMark> Marks => _marks;

    public int Width => _marks.Count == 0 ? 0 : (_marks.Count * (MarkSize + MarkGap)) - MarkGap;

    /// <returns>A mark's size while it pops in: 1.3 as it appears, down to 1 after the pop.</returns>
    public static float PopScale(TimeSpan sinceShown) => 1 + ((PopStartScale - 1) * (1 - Share(sinceShown, _popTime)));

    /// <returns>How visible an ended mark still is: 1 as its status ends, down to 0 after the fade.</returns>
    public static float FadeOpacity(TimeSpan sinceEnded) => 1 - Share(sinceEnded, _fadeTime);

    public static bool IsHarmful(StatusKind kind) => kind is StatusKind.Stun or StatusKind.Exposed or StatusKind.Poison;

    /// <summary>Adds a mark for each new status, writes changed turns, and starts the fade of each ended one.</summary>
    public void Refresh()
    {
        foreach (var mark in _marks.Where(mark => mark.SinceEnded is null && !_statuses.Has(mark.Kind)))
        {
            mark.SinceEnded = TimeSpan.Zero;
        }

        foreach (var status in _statuses.Active)
        {
            var mark = _marks.Find(shown => shown.Kind == status.Kind && shown.SinceEnded is null) ?? AddMark(status.Kind);
            if (mark.TurnsLeft != status.TurnsLeft)
            {
                mark.TurnsLeft = status.TurnsLeft;
                mark.Turns = MeasuredText.Of(_turnsFont.Font, NumberText.Of(status.TurnsLeft));
            }
        }
    }

    public void Update(TimeSpan delta)
    {
        foreach (var mark in _marks)
        {
            mark.SinceShown += delta;
            mark.SinceEnded += delta;
        }

        _marks.RemoveAll(mark => mark.SinceEnded >= _fadeTime);
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the baked diamonds are kept.</param>
    /// <param name="topLeft">The first mark's top left.</param>
    /// <param name="opacity">0 to 1, while the combatant's panel or tag is dimmed or fading.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Point topLeft, float opacity)
    {
        for (var index = 0; index < _marks.Count; index++)
        {
            var mark = _marks[index];
            var center = new Vector2(topLeft.X + (index * (MarkSize + MarkGap)) + (MarkSize / 2f), topLeft.Y + (MarkSize / 2f));
            var fade = opacity * (mark.SinceEnded is { } sinceEnded ? FadeOpacity(sinceEnded) : 1);
            var size = MarkSize * PopScale(mark.SinceShown);
            var area = new Rectangle((int)(center.X - (size / 2)), (int)(center.Y - (size / 2)), (int)size, (int)size);
            batch.Draw(_ui.Diamond(textures, _fill), area, Color.White * fade);
            batch.Draw(_ui.Diamond(textures, _rim), area, (IsHarmful(mark.Kind) ? UiTheme.StatusHarmfulRim : UiTheme.StatusHelpfulRim) * fade);
            _letterFont.DrawCentered(batch, mark.Letter, center, (UiTheme.TextPrimary * fade, PopScale(mark.SinceShown)));
            var turnsTopLeft = new Vector2(area.Right - (mark.Turns.Width / 2), area.Bottom - _turnsFont.Font.LineHeight);
            _turnsFont.Draw(batch, mark.Turns.Text, turnsTopLeft, UiTheme.TextPrimary * fade);
        }
    }

    private static float Share(TimeSpan elapsed, TimeSpan duration) => Math.Clamp((float)(elapsed / duration), 0, 1);

    private StatusMark AddMark(StatusKind kind)
    {
        var name = _messages.Text(StatusNames.IdOf(kind));
        var letter = name[..1].ToUpperInvariant();
        var mark = new StatusMark(kind, MeasuredText.Of(_letterFont.Font, letter)) { TurnsLeft = -1 };
        _marks.Add(mark);
        return mark;
    }
}
