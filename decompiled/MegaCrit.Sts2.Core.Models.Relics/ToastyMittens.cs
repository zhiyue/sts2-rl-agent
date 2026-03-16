using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class ToastyMittens : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new PowerVar<StrengthPower>(1m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
		HoverTipFactory.FromPower<StrengthPower>()
	});

	public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
	{
		if (player != base.Owner.Creature.Player)
		{
			return;
		}
		Flash();
		await CardPileCmd.ShuffleIfNecessary(choiceContext, base.Owner);
		IReadOnlyList<CardModel> cards = PileType.Draw.GetPile(player).Cards;
		CardModel cardModel = null;
		if (combatState.RoundNumber == 1)
		{
			cardModel = cards.FirstOrDefault((CardModel c) => !c.Keywords.Contains(CardKeyword.Innate));
		}
		if (cardModel == null)
		{
			cardModel = cards.FirstOrDefault();
		}
		if (cardModel != null)
		{
			await CardCmd.Exhaust(choiceContext, cardModel);
		}
		await PowerCmd.Apply<StrengthPower>(player.Creature, base.DynamicVars.Strength.BaseValue, player.Creature, null);
	}
}
