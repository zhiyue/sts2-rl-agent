namespace MegaCrit.Sts2.Core.Audio;

/// <summary>
/// Set of FMOD sfx that are used more globally (vs being tied to a specific creature of vfx).
/// Any Temporary Sfx or Sfx not using FMOD should be put into TmpSfx.cs
/// </summary>
public static class FmodSfx
{
	public const string block = "event:/sfx/block_gain";

	public const string blockBreak = "event:/sfx/block_break";

	public const string blockHit = "event:/sfx/block_hit";

	public const string buff = "event:/sfx/buff";

	public const string debuff = "event:/sfx/debuff";

	public const string heal = "event:/sfx/heal";

	public const string fire = "event:/sfx/characters/attack_fire";

	public const string backButton = "event:/sfx/ui/clicks/ui_back";

	public const string uiHover = "event:/sfx/ui/clicks/ui_hover";

	public const string uiClick = "event:/sfx/ui/clicks/ui_click";

	public const string timelineHover = "event:/sfx/ui/timeline/ui_timeline_hover";

	public const string timelineHoverLocked = "event:/sfx/ui/timeline/ui_timeline_hover_locked";

	public const string timelineClick = "event:/sfx/ui/timeline/ui_timeline_click";

	public const string timelineOpenEpoch = "event:/sfx/ui/timeline/ui_timeline_open_epoch";

	public const string timelineCloseEpoch = "event:/sfx/ui/timeline/ui_timeline_close_epoch";

	public const string timelineOpen = "event:/sfx/ui/timeline/ui_timeline_open";

	public const string timelineUnlock = "event:/sfx/ui/timeline/ui_timeline_unlock";

	public const string uiTickboxOn = "event:/sfx/ui/clicks/ui_checkbox_on";

	public const string uiTickboxOff = "event:/sfx/ui/clicks/ui_checkbox_off";

	public const string wipeMap = "event:/sfx/ui/wipe_map";

	public const string closeMap = "event:/sfx/ui/map/map_close";

	public const string openMap = "event:/sfx/ui/map/map_open";

	public const string selectMapNode = "event:/sfx/ui/map/map_select";

	public const string eraseMapSketch = "event:/sfx/ui/map/map_erase";

	public const string openPauseMenu = "event:/sfx/ui/pause_open";

	public const string closePauseMenu = "event:/sfx/ui/pause_close";

	public const string relicFlashGeneral = "event:/sfx/ui/relic_activate_general";

	public const string relicFlashDraw = "event:/sfx/ui/relic_activate_draw";

	public const string gainEnergy = "event:/sfx/ui/gain_energy";

	public const string enchant = "event:/sfx/ui/enchant_shimmer";

	public const string merchantWelcome = "event:/sfx/npcs/merchant/merchant_welcome";

	public const string merchantPassive = "event:/sfx/npcs/merchant/merchant_passive";

	public const string merchantThankYou = "event:/sfx/npcs/merchant/merchant_thank_yous";

	public const string merchantDisappointment = "event:/sfx/npcs/merchant/merchant_dissapointment";

	public const string fakeMerchantLaugh = "event:/sfx/npcs/reverse_merchant/reverse_merchant_laugh";

	public const string transform = "event:/sfx/ui/cards/card_transform";

	public const string doomKill = "event:/sfx/characters/necrobinder/necrobinder_doom_kill";

	public const string necrobinderSummon = "event:/sfx/characters/necrobinder/necrobinder_summon";

	public const string cardImpactIntoMulti = "event:/sfx/ui/cards/card_impact_into_multi";

	public const string cardImpactIntoSingle = "event:/sfx/ui/cards/card_impact_into_single";

	public const string cardMovementIntoDeck = "event:/sfx/ui/cards/card_movement_B_into_deck";

	public const string cardMovementIntoDiscard = "event:/sfx/ui/cards/card_movement_B_into_discard";

	public const string cardMovementIntoDraw = "event:/sfx/ui/cards/card_movement_B_into_draw";

	public const string cardMovementPlayIntoDiscard = "event:/sfx/ui/cards/card_movement_B_play_into_discard";

	public const string cardMovementIntoPower = "event:/sfx/ui/cards/card_movement_B_power";
}
