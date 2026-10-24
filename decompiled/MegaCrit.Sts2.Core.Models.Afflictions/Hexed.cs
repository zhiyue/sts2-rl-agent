using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Afflictions;

/// <summary>
/// Most of this Affliction's logic lives in <see cref="T:MegaCrit.Sts2.Core.Models.Powers.HexPower" />.
/// </summary>
public sealed class Hexed : AfflictionModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => new global::_003C_003Ez__ReadOnlySingleElementList<IHoverTip>(HoverTipFactory.FromKeyword(CardKeyword.Ethereal));

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (card != base.Card)
		{
			return Task.CompletedTask;
		}
		if (card.Owner.Creature.HasPower<HexPower>())
		{
			return Task.CompletedTask;
		}
		CardCmd.ClearAffliction(base.Card);
		return Task.CompletedTask;
	}
}
