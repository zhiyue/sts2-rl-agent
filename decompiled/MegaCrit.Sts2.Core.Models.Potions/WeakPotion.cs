using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class WeakPotion : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Common;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AnyEnemy;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<WeakPower>(3m));

	public override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<WeakPower>());

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		PotionModel.AssertValidForTargetedPotion(target);
		NCombatRoom.Instance?.PlaySplashVfx(target, new Color("94f882"));
		await PowerCmd.Apply<WeakPower>(target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, null);
	}
}
