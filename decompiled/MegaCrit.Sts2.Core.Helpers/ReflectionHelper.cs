using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Modding;

namespace MegaCrit.Sts2.Core.Helpers;

/// <summary>
/// A few utility methods for accessing types via reflection.
/// In general, you should avoid using this, as it does not work on console platforms (or anywhere that NativeAOT is
/// required). You can check for support with SubtypesAvailable.
/// Only use this in scenarios where it is required (e.g. for modding).
/// </summary>
public static class ReflectionHelper
{
	public const BindingFlags allAccessLevels = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static Type[]? _allTypes;

	private static Type[]? _modTypes;

	public static bool SubtypesAvailable => true;

	public static Type[] AllTypes
	{
		get
		{
			if (_allTypes == null)
			{
				_allTypes = Assembly.GetAssembly(typeof(ReflectionHelper)).GetTypes();
			}
			return _allTypes;
		}
	}

	public static Type[] ModTypes
	{
		get
		{
			if (ModManager.State == ModManagerState.None)
			{
				throw new InvalidOperationException("ModManager is not finished initializing! ReflectionHelper.ModTypes cannot be called before that.");
			}
			if (_modTypes == null)
			{
				_modTypes = (from m in ModManager.GetLoadedMods()
					select m.assembly).OfType<Assembly>().SelectMany((Assembly a) => a.GetTypes()).ToArray();
			}
			return _modTypes;
		}
	}

	public static IEnumerable<Type> GetSubtypes(Type parentType)
	{
		return GetSubtypesFromList(AllTypes, parentType);
	}

	public static IEnumerable<Type> GetSubtypesInMods(Type parentType)
	{
		return GetSubtypesFromList(ModTypes, parentType);
	}

	public static IEnumerable<Type> GetSubtypesFromAssembly(Assembly assembly, Type parentType)
	{
		return GetSubtypesFromList(assembly.GetTypes(), parentType);
	}

	private static IEnumerable<Type> GetSubtypesFromList(IList<Type> list, Type parentType)
	{
		return list.Where((Type type) => (object)type != null && !type.IsAbstract && !type.IsInterface && InheritsOrImplements(type, parentType));
	}

	public static IEnumerable<Type> GetSubtypes<T>() where T : class
	{
		return GetSubtypes(typeof(T));
	}

	public static IEnumerable<Type> GetSubtypesInMods<T>() where T : class
	{
		return GetSubtypesInMods(typeof(T));
	}

	public static bool InheritsOrImplements([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type derived, Type baseType)
	{
		if (!derived.IsSubclassOf(baseType))
		{
			return derived.GetInterfaces().Contains(baseType);
		}
		return true;
	}
}
