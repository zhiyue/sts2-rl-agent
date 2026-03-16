using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class TheSmith : CardModel
{
	public override int CanonicalStarCost => 4;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new ForgeVar(30));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge();

	public TheSmith()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await ForgeCmd.Forge(base.DynamicVars.Forge.IntValue, base.Owner, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Forge.UpgradeValueBy(10m);
	}
}
