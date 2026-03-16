using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class EssenceOfDarkness : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Rare;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.Self;

	public override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<DarkOrb>()
	});

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		int count = base.Owner.PlayerCombatState.OrbQueue.Capacity;
		for (int i = 0; i < count; i++)
		{
			await OrbCmd.Channel<DarkOrb>(choiceContext, base.Owner);
		}
	}
}
