using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ArtOfWar : RelicModel
{
	private bool _anyAttacksPlayedLastTurn;

	private bool _anyAttacksPlayedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new EnergyVar(1));

	private bool AnyAttacksPlayedLastTurn
	{
		get
		{
			return _anyAttacksPlayedLastTurn;
		}
		set
		{
			AssertMutable();
			_anyAttacksPlayedLastTurn = value;
		}
	}

	private bool AnyAttacksPlayedThisTurn
	{
		get
		{
			return _anyAttacksPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_anyAttacksPlayedThisTurn = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (base.Owner != cardPlay.Card.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		if (cardPlay.Card.Type != CardType.Attack)
		{
			return Task.CompletedTask;
		}
		if (AnyAttacksPlayedLastTurn)
		{
			return Task.CompletedTask;
		}
		base.Status = RelicStatus.Normal;
		AnyAttacksPlayedThisTurn = true;
		return Task.CompletedTask;
	}

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Creature.Side)
		{
			return Task.CompletedTask;
		}
		AnyAttacksPlayedLastTurn = AnyAttacksPlayedThisTurn;
		AnyAttacksPlayedThisTurn = false;
		return Task.CompletedTask;
	}

	public override async Task AfterEnergyReset(Player player)
	{
		if (player != base.Owner)
		{
			return;
		}
		base.Status = RelicStatus.Active;
		if (base.Owner.Creature.CombatState.RoundNumber > 1)
		{
			if (!AnyAttacksPlayedLastTurn)
			{
				Flash();
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			}
			AnyAttacksPlayedLastTurn = false;
			AnyAttacksPlayedThisTurn = false;
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		AnyAttacksPlayedLastTurn = false;
		AnyAttacksPlayedThisTurn = false;
		return Task.CompletedTask;
	}
}
