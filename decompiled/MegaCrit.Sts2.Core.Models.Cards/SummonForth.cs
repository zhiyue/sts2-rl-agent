using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class SummonForth : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new ForgeVar(8));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromForge().Concat(new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Retain)));

	public SummonForth()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await ForgeCmd.Forge(base.DynamicVars.Forge.IntValue, base.Owner, this);
		IEnumerable<SovereignBlade> enumerable = (from c in base.Owner.PlayerCombatState.AllCards.OfType<SovereignBlade>()
			where c.Pile.Type != PileType.Hand
			select c).ToList();
		foreach (SovereignBlade item in enumerable)
		{
			await CardPileCmd.Add(item, PileType.Hand);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Forge.UpgradeValueBy(3m);
	}
}
