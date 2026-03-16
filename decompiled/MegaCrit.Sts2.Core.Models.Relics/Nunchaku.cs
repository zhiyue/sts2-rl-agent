using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Nunchaku : RelicModel
{
	private bool _isActivating;

	private int _attacksPlayed;

	public override RelicRarity Rarity => RelicRarity.Uncommon;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return AttacksPlayed % base.DynamicVars.Cards.IntValue;
			}
			return base.DynamicVars.Cards.IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(10),
		new EnergyVar(1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.ForEnergy(this));

	private bool IsActivating
	{
		get
		{
			return _isActivating;
		}
		set
		{
			AssertMutable();
			_isActivating = value;
			UpdateDisplay();
		}
	}

	[SavedProperty]
	public int AttacksPlayed
	{
		get
		{
			return _attacksPlayed;
		}
		set
		{
			AssertMutable();
			_attacksPlayed = value;
			UpdateDisplay();
		}
	}

	private void UpdateDisplay()
	{
		if (IsActivating)
		{
			base.Status = RelicStatus.Normal;
		}
		else
		{
			int intValue = base.DynamicVars.Cards.IntValue;
			base.Status = ((AttacksPlayed % intValue == intValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
		}
		InvokeDisplayAmountChanged();
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Owner && cardPlay.Card.Type == CardType.Attack)
		{
			AttacksPlayed++;
			int intValue = base.DynamicVars.Cards.IntValue;
			if (CombatManager.Instance.IsInProgress && AttacksPlayed % intValue == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			}
		}
	}

	private async Task DoActivateVisuals()
	{
		IsActivating = true;
		Flash();
		await Cmd.Wait(1f);
		IsActivating = false;
	}
}
