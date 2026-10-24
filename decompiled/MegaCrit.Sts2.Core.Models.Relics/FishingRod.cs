using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class FishingRod : RelicModel
{
	private const string _combatsKey = "Combats";

	private int _combatsSeen;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override int DisplayAmount => CombatsSeen % base.DynamicVars["Combats"].IntValue;

	public override bool ShowCounter => true;

	[SavedProperty]
	public int CombatsSeen
	{
		get
		{
			return _combatsSeen;
		}
		set
		{
			AssertMutable();
			_combatsSeen = value;
			InvokeDisplayAmountChanged();
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Combats", 3m));

	public override Task AfterCombatEnd(CombatRoom room)
	{
		if (room.Encounter.RoomType != RoomType.Monster)
		{
			return Task.CompletedTask;
		}
		CombatsSeen++;
		if (CombatsSeen % base.DynamicVars["Combats"].IntValue == 0)
		{
			Flash();
			IEnumerable<CardModel> items = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c.IsUpgradable);
			CardModel cardModel = base.Owner.RunState.Rng.Niche.NextItem(items);
			if (cardModel != null)
			{
				CardCmd.Upgrade(cardModel);
			}
		}
		return Task.CompletedTask;
	}
}
