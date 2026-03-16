using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Afflictions;

public sealed class Hexed : AfflictionModel
{
	private bool _appliedEthereal;

	public bool AppliedEthereal
	{
		get
		{
			return _appliedEthereal;
		}
		set
		{
			AssertMutable();
			_appliedEthereal = value;
		}
	}

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
		if (AppliedEthereal)
		{
			CardCmd.RemoveKeyword(base.Card, CardKeyword.Ethereal);
		}
		CardCmd.ClearAffliction(base.Card);
		return Task.CompletedTask;
	}
}
