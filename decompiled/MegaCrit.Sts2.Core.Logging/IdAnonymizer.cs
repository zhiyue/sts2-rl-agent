using System.Collections.Generic;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Logging;

public static class IdAnonymizer
{
	private static readonly Dictionary<ulong, ulong> _idToAnonymized = new Dictionary<ulong, ulong>();

	public static ulong Anonymize(ulong id)
	{
		if (!_idToAnonymized.TryGetValue(id, out var value))
		{
			value = Rng.Chaotic.NextUnsignedInt();
			_idToAnonymized[id] = value;
		}
		return value;
	}
}
