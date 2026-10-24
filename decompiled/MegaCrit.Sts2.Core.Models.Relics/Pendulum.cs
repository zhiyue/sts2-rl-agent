using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Pendulum : RelicModel
{
	private const string _turnsKey = "Turns";

	private bool _isActivating;

	private int _turnsSeen;

	public override string FlashSfx => "event:/sfx/ui/relic_activate_draw";

	public override RelicRarity Rarity => RelicRarity.Common;

	public override bool ShowCounter => true;

	public override int DisplayAmount
	{
		get
		{
			if (!IsActivating)
			{
				return TurnsSeen;
			}
			return base.DynamicVars["Turns"].IntValue;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(1),
		new DynamicVar("Turns", 3m)
	});

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
			InvokeDisplayAmountChanged();
		}
	}

	[SavedProperty]
	public int TurnsSeen
	{
		get
		{
			return _turnsSeen;
		}
		set
		{
			AssertMutable();
			_turnsSeen = value;
			InvokeDisplayAmountChanged();
		}
	}

	public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
		if (player == base.Owner)
		{
			TurnsSeen = (TurnsSeen + 1) % base.DynamicVars["Turns"].IntValue;
			base.Status = ((TurnsSeen == base.DynamicVars["Turns"].IntValue - 1) ? RelicStatus.Active : RelicStatus.Normal);
			if (TurnsSeen == 0)
			{
				TaskHelper.RunSafely(DoActivateVisuals());
				await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
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

	public override Task AfterCombatEnd(CombatRoom _)
	{
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
