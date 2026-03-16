using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Relax : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[3]
	{
		new BlockVar(15m, ValueProp.Move),
		new CardsVar(2),
		new EnergyVar(2)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(base.EnergyHoverTip);

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	public Relax()
		: base(3, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		await PowerCmd.Apply<DrawCardsNextTurnPower>(base.Owner.Creature, base.DynamicVars.Cards.BaseValue, base.Owner.Creature, this);
		await PowerCmd.Apply<EnergyNextTurnPower>(base.Owner.Creature, base.DynamicVars.Energy.BaseValue, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(2m);
		base.DynamicVars.Cards.UpgradeValueBy(1m);
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
