using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Saves.Migrations.PrefsSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.ProfileSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.ProgressSaves;
using MegaCrit.Sts2.Core.Saves.Migrations.RunHistories;
using MegaCrit.Sts2.Core.Saves.Migrations.SerializableRuns;
using MegaCrit.Sts2.Core.Saves.Migrations.SettingsSaves;

namespace MegaCrit.Sts2.Core.Saves.Migrations;

public static class IMigrationSubtypes
{
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t0 = typeof(PrefsSaveV1ToV2);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t1 = typeof(ProfileSaveV1ToV2);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t2 = typeof(ProgressSaveV20ToV21);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t3 = typeof(RunHistoryV7ToV8);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t4 = typeof(SerializableRunV12ToV13);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t5 = typeof(SerializableRunV13ToV14);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t6 = typeof(SettingsSaveV3ToV4);

	private static readonly Type[] _subtypes = new Type[7] { _t0, _t1, _t2, _t3, _t4, _t5, _t6 };

	public static int Count => 7;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
