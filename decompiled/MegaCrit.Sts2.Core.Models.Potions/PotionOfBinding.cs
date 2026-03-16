using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Models.Potions;

public sealed class PotionOfBinding : PotionModel
{
	public override PotionRarity Rarity => PotionRarity.Uncommon;

	public override PotionUsage Usage => PotionUsage.CombatOnly;

	public override TargetType TargetType => TargetType.AllEnemies;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<VulnerablePower>(1m),
		new PowerVar<WeakPower>(1m)
	});

	public override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<WeakPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	});

	protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
	{
		IReadOnlyList<Creature> targets = base.Owner.Creature.CombatState.HittableEnemies;
		foreach (Creature item in targets)
		{
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NSmokePuffVfx.Create(item, NSmokePuffVfx.SmokePuffColor.Green));
		}
		await PowerCmd.Apply<WeakPower>(targets, base.DynamicVars["VulnerablePower"].IntValue, base.Owner.Creature, null);
		await PowerCmd.Apply<VulnerablePower>(targets, base.DynamicVars["WeakPower"].IntValue, base.Owner.Creature, null);
	}
}
