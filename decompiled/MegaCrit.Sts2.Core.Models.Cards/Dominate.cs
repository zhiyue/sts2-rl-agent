using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Dominate : CardModel
{
	private const string _strengthPerVulnerableKey = "StrengthPerVulnerable";

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new PowerVar<VulnerablePower>(1m),
		new DynamicVar("StrengthPerVulnerable", 1m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromPower<StrengthPower>(),
		HoverTipFactory.FromPower<VulnerablePower>()
	});

	public Dominate()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars["VulnerablePower"].BaseValue, base.Owner.Creature, this);
		int num = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
		await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, num, base.Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
	}
}
