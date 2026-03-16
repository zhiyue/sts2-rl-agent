using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class JsonSerializeConditionAttribute : Attribute
{
	public readonly SerializationCondition defaultBehaviour;

	public JsonSerializeConditionAttribute(SerializationCondition defaultBehaviour)
	{
		this.defaultBehaviour = defaultBehaviour;
	}

	public static void CheckJsonSerializeConditionsModifier(JsonTypeInfo typeInfo)
	{
		if (typeInfo.Kind != JsonTypeInfoKind.Object)
		{
			return;
		}
		foreach (JsonPropertyInfo property in typeInfo.Properties)
		{
			JsonSerializeConditionAttribute attr = property.AttributeProvider?.GetCustomAttributes(inherit: true).OfType<JsonSerializeConditionAttribute>().FirstOrDefault();
			if (attr == null)
			{
				continue;
			}
			ICustomAttributeProvider attributeProvider = property.AttributeProvider;
			MemberInfo memberInfo = attributeProvider as MemberInfo;
			if ((object)memberInfo != null)
			{
				property.ShouldSerialize = (object _, object? c) => attr.defaultBehaviour.ShouldSerialize(c, memberInfo);
			}
		}
	}
}
