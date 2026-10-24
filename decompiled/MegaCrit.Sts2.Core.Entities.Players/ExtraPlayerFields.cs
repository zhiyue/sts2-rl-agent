using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Entities.Players;

/// <summary>
/// Extra fields that are used for specific pieces of content per-player.
/// </summary>
public class ExtraPlayerFields
{
	/// <summary>
	/// The number of times the player has removed a card from their deck in the card shop.
	/// The card removal price increases each time, so we need to track this across the whole run.
	/// </summary>
	public int CardShopRemovalsUsed { get; set; }

	/// <summary>
	/// Points earned in the <see cref="T:MegaCrit.Sts2.Core.Models.Events.WelcomeToWongos" /> event.
	/// We can't save these immediately to SerializableProgress because, otherwise, the player would be able to exit the client,
	/// reload the event, and accumulate more points. We can't write to SerializableProgress and wait for something else to save
	/// it, because it only saves at the end of a run. So, we have to save this to the run data and let SaveManager
	/// accumulate the points into SerializableProgress at the end of the run.
	/// </summary>
	public int WongoPoints { get; set; }

	/// <summary>
	/// Whether or not the player unlocked the CCCCombo badge.
	/// </summary>
	public bool CccomboBadgeUnlocked { get; set; }

	/// <summary>
	/// How much damage the player dealt, aggregated among all combats.
	/// </summary>
	public int DamageDealt { get; set; }

	/// <summary>
	/// How many debuffs the player applied to enemies, aggregated among all combats.
	/// </summary>
	public int DebuffsApplied { get; set; }

	public SerializableExtraPlayerFields ToSerializable()
	{
		return new SerializableExtraPlayerFields
		{
			CardShopRemovalsUsed = CardShopRemovalsUsed,
			WongoPoints = WongoPoints,
			CccomboBadgeUnlocked = CccomboBadgeUnlocked,
			DamageDealt = DamageDealt,
			DebuffsApplied = DebuffsApplied
		};
	}

	public static ExtraPlayerFields FromSerializable(SerializableExtraPlayerFields save)
	{
		return new ExtraPlayerFields
		{
			CardShopRemovalsUsed = save.CardShopRemovalsUsed,
			WongoPoints = save.WongoPoints,
			CccomboBadgeUnlocked = save.CccomboBadgeUnlocked,
			DamageDealt = save.DamageDealt,
			DebuffsApplied = save.DebuffsApplied
		};
	}
}
