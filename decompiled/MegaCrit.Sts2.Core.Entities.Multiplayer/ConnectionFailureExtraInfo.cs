using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

/// <summary>
/// Class for holding extra info to display in multiplayer errors if the error is a ConnectionFailureReason.
/// </summary>
public record ConnectionFailureExtraInfo
{
	/// <summary>
	/// The mods that the host has that we are missing on our end.
	/// Only set on ConnectionFailureReason.ModMismatch.
	/// </summary>
	public List<string>? missingModsOnLocal;

	/// <summary>
	/// The mods that we have that are missing on the host's end.
	/// Only set on ConnectionFailureReason.ModMismatch.
	/// </summary>
	public List<string>? missingModsOnHost;
}
