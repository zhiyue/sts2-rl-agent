using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BorrowedTime : CardModel
{
	public const int doomAmount = 3;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<DoomPower>(3m),
		new EnergyVar(1)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<DoomPower>(),
		base.EnergyHoverTip
	});

	public BorrowedTime()
		: base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<DoomPower>(base.Owner.Creature, base.DynamicVars.Doom.BaseValue, base.Owner.Creature, this);
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
