using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BeatDown : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(3));

	public BeatDown()
		: base(3, CardType.Skill, CardRarity.Rare, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		CardPile pile = PileType.Discard.GetPile(base.Owner);
		IEnumerable<CardModel> enumerable = pile.Cards.Where((CardModel c) => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
			.Take(base.DynamicVars.Cards.IntValue);
		foreach (CardModel item in enumerable)
		{
			if (!CombatManager.Instance.IsOverOrEnding)
			{
				if (item.TargetType == TargetType.AnyEnemy)
				{
					Creature target = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
					await CardCmd.AutoPlay(choiceContext, item, target);
				}
				else
				{
					await CardCmd.AutoPlay(choiceContext, item, null);
				}
				continue;
			}
			break;
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
