using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Scenes;

/// <summary>
/// A root scene that sometimes needs a still copy of the frame it just drew, such as the field breaking into a battle
/// (decision 258). Only the host draws, so the host makes the copy; the scene's update never touches the device (ADR
/// 0008).
/// </summary>
internal interface IFreezesFrame
{
    /// <summary>Whether the scene wants a copy of the frame the host has just drawn.</summary>
    bool WantsFrozenFrame { get; }

    /// <summary>Hands the scene the copy, which the host owns and keeps until the next one.</summary>
    /// <param name="frame">The virtual screen as drawn, <c>1920 x 1080</c>.</param>
    void FreezeFrame(Texture2D frame);
}
