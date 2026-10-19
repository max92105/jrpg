namespace Jrpg.Game.Scenes;

/// <summary>
/// What a scene asks the flow for, such as a new game, instead of calling another scene (ADR 0008). A scene returns
/// null on almost every update, so asking allocates nothing.
/// </summary>
internal abstract record SceneRequest;
