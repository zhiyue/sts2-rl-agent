using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class BigBang : CardModel
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlyArray<DynamicVar>(new DynamicVar[4]
	{
		new CardsVar(1),
		new EnergyVar(1),
		new StarsVar(1),
		new ForgeVar(5)
	});

	protected override IEnumerable<IHoverTip> ExtraHoverTips
	{
		get
		{
			List<IHoverTip> list = new List<IHoverTip>();
			list.Add(base.EnergyHoverTip);
			list.AddRange(HoverTipFactory.FromForge());
			return new _003C_003Ez__ReadOnlyList<IHoverTip>(list);
		}
	}

	public BigBang()
		: base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
		await PlayerCmd.GainStars(base.DynamicVars.Stars.BaseValue, base.Owner);
		await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
		await ForgeCmd.Forge(base.DynamicVars.Forge.IntValue, base.Owner, this);
	}

	protected override void OnUpgrade()
	{
		AddKeyword(CardKeyword.Innate);
	}
}
