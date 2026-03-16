using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BulkUp : CardModel
{
	private const string _orbSlotVar = "OrbSlots";

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<DexterityPower>()
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new DynamicVar("OrbSlots", 1m),
		new PowerVar<StrengthPower>(2m),
		new PowerVar<DexterityPower>(2m)
	});

	public BulkUp()
		: base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		OrbCmd.RemoveSlots(base.Owner, base.DynamicVars["OrbSlots"].IntValue);
		await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, base.DynamicVars.Strength.BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<DexterityPower>(base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Strength.UpgradeValueBy(1m);
		base.DynamicVars.Dexterity.UpgradeValueBy(1m);
	}
}
