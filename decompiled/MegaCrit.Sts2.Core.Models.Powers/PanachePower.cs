using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class PanachePower : PowerModel
{
	private class Data
	{
		public bool alreadyApplied;
	}

	private const int _baseCardsLeft = 5;

	private const string _cardsLeftKey = "CardsLeft";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => base.DynamicVars["CardsLeft"].IntValue;

	public override bool IsInstanced => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CardsLeft", 5m));

	protected override object InitInternalData()
	{
		return new Data();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner != base.Owner.Player)
		{
			return;
		}
		Data data = GetInternalData<Data>();
		if (data.alreadyApplied)
		{
			base.DynamicVars["CardsLeft"].BaseValue--;
			InvokeDisplayAmountChanged();
			if (base.DynamicVars["CardsLeft"].IntValue <= 0)
			{
				await Cmd.Wait(0.5f);
				await CreatureCmd.Damage(context, base.CombatState.HittableEnemies, base.Amount, ValueProp.Unpowered, base.Owner);
				base.DynamicVars["CardsLeft"].BaseValue = 5m;
				InvokeDisplayAmountChanged();
			}
		}
		data.alreadyApplied = true;
	}

	public override Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side != base.Owner.Side)
		{
			return Task.CompletedTask;
		}
		base.DynamicVars["CardsLeft"].BaseValue = 5m;
		InvokeDisplayAmountChanged();
		return Task.CompletedTask;
	}
}
