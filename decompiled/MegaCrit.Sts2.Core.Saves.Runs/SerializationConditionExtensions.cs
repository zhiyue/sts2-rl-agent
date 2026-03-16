using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public static class SerializationConditionExtensions
{
	private static readonly Dictionary<Type, object?> _defaultTypeCache = new Dictionary<Type, object>();

	private static object? GetTypeDefaultValue([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type t)
	{
		if (t.IsValueType)
		{
			return Activator.CreateInstance(t);
		}
		return null;
	}

	private static object? GetMemberDefaultValue(MemberInfo info)
	{
		if (!_defaultTypeCache.TryGetValue(info.DeclaringType, out object value))
		{
			value = Activator.CreateInstance(info.DeclaringType);
			_defaultTypeCache[info.DeclaringType] = value;
		}
		return info.MemberType switch
		{
			MemberTypes.Field => ((FieldInfo)info).GetValue(value), 
			MemberTypes.Property => ((PropertyInfo)info).GetValue(value), 
			_ => throw new ArgumentException($"Input MemberInfo must be of type FieldInfo or PropertyInfo. It was {info.MemberType} ({info.GetType()})"), 
		};
	}

	[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
	private static Type GetUnderlyingType(this MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			_ => throw new ArgumentException($"Input MemberInfo must be of type FieldInfo or PropertyInfo. It was {member.MemberType} ({member.GetType()})"), 
		};
	}

	public static bool ShouldSerialize(this SerializationCondition condition, object? candidate, MemberInfo memberInfo)
	{
		switch (condition)
		{
		case SerializationCondition.AlwaysSave:
			return true;
		case SerializationCondition.SaveIfNotPropertyDefault:
			return !object.Equals(candidate, GetMemberDefaultValue(memberInfo));
		case SerializationCondition.SaveIfNotTypeDefault:
			return !object.Equals(candidate, GetTypeDefaultValue(memberInfo.GetUnderlyingType()));
		case SerializationCondition.SaveIfNotCollectionEmptyOrNull:
			if (!(candidate is ICollection collection))
			{
				return false;
			}
			if (collection.Count == 0)
			{
				return false;
			}
			return true;
		default:
			throw new InvalidEnumArgumentException("Invalid condition passed!");
		}
	}
}
