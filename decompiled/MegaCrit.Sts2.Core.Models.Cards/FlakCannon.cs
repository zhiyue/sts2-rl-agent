using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FlakCannon : CardModel
{
	private const string _calculatedHitsKey = "CalculatedHits";

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new DamageVar(8m, ValueProp.Move),
		new CalculationBaseVar(0m),
		new CalculationExtraVar(1m),
		new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => GetStatuses(card.Owner).Count())
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Exhaust));

	public FlakCannon()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		List<CardModel> list = GetStatuses(base.Owner).ToList();
		int statusCount = (int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target);
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(statusCount).FromCard(this)
			.TargetingRandomOpponents(base.CombatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	private static IEnumerable<CardModel> GetStatuses(Player owner)
	{
		return owner.PlayerCombatState.AllCards.Where((CardModel c) => c.Type == CardType.Status && c.Pile.Type != PileType.Exhaust);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
