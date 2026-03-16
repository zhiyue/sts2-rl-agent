using System;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Localization;

public struct SerializableDynamicVar : IPacketSerializable
{
	[JsonPropertyName("type")]
	public DynamicVarType type;

	[JsonPropertyName("decimal_value")]
	public decimal decimalValue;

	[JsonPropertyName("bool_value")]
	public bool boolValue;

	[JsonPropertyName("string_value")]
	[JsonSerializeCondition(SerializationCondition.SaveIfNotTypeDefault)]
	public string? stringValue;

	public object ToDynamicVar(string name)
	{
		return type switch
		{
			DynamicVarType.DynamicString => new StringVar(name, stringValue), 
			DynamicVarType.BaseDynamic => new DynamicVar(name, decimalValue), 
			DynamicVarType.Decimal => decimalValue, 
			DynamicVarType.String => stringValue, 
			DynamicVarType.Bool => boolValue, 
			_ => throw new InvalidOperationException($"Tried to convert SerializableDynamicVar with invalid type {type}"), 
		};
	}

	public static SerializableDynamicVar? FromDynamicVar(object var)
	{
		if (!(var is StringVar stringVar))
		{
			if (!(var is DynamicVar dynamicVar))
			{
				if (!(var is string text))
				{
					if (!(var is decimal num))
					{
						if (var is bool flag)
						{
							return new SerializableDynamicVar
							{
								type = DynamicVarType.Bool,
								boolValue = flag
							};
						}
						return null;
					}
					return new SerializableDynamicVar
					{
						type = DynamicVarType.Decimal,
						decimalValue = num
					};
				}
				return new SerializableDynamicVar
				{
					type = DynamicVarType.String,
					stringValue = text
				};
			}
			return new SerializableDynamicVar
			{
				type = DynamicVarType.BaseDynamic,
				decimalValue = dynamicVar.BaseValue
			};
		}
		return new SerializableDynamicVar
		{
			type = DynamicVarType.DynamicString,
			stringValue = stringVar.StringValue
		};
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteEnum(type);
		switch (type)
		{
		case DynamicVarType.DynamicString:
		case DynamicVarType.String:
			writer.WriteString(stringValue);
			break;
		case DynamicVarType.BaseDynamic:
		case DynamicVarType.Decimal:
			writer.WriteFloat((float)decimalValue);
			break;
		case DynamicVarType.Bool:
			writer.WriteBool(boolValue);
			break;
		default:
			throw new InvalidOperationException($"Tried to convert SerializableDynamicVar with invalid type {type}");
		}
	}

	public void Deserialize(PacketReader reader)
	{
		type = reader.ReadEnum<DynamicVarType>();
		switch (type)
		{
		case DynamicVarType.DynamicString:
		case DynamicVarType.String:
			stringValue = reader.ReadString();
			break;
		case DynamicVarType.BaseDynamic:
		case DynamicVarType.Decimal:
			decimalValue = (decimal)reader.ReadFloat();
			break;
		case DynamicVarType.Bool:
			boolValue = reader.ReadBool();
			break;
		default:
			throw new InvalidOperationException($"Tried to convert SerializableDynamicVar with invalid type {type}");
		}
	}
}
