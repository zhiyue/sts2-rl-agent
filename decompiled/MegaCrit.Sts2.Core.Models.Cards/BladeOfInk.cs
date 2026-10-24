using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BladeOfInk : CardModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(HoverTipFactory.FromCard<Shiv>());
			list.AddRange(HoverTipFactory.FromEnchantment<Inky>());
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new CardsVar(2));

	public BladeOfInk()
		: base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		foreach (CardModel item in await Shiv.CreateInHand(base.Owner, base.DynamicVars.Cards.IntValue, base.CombatState))
		{
			CardCmd.Enchant<Inky>(item, 1m);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
