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

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.AddRange(HoverTipFactory.FromForge());
			list.Add(HoverTipFactory.FromKeyword(CardKeyword.Retain));
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public SummonForth()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		IEnumerable<SovereignBlade> cards = base.Owner.PlayerCombatState.AllCards.OfType<SovereignBlade>().Where(delegate(SovereignBlade c)
		{
			CardPile? pile = c.Pile;
			return pile == null || pile.Type != PileType.Hand;
		});
		await CardPileCmd.Add(cards, PileType.Hand);
		await ForgeCmd.Forge(base.DynamicVars.Forge.IntValue, base.Owner, this);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Forge.UpgradeValueBy(3m);
	}
}
