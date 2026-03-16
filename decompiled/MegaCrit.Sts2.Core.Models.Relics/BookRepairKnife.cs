using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class BookRepairKnife : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<DoomPower>());

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new HealVar(3m));

	public override Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
	{
		int num = creatures.Count((Creature c) => c != base.Owner.Creature && c.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal()));
		if (num == 0)
		{
			return Task.CompletedTask;
		}
		Flash();
		return CreatureCmd.Heal(base.Owner.Creature, base.DynamicVars.Heal.BaseValue * (decimal)num);
	}
}
