using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ReptileTrinket : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(3m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromPower<StrengthPower>());

	public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
	{
		if (potion.Owner == base.Owner && CombatManager.Instance.IsInProgress)
		{
			Flash();
			await PowerCmd.Apply<ReptileTrinketPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, null);
		}
	}
}
