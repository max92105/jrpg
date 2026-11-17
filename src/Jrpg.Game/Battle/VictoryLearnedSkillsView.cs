using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The victory screen's "Omer learned Meteor." lines (decision 964), centered under the members' cards in pale text,
/// each fading in once its member's bar has filled its level. Every line keeps its own row from the start, so one
/// fading in never moves another; with none learned, nothing is drawn.
/// </summary>
internal sealed class VictoryLearnedSkillsView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56), in the cards' body face.
    private const int LinePixels = 24;
    private const int GapUnderCards = 12;

    /// <summary>The distance from one line's top to the next one's.</summary>
    internal const int LinePitch = 34;

    private readonly VictoryScreen _screen;
    private readonly OutlinedFont _font;
    private readonly IReadOnlyList<LearnedSkillLine> _lines;
    private readonly MeasuredText[] _texts;
    private readonly int _top;
    private readonly float _centerX;

    /// <param name="screen">The state it draws, whose rewards name the skills learned.</param>
    /// <param name="text">Messages and fonts.</param>
    public VictoryLearnedSkillsView(VictoryScreen screen, ScreenText text)
    {
        _screen = screen;
        _font = new OutlinedFont(text.Fonts.FontAt(LinePixels));
        _lines = LearnedSkillLine.AllOf(screen.Rewards, text.Messages);
        _texts = [.. _lines.Select(line => MeasuredText.Of(_font.Font, line.Text))];
        _top = LinesTop(screen.Rewards.Gains.Count);
        _centerX = LinesCenterX(screen.Rewards.Gains.Count);
    }

    /// <summary>The first line's top: under the lowest row of <paramref name="cardCount"/> cards.</summary>
    internal static int LinesTop(int cardCount) =>
        VictoryScreenView.CardTopLeft(cardCount - 1).Y + VictoryScreenView.CardHeight + GapUnderCards;

    /// <summary>
    /// The lines' middle: the middle of the card columns in use, the left one alone for a single card.
    /// </summary>
    internal static float LinesCenterX(int cardCount)
    {
        var lastColumn = VictoryScreenView.CardTopLeft(Math.Clamp(cardCount, 1, 2) - 1);
        return (VictoryScreenView.CardTopLeft(0).X + lastColumn.X + PointedBarShape.CardHeaderWidth) / 2f;
    }

    /// <param name="batch">Already begun.</param>
    public void Draw(SpriteBatch batch)
    {
        for (var index = 0; index < _lines.Count; index++)
        {
            var opacity = _screen.OpacityOf(_lines[index]);
            if (opacity <= 0)
            {
                continue;
            }

            var center = new Vector2(_centerX, _top + (index * LinePitch) + (LinePitch / 2f));
            _font.DrawCentered(batch, _texts[index], center, (UiTheme.TextPrimary * opacity, 1));
        }
    }
}
