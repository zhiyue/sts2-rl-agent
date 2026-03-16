using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Timeline.Stories;

namespace MegaCrit.Sts2.Core.Timeline;

public static class StoryModelSubtypes
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t0 = typeof(DefectStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t1 = typeof(IroncladStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t2 = typeof(MagnumOpusStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t3 = typeof(NecrobinderStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t4 = typeof(RegentStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t5 = typeof(SilentStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t6 = typeof(TalesFromTheSpireStory);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	private static readonly Type _t7 = typeof(TheReopeningStory);

	private static readonly Type[] _subtypes = new Type[8] { _t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7 };

	public static int Count => 8;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicProperties)]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
