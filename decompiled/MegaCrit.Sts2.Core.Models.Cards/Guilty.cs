using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Guilty : CardModel
{
	public const int maxCombats = 5;

	private const string _combatsKey = "Combats";

	private int _combatsSeen;

	public override int MaxUpgradeLevel => 0;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Combats", 5m));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Unplayable);

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
			base.DynamicVars["Combats"].BaseValue = 5 - CombatsSeen;
		}
	}

	public Guilty()
		: base(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
	{
	}

	public override async Task AfterCombatEnd(CombatRoom _)
	{
		CardPile? pile = base.Pile;
		if (pile != null && pile.Type == PileType.Deck)
		{
			CombatsSeen++;
			if (CombatsSeen >= 5 && base.Pile.Type == PileType.Deck)
			{
				await CardPileCmd.RemoveFromDeck(this);
			}
		}
	}
}
