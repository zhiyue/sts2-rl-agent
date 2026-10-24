using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace MegaCrit.Sts2.Core.Models.Powers;

public sealed class WitheringPresencePower : PowerModel
{
	private const int _baseCardsLeft = 6;

	private const string _cardsLeftKey = "CardsLeft";

	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override int DisplayAmount => base.DynamicVars["CardsLeft"].IntValue;

	public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("CardsLeft", 6m));

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			Wither wither = (Wither)ModelDb.Card<Wither>().ToMutable();
			if (base.Owner.Monster is Aeonglass aeonglass)
			{
				aeonglass.MatchWitherToUpgradeCount(wither);
			}
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.FromCard(wither));
			list.AddRange(ModelDb.Card<Wither>().HoverTips);
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card.Owner == base.Target.Player)
		{
			base.DynamicVars["CardsLeft"].BaseValue--;
			InvokeDisplayAmountChanged();
			if (base.DynamicVars["CardsLeft"].IntValue <= 0)
			{
				await Cmd.Wait(0.5f);
				await CardPileCmd.AddToCombatAndPreview<Wither>(cardPlay.Card.Owner.Creature, PileType.Hand, 1, null);
				Flash();
				base.DynamicVars["CardsLeft"].BaseValue = 6m;
				InvokeDisplayAmountChanged();
			}
		}
	}
}
