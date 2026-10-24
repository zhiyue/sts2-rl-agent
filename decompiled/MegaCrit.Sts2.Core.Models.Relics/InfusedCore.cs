using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class InfusedCore : RelicModel
{
	private const string _lightningKey = "Lightning";

	private const string _extraDamageKey = "ExtraDamage";

	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Lightning", 3m),
		new DynamicVar("ExtraDamage", 1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.Channeling),
		HoverTipFactory.FromOrb<LightningOrb>()
	});

	public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (participants.Contains(base.Owner.Creature) && base.Owner.PlayerCombatState.TurnNumber <= 1)
		{
			for (int i = 0; (decimal)i < base.DynamicVars["Lightning"].BaseValue; i++)
			{
				await OrbCmd.Channel<LightningOrb>(new BlockingPlayerChoiceContext(), base.Owner);
			}
		}
	}

	public override decimal ModifyOrbValue(OrbModel orb, decimal value)
	{
		if (orb.Owner != base.Owner)
		{
			return value;
		}
		if (!(orb is LightningOrb))
		{
			return value;
		}
		return value + base.DynamicVars["ExtraDamage"].BaseValue;
	}
}
