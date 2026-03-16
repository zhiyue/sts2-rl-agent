using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class CloakClasp : RelicModel
{
	public override RelicRarity Rarity => RelicRarity.Rare;

	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new BlockVar(1m, ValueProp.Unpowered));

	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.Static(StaticHoverTip.Block));

	public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Creature.Side)
		{
			IReadOnlyList<CardModel> cards = PileType.Hand.GetPile(base.Owner).Cards;
			if (cards.Count != 0)
			{
				int num = (int)((decimal)cards.Count * base.DynamicVars.Block.BaseValue);
				Flash();
				await CreatureCmd.GainBlock(base.Owner.Creature, num, ValueProp.Unpowered, null);
			}
		}
	}
}
