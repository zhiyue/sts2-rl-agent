namespace MegaCrit.Sts2.Core.Platform;

public static class PlatformBranchExtensions
{
	/// <summary>
	/// Prefer over ToString as this gives the actual branch name as displayed in Steam or other platform
	/// </summary>
	public static string ToName(this PlatformBranch branch)
	{
		switch (branch)
		{
		case PlatformBranch.None:
			return "none";
		case PlatformBranch.DevTest:
			return "dev-test";
		case PlatformBranch.PrivateBeta:
			return "private-beta";
		case PlatformBranch.PublicBeta:
			return "public-beta";
		case PlatformBranch.Production:
			return "public";
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(branch);
			string result = default(string);
			return result;
		}
		}
	}

	public static PlatformBranch? FromName(string name)
	{
		return name switch
		{
			"dev-test" => PlatformBranch.DevTest, 
			"private-beta" => PlatformBranch.PrivateBeta, 
			"public-beta" => PlatformBranch.PublicBeta, 
			"public" => PlatformBranch.Production, 
			_ => null, 
		};
	}
}
