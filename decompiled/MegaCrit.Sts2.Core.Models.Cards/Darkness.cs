using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Darkness : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<DarkOrb>()
	});

	public Darkness()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await OrbCmd.Channel<DarkOrb>(choiceContext, base.Owner);
		IEnumerable<OrbModel> enumerable = base.Owner.PlayerCombatState.OrbQueue.Orbs.Where((OrbModel orb) => orb is DarkOrb);
		int triggerCount = ((!base.IsUpgraded) ? 1 : 2);
		foreach (OrbModel darknessOrb in enumerable)
		{
			for (int i = 0; i < triggerCount; i++)
			{
				await OrbCmd.Passive(choiceContext, darknessOrb, null);
			}
		}
	}
}
