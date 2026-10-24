using System;

namespace MegaCrit.Sts2.Core.Hooks;

/// <summary>
/// Represents which phase(s) of HP-loss-modification hooks should be run for a given HP loss.
/// In CreatureCmd.Damage, the two phases bracket damage redirection (Osty), so they are invoked separately with
/// potentially different targets. Callers that don't deal with redirection (like damage previews) can use
/// <see cref="F:MegaCrit.Sts2.Core.Hooks.HpLossHookPhase.All" /> to apply both phases at once.
/// </summary>
[Flags]
public enum HpLossHookPhase
{
	None = 0,
	/// <summary>
	/// Hooks that modify HP loss before damage is redirected from Necrobinder to Osty.
	/// Including this will cause <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostBeforeOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostBeforeOstyLate(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> to be called.
	/// </summary>
	BeforeOsty = 1,
	/// <summary>
	/// Hooks that modify HP loss after damage is redirected from Necrobinder to Osty.
	/// Including this will cause <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostAfterOsty(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> and
	/// <see cref="M:MegaCrit.Sts2.Core.Models.AbstractModel.ModifyHpLostAfterOstyLate(MegaCrit.Sts2.Core.Entities.Creatures.Creature,System.Decimal,MegaCrit.Sts2.Core.ValueProps.ValueProp,MegaCrit.Sts2.Core.Entities.Creatures.Creature,MegaCrit.Sts2.Core.Models.CardModel)" /> to be called.
	/// </summary>
	AfterOsty = 2,
	/// <summary>
	/// Include all HP-loss-modification hooks. Use when there is no redirection step between phases
	/// (e.g. damage previews).
	/// </summary>
	All = 3
}
