using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>
/// An event presenter over the exploration, drawing with the repository's screen text, with default settings and its
/// own voice channel unless a test passes them.
/// </summary>
internal static class EventPresenters
{
    public static EventPresenter Over(ExplorationState exploration, PlayerSettings? settings = null, VoiceChannel? voice = null) =>
        new(exploration, new EventLook(RepositoryScreenText.Text, new UiTextures()), settings ?? new PlayerSettings(), voice ?? new VoiceChannel());
}
