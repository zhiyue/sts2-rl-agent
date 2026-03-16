using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Dualcast : CardModel
{
	public override OrbEvokeType OrbEvokeType => OrbEvokeType.Front;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Evoke));

	public Dualcast()
		: base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (base.Owner.PlayerCombatState.OrbQueue.Orbs.Count > 0)
		{
			await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
			await OrbCmd.EvokeNext(choiceContext, base.Owner, dequeue: false);
			await Cmd.CustomScaledWait(0.1f, 0.25f);
			await OrbCmd.EvokeNext(choiceContext, base.Owner);
		}
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
