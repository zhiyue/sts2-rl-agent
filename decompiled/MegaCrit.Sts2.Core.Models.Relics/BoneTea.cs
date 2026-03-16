using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BoneTea : RelicModel
{
	private const string _combatsKey = "Combats";

	private int _combatsLeft = 1;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool IsUsedUp => CombatsLeft <= 0;

	public override bool ShowCounter => false;

	public override int DisplayAmount => Math.Max(0, CombatsLeft);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Combats", CombatsLeft));

	[SavedProperty]
	public int CombatsLeft
	{
		get
		{
			return _combatsLeft;
		}
		set
		{
			AssertMutable();
			_combatsLeft = value;
			base.DynamicVars["Combats"].BaseValue = _combatsLeft;
			InvokeDisplayAmountChanged();
			if (IsUsedUp)
			{
				base.Status = RelicStatus.Disabled;
			}
		}
	}

	public override Task AfterSideTurnStart(CombatSide side, CombatState combatState)
	{
		if (IsUsedUp)
		{
			return Task.CompletedTask;
		}
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		if (combatState.RoundNumber > 1)
		{
			return Task.CompletedTask;
		}
		foreach (CardModel card in PileType.Hand.GetPile(base.Owner).Cards)
		{
			CardCmd.Upgrade(card);
		}
		CombatsLeft--;
		Flash();
		return Task.CompletedTask;
	}
}
