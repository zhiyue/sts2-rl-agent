using System;
using System.Diagnostics.CodeAnalysis;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

public static class BootstrapSettingsUtil
{
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public static Type? Get()
	{
		if (IBootstrapSettingsSubtypes.Count > 0)
		{
			return IBootstrapSettingsSubtypes.Get(0);
		}
		return null;
	}
}
