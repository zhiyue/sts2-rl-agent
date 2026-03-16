using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DeprecatedAncientEvent : AncientEventModel
{
	public override IEnumerable<EventOption> AllPossibleOptions => Array.Empty<EventOption>();

	protected override AncientDialogueSet DefineDialogues()
	{
		throw new NotImplementedException();
	}

	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return Array.Empty<EventOption>();
	}
}
