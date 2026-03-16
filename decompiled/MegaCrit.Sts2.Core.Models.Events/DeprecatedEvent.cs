using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Events;

namespace MegaCrit.Sts2.Core.Models.Events;

public sealed class DeprecatedEvent : EventModel
{
	protected override IReadOnlyList<EventOption> GenerateInitialOptions()
	{
		return Array.Empty<EventOption>();
	}
}
