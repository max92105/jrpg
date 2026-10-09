using Godot;
using System;

public partial class MainMenu : CanvasLayer
{
	[Export] public ColorRect FadeRect;
	[Export] public TextureRect Background;
	[Export] public Label TitleLabel;
	[Export] public VBoxContainer MenuVBox;
	[Export] public Button StartGame;
	[Export] public Button LoadGame;
	[Export] public Button ExitGame;

	public override void _Ready()
	{
		// Hook up button signals
		StartGame.Pressed += OnStartGamePressed;
		LoadGame.Pressed += OnLoadGamePressed;
		ExitGame.Pressed += OnExitGamePressed;

		// Start fade in
		FadeRect.Modulate = new Color(0, 0, 0, 1);
		var tween = GetTree().CreateTween();
		tween.TweenProperty(FadeRect, "modulate:a", 0f, 0.8);
	}

	private void OnStartGamePressed()
	{
		GD.Print("Start Game Pressed");
		// GetTree().ChangeSceneToFile("res://Scenes/Game/GameScene.tscn");
	}

	private void OnLoadGamePressed()
	{
		GD.Print("Load Game Pressed");
		// Load screen not yet implemented
	}

	private void OnExitGamePressed()
	{
		GetTree().Quit();
	}
}
