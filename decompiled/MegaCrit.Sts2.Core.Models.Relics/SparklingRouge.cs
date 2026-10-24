using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SparklingRouge : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<StrengthPower>(1m),
		new PowerVar<DexterityPower>(1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>()
	});

	public override async Task AfterBlockCleared(Creature creature)
	{
		if (creature == base.Owner.Creature && base.Owner.PlayerCombatState.TurnNumber == 3)
		{
			Flash();
			await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, null);
			await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, null);
		}
	}
}
