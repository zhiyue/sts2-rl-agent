using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Characters;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Cleanse : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new SummonVar(3m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.Static(StaticHoverTip.SummonDynamic, base.DynamicVars.Summon),
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
	});

	public Cleanse()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, Necrobinder.GetSummonAnimIfApplicable(base.Owner.Character), Necrobinder.GetSummonDelayIfApplicable(base.Owner.Character));
		await OstyCmd.Summon(choiceContext, base.Owner, base.DynamicVars.Summon.BaseValue, this);
		CardModel cardModel = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Draw.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1))).FirstOrDefault();
		if (cardModel != null)
		{
			await CardCmd.Exhaust(choiceContext, cardModel);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Summon.UpgradeValueBy(2m);
	}
}
