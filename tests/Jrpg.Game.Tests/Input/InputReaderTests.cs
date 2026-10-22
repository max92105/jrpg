using Jrpg.Game.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Jrpg.Game.Tests.Input;

public sealed class InputReaderTests
{
    private static readonly GamePadState _padAtRest = GamePadState.Default;

    private readonly InputReader _reader = new();

    [Fact]
    public void Read_KeyHeldTwoUpdates_PressedOnlyOnFirst()
    {
        var space = new KeyboardState(Keys.Space);

        var first = _reader.Read(space, _padAtRest);
        var second = _reader.Read(space, _padAtRest);

        Assert.True(first.IsPressed(GameButton.Confirm));
        Assert.False(second.IsPressed(GameButton.Confirm));
        Assert.True(second.IsHeld(GameButton.Confirm));
    }

    [Fact]
    public void Read_KeyReleasedThenPressedAgain_PressedAgain()
    {
        _reader.Read(new KeyboardState(Keys.Enter), _padAtRest);
        _reader.Read(new KeyboardState(), _padAtRest);

        var again = _reader.Read(new KeyboardState(Keys.Enter), _padAtRest);

        Assert.True(again.IsPressed(GameButton.Confirm));
    }

    [Fact]
    public void Read_Escape_PressesCancelAndMenu()
    {
        var frame = _reader.Read(new KeyboardState(Keys.Escape), _padAtRest);

        Assert.True(frame.IsPressed(GameButton.Cancel));
        Assert.True(frame.IsPressed(GameButton.Menu));
    }

    [Fact]
    public void Read_PadA_PressesConfirmLikeTheKeyboard()
    {
        var frame = _reader.Read(new KeyboardState(), PadWith(Buttons.A));

        Assert.True(frame.IsPressed(GameButton.Confirm));
    }

    [Fact]
    public void Read_ArrowKey_MovesThatWay()
    {
        var frame = _reader.Read(new KeyboardState(Keys.Right), _padAtRest);

        Assert.Equal(1, frame.Move.X);
        Assert.Equal(0, frame.Move.Y);
    }

    [Fact]
    public void Read_WKey_MovesUp()
    {
        var frame = _reader.Read(new KeyboardState(Keys.W), _padAtRest);

        Assert.Equal(0, frame.Move.X);
        Assert.Equal(-1, frame.Move.Y);
    }

    [Fact]
    public void Read_Backspace_PressesCancelOnly()
    {
        var frame = _reader.Read(new KeyboardState(Keys.Back), _padAtRest);

        Assert.True(frame.IsPressed(GameButton.Cancel));
        Assert.False(frame.IsPressed(GameButton.Menu));
    }

    [Fact]
    public void Read_StickOutsideDeadzone_OverridesKeys()
    {
        var stickDown = PadWith(new Vector2(0, -0.6f));

        var frame = _reader.Read(new KeyboardState(Keys.Up), stickDown);

        Assert.Equal(0.6, frame.Move.Y, 6);
    }

    [Fact]
    public void Read_NothingPressed_DeviceIsKeyboard()
    {
        var frame = _reader.Read(new KeyboardState(), _padAtRest);

        Assert.Equal(InputDevice.Keyboard, frame.Device);
    }

    [Fact]
    public void Read_PadButtonPressed_SwitchesToController()
    {
        var frame = _reader.Read(new KeyboardState(), PadWith(Buttons.A));

        Assert.Equal(InputDevice.Controller, frame.Device);
    }

    [Fact]
    public void Read_StickPastDeadZone_SwitchesToController()
    {
        var frame = _reader.Read(new KeyboardState(), PadWith(new Vector2(StickReader.Deadzone + 0.1f, 0)));

        Assert.Equal(InputDevice.Controller, frame.Device);
    }

    [Fact]
    public void Read_StickInsideDeadZone_KeepsTheDevice()
    {
        var frame = _reader.Read(new KeyboardState(), PadWith(new Vector2(StickReader.Deadzone / 2, 0)));

        Assert.Equal(InputDevice.Keyboard, frame.Device);
    }

    [Fact]
    public void Read_KeyPressedAfterPad_SwitchesBackToKeyboard()
    {
        _reader.Read(new KeyboardState(), PadWith(Buttons.A));

        var frame = _reader.Read(new KeyboardState(Keys.E), _padAtRest);

        Assert.Equal(InputDevice.Keyboard, frame.Device);
    }

    [Fact]
    public void Read_NothingPressedAfterPad_StaysController()
    {
        _reader.Read(new KeyboardState(), PadWith(Buttons.A));

        var frame = _reader.Read(new KeyboardState(), _padAtRest);

        Assert.Equal(InputDevice.Controller, frame.Device);
    }

    [Fact]
    public void Read_StickStillHeldAfterKeyPressed_StaysKeyboard()
    {
        var stickRight = PadWith(new Vector2(StickReader.Deadzone + 0.1f, 0));
        _reader.Read(new KeyboardState(), stickRight);
        _reader.Read(new KeyboardState(Keys.E), stickRight);

        var frame = _reader.Read(new KeyboardState(), stickRight);

        Assert.Equal(InputDevice.Keyboard, frame.Device);
    }

    [Fact]
    public void Read_SameButtonFromKeyThenPad_NotPressedAgain()
    {
        _reader.Read(new KeyboardState(Keys.E), _padAtRest);

        var frame = _reader.Read(new KeyboardState(Keys.E), PadWith(Buttons.A));

        Assert.False(frame.IsPressed(GameButton.Confirm));
        Assert.Equal(InputDevice.Controller, frame.Device);
    }

    private static GamePadState PadWith(Buttons button) =>
        new(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(button), new GamePadDPad());

    private static GamePadState PadWith(Vector2 leftStick) =>
        new(new GamePadThumbSticks(leftStick, Vector2.Zero), new GamePadTriggers(), new GamePadButtons(), new GamePadDPad());
}
