using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class KingsCourage : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyPlayer;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new ForgeVar(15));

	public override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		await ForgeCmd.Forge(base.DynamicVars.Forge.IntValue, target.Player, this);
	}
}
