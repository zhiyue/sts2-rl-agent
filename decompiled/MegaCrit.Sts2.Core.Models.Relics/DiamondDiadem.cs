using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class DiamondDiadem : RelicModel
{
	private const string _cardThresholdKey = "CardThreshold";

	private int _cardsPlayedThisTurn;

	public override bool ShowCounter => CombatManager.Instance.IsInProgress;

	public override int DisplayAmount => _cardsPlayedThisTurn;

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public int CardsPlayedThisTurn
	{
		get
		{
			return _cardsPlayedThisTurn;
		}
		set
		{
			AssertMutable();
			_cardsPlayedThisTurn = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CardThreshold", 2m));

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner)
		{
			return Task.CompletedTask;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return Task.CompletedTask;
		}
		CardsPlayedThisTurn++;
		RefreshCounter();
		return Task.CompletedTask;
	}

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner.Creature))
		{
			if ((decimal)CardsPlayedThisTurn <= base.DynamicVars["CardThreshold"].BaseValue)
			{
				Flash();
				await PowerCmd.Apply<DiamondDiademPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, null);
			}
			CardsPlayedThisTurn = 0;
			RefreshCounter();
		}
	}

	private void RefreshCounter()
	{
		base.Status = (((decimal)CardsPlayedThisTurn <= base.DynamicVars["CardThreshold"].BaseValue) ? RelicStatus.Active : RelicStatus.Normal);
		InvokeDisplayAmountChanged();
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		CardsPlayedThisTurn = 0;
		base.Status = RelicStatus.Normal;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
