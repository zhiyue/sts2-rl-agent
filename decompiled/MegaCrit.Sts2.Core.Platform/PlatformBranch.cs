namespace MegaCrit.Sts2.Core.Platform;

/// <summary>
/// Represents one of the several branches that a player could be playing on.
///
/// The members are ordered from oldest build to newest build. Builds promote along the path
/// dev-test -&gt; private-beta -&gt; public-beta -&gt; public, reaching dev-test first and the public default branch last,
/// so dev-test carries the newest build and public the oldest. The enum lists branches in the reverse of that promotion
/// path. This order is relied on for Steam Workshop version-range comparisons, so it must match the branch order
/// configured on the Steamworks SteamPipe &gt; Builds page. None sorts below every real branch.
/// </summary>
public enum PlatformBranch
{
	/// <summary>
	/// Branch was unable to be determined. This can be returned in valid cases; for instance, when not running on any
	/// platform.
	/// </summary>
	None,
	/// <summary>
	/// Production branch. What most players are going to be playing.
	/// </summary>
	Production,
	/// <summary>
	/// Public beta branch. Accessible by the public, but they have to opt into it.
	/// </summary>
	PublicBeta,
	/// <summary>
	/// Private beta branch, i.e. what Byrdlett has access to.
	/// </summary>
	PrivateBeta,
	/// <summary>
	/// Dev test branch. Only accessible to devs.
	/// </summary>
	DevTest
}
