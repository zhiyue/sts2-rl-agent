using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Apotheosis : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlyArray<CardKeyword>(new CardKeyword[2]
	{
		CardKeyword.Exhaust,
		CardKeyword.Innate
	});

	public Apotheosis()
		: base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self)
	{
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (CardModel allCard in base.Owner.PlayerCombatState.AllCards)
		{
			if (allCard != this && allCard.IsUpgradable)
			{
				CardCmd.Upgrade(allCard);
			}
		}
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
		base.EnergyCost.UpgradeBy(-1);
	}
}
