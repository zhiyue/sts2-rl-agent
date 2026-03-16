using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Permafrost : RelicModel
{
	private bool _activatedThisCombat;

	public override RelicRarity Rarity => RelicRarity.Common;

	private bool ActivatedThisCombat
	{
		get
		{
			return _activatedThisCombat;
		}
		set
		{
			AssertMutable();
			_activatedThisCombat = value;
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(6m, ValueProp.Unpowered));

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		ActivatedThisCombat = false;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
	{
		if (CombatManager.Instance.IsInProgress && cardPlay.Card.Owner == base.Owner && cardPlay.Card.Type == CardType.Power && !ActivatedThisCombat)
		{
			Flash();
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, null);
			ActivatedThisCombat = true;
		}
	}
}
