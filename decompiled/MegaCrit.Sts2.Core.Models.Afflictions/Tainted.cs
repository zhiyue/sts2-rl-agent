using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace MegaCrit.Sts2.Core.Models.Afflictions;

/// <summary>
/// Most of this Affliction's logic lives in <see cref="T:MegaCrit.Sts2.Core.Models.Powers.TaintedPower" />.
/// </summary>
public sealed class Tainted : AfflictionModel
{
	public override bool IsStackable => true;

	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<TaintedPower>(base.Amount);

	public override bool CanAfflictCardType(CardType cardType)
	{
		return cardType == CardType.Skill;
	}
}
