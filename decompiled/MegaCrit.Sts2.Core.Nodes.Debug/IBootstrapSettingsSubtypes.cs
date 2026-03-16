using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

public static class IBootstrapSettingsSubtypes
{
	private static readonly Type[] _subtypes = Array.Empty<Type>();

	public static int Count => 0;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
