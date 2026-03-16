using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class Compact : CardModel
{
	public override bool GainsBlock => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlyArray<IHoverTip>(new IHoverTip[3]
	{
		HoverTipFactory.FromCard<Fuel>(base.IsUpgraded),
		HoverTipFactory.Static(StaticHoverTip.Transform),
		HoverTipFactory.ForEnergy(this)
	});

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(6m, ValueProp.Move));

	public Compact()
		: base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
		await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.Where((CardModel c) => c != null && c.IsTransformable && c.Type == CardType.Status).ToList();
		foreach (CardModel item in list)
		{
			CardModel cardModel = base.CombatState.CreateCard<Fuel>(base.Owner);
			if (base.IsUpgraded)
			{
				CardCmd.Upgrade(cardModel);
			}
			await CardCmd.Transform(item, cardModel);
		}
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Block.UpgradeValueBy(1m);
	}
}
