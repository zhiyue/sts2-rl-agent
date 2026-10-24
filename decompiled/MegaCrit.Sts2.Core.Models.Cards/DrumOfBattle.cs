using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class DrumOfBattle : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new CardsVar(2),
		new EnergyVar(2)
	});

	public DrumOfBattle()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card == this && base.CombatState != null)
		{
			int playCount = await GeneratePlayCount(base.CombatState, null);
			for (int i = 0; i < playCount; i++)
			{
				await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, base.Owner);
			}
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
