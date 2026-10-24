using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Combat.History.Entries;

/// <summary>
/// Tracks every time a creature receives damage.
/// </summary>
public class DamageReceivedEntry : CombatHistoryEntry
{
	public DamageResult Result { get; }

	public Creature? Dealer { get; }

	public CardModel? CardSource { get; }

	public Creature Receiver => base.Actor;

	public override string Description
	{
		get
		{
			string id = GetId(Receiver);
			if (Dealer == null)
			{
				return $"{id} took {Result.UnblockedDamage} damage";
			}
			return $"{GetId(Dealer)} dealt {Result.UnblockedDamage} damage to {id}";
		}
	}

	public DamageReceivedEntry(DamageResult result, Creature receiver, Creature? dealer, CardModel? cardSource, int roundNumber, CombatSide currentSide, CombatHistory history, IEnumerable<Player> players)
		: base(receiver, roundNumber, currentSide, history, players)
	{
		Result = result;
		Dealer = dealer;
		CardSource = cardSource;
	}

	private static string GetId(Creature creature)
	{
		if (!creature.IsPlayer)
		{
			return creature.Monster.Id.Entry;
		}
		return creature.Player.Character.Id.Entry;
	}
}
