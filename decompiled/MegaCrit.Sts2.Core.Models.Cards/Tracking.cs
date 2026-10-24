using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Tracking : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	public Tracking()
		: base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
		if (base.Owner.Creature.HasPower<TrackingPower>())
		{
			await PowerCmd.Apply<TrackingPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
		}
		else
		{
			await PowerCmd.Apply<TrackingPower>(choiceContext, base.Owner.Creature, 2m, base.Owner.Creature, this);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
