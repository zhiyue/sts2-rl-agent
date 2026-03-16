using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public record ConnectionFailureExtraInfo
{
	public List<string>? missingModsOnLocal;

	public List<string>? missingModsOnHost;
}
