using Jrpg.Core.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Dialogue;

/// <summary>The message for a recruit step: who joined, or that they were already in the party.</summary>
internal static class RecruitText
{
    public static string Of(Recruitment recruitment, UiMessages messages) => messages.Format(
        recruitment.HasJoined ? MessageIds.MemberJoined : MessageIds.MemberAlreadyInParty,
        new MessageArgument("name", recruitment.Member.Definition.Name));
}
