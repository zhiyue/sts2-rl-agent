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
	private static readonly Type _t4 = typeof(RunHistoryV8ToV9);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t5 = typeof(SerializableRunV12ToV13);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t6 = typeof(SerializableRunV13ToV14);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t7 = typeof(SerializableRunV14ToV15);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t8 = typeof(SerializableRunV15ToV16);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t9 = typeof(SettingsSaveV3ToV4);

	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static readonly Type _t10 = typeof(SettingsSaveV4ToV5);

	private static readonly Type[] _subtypes = new Type[11]
	{
		_t0, _t1, _t2, _t3, _t4, _t5, _t6, _t7, _t8, _t9,
		_t10
	};

	public static int Count => 11;

	public static IReadOnlyList<Type> All => _subtypes;

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2063", Justification = "The list only contains types stored with the correct DynamicallyAccessedMembers attribute, enforced by source generation.")]
	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	public static Type Get(int i)
	{
		return _subtypes[i];
	}
}
