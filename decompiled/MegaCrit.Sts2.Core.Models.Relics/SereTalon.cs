using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class SereTalon : RelicModel
{
	private const string _cursesKey = "Curses";

	private const string _wishesKey = "Wishes";

	public override RelicRarity Rarity => RelicRarity.Ancient;

	public override bool HasUponPickupEffect => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[2]
	{
		new DynamicVar("Curses", 2m),
		new DynamicVar("Wishes", 3m)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<Wish>();

	public override async Task AfterObtained()
	{
		HashSet<CardModel> availableCurses = (from c in ModelDb.CardPool<CurseCardPool>().GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint)
			where c.CanBeGeneratedByModifiers
			select c).ToHashSet();
		List<CardPileAddResult> curseResults = new List<CardPileAddResult>();
		for (int i = 0; i < base.DynamicVars["Curses"].IntValue; i++)
		{
			CardModel cardModel = base.Owner.RunState.Rng.Niche.NextItem(availableCurses);
			availableCurses.Remove(cardModel);
			CardModel card = base.Owner.RunState.CreateCard(cardModel, base.Owner);
			curseResults.Add(await CardPileCmd.Add(card, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(curseResults, 2f);
		await Cmd.Wait(0.75f);
		List<CardPileAddResult> wishResults = new List<CardPileAddResult>();
		for (int i = 0; i < base.DynamicVars["Wishes"].IntValue; i++)
		{
			CardModel card2 = base.Owner.RunState.CreateCard(ModelDb.Card<Wish>(), base.Owner);
			wishResults.Add(await CardPileCmd.Add(card2, PileType.Deck));
		}
		CardCmd.PreviewCardPileAdd(wishResults, 2f);
		await Cmd.Wait(0.75f);
	}
}
