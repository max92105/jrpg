namespace Jrpg.Game.Exploration;

/// <summary>
/// What the exploration scene shows over the field besides events: the interaction prompt and the entry banner, the
/// scene driving <see cref="Banner"/> and drawing <see cref="BannerView"/>, which reads the same banner.
/// </summary>
internal sealed record FieldOverlays(InteractionPromptView Prompt, MapBanner Banner, MapBannerView BannerView);
