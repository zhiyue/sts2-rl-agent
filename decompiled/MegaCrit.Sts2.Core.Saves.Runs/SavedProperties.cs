using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace MegaCrit.Sts2.Core.Saves.Runs;

[Serializable]
public class SavedProperties : IPacketSerializable
{
	public struct SavedProperty<T>
	{
		public string name;

		public T value;

		public SavedProperty(string name, T value)
		{
			this.name = name;
			this.value = value;
		}
	}

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("ints")]
	public List<SavedProperty<int>>? ints;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("bools")]
	public List<SavedProperty<bool>>? bools;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("strings")]
	public List<SavedProperty<string>>? strings;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("int_arrays")]
	public List<SavedProperty<int[]>>? intArrays;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("model_ids")]
	public List<SavedProperty<ModelId>>? modelIds;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("cards")]
	public List<SavedProperty<SerializableCard>>? cards;

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonPropertyName("card_arrays")]
	public List<SavedProperty<SerializableCard[]>>? cardArrays;

	public static SavedProperties? From(AbstractModel model)
	{
		return FromInternal(model, model.Id);
	}

	public static SavedProperties? FromInternal(object model, ModelId? id)
	{
		SavedProperties savedProperties = new SavedProperties();
		foreach (PropertyInfo item in SavedPropertiesTypeCache.GetJsonPropertiesForType(model.GetType()) ?? new List<PropertyInfo>())
		{
			string name = item.Name;
			object value = item.GetValue(model);
			SavedPropertyAttribute customAttribute = item.GetCustomAttribute<SavedPropertyAttribute>();
			if (!customAttribute.defaultBehaviour.ShouldSerialize(value, item))
			{
				continue;
			}
			if (value == null)
			{
				Log.Warn($"Property {name} on {id} is null, which is not a valid SavedProperty");
			}
			else if (!(value is int value2))
			{
				if (!(value is int[] value3))
				{
					if (!(value is Enum value4))
					{
						if (!(value is Enum[] source))
						{
							if (!(value is ModelId value5))
							{
								if (!(value is bool value6))
								{
									if (!(value is string value7))
									{
										if (!(value is SerializableCard value8))
										{
											if (!(value is List<SerializableCard> list))
											{
												throw new JsonException($"Property {name} on {id} is not a valid type for [SavedProperty] (type {value?.GetType()}).");
											}
											SavedProperties savedProperties2 = savedProperties;
											if (savedProperties2.cardArrays == null)
											{
												savedProperties2.cardArrays = new List<SavedProperty<SerializableCard[]>>();
											}
											savedProperties.cardArrays.Add(new SavedProperty<SerializableCard[]>(name, list.ToArray()));
										}
										else
										{
											SavedProperties savedProperties2 = savedProperties;
											if (savedProperties2.cards == null)
											{
												savedProperties2.cards = new List<SavedProperty<SerializableCard>>();
											}
											savedProperties.cards.Add(new SavedProperty<SerializableCard>(name, value8));
										}
									}
									else
									{
										SavedProperties savedProperties2 = savedProperties;
										if (savedProperties2.strings == null)
										{
											savedProperties2.strings = new List<SavedProperty<string>>();
										}
										savedProperties.strings.Add(new SavedProperty<string>(name, value7));
									}
								}
								else
								{
									SavedProperties savedProperties2 = savedProperties;
									if (savedProperties2.bools == null)
									{
										savedProperties2.bools = new List<SavedProperty<bool>>();
									}
									savedProperties.bools.Add(new SavedProperty<bool>(name, value6));
								}
							}
							else
							{
								SavedProperties savedProperties2 = savedProperties;
								if (savedProperties2.modelIds == null)
								{
									savedProperties2.modelIds = new List<SavedProperty<ModelId>>();
								}
								savedProperties.modelIds.Add(new SavedProperty<ModelId>(name, value5));
							}
						}
						else
						{
							SavedProperties savedProperties2 = savedProperties;
							if (savedProperties2.intArrays == null)
							{
								savedProperties2.intArrays = new List<SavedProperty<int[]>>();
							}
							savedProperties.intArrays.Add(new SavedProperty<int[]>(name, source.Select(Convert.ToInt32).ToArray()));
						}
					}
					else
					{
						SavedProperties savedProperties2 = savedProperties;
						if (savedProperties2.ints == null)
						{
							savedProperties2.ints = new List<SavedProperty<int>>();
						}
						savedProperties.ints.Add(new SavedProperty<int>(name, Convert.ToInt32(value4)));
					}
				}
				else
				{
					SavedProperties savedProperties2 = savedProperties;
					if (savedProperties2.intArrays == null)
					{
						savedProperties2.intArrays = new List<SavedProperty<int[]>>();
					}
					savedProperties.intArrays.Add(new SavedProperty<int[]>(name, value3));
				}
			}
			else
			{
				SavedProperties savedProperties2 = savedProperties;
				if (savedProperties2.ints == null)
				{
					savedProperties2.ints = new List<SavedProperty<int>>();
				}
				savedProperties.ints.Add(new SavedProperty<int>(name, value2));
			}
		}
		if (!savedProperties.Any())
		{
			return null;
		}
		return savedProperties;
	}

	private bool Any()
	{
		if (ints != null)
		{
			List<SavedProperty<int>>? list = ints;
			if (list == null || list.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (bools != null)
		{
			List<SavedProperty<bool>>? list2 = bools;
			if (list2 == null || list2.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (intArrays != null)
		{
			List<SavedProperty<int[]>>? list3 = intArrays;
			if (list3 == null || list3.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (strings != null)
		{
			List<SavedProperty<string>>? list4 = strings;
			if (list4 == null || list4.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (modelIds != null)
		{
			List<SavedProperty<ModelId>>? list5 = modelIds;
			if (list5 == null || list5.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (cards != null)
		{
			List<SavedProperty<SerializableCard>>? list6 = cards;
			if (list6 == null || list6.Count != 0)
			{
				goto IL_00df;
			}
		}
		if (cardArrays != null)
		{
			List<SavedProperty<SerializableCard[]>>? list7 = cardArrays;
			if (list7 == null)
			{
				return true;
			}
			return list7.Count != 0;
		}
		return false;
		IL_00df:
		return true;
	}

	public void Fill(AbstractModel model)
	{
		FillInternal(model);
	}

	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "We only create array types that are referenced by SavedProperties that already exist in code.")]
	public void FillInternal(object model)
	{
		Type type = model.GetType();
		if (ints != null)
		{
			foreach (SavedProperty<int> @int in ints)
			{
				PropertyInfo property = type.GetProperty(@int.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				property?.SetValue(model, property.PropertyType.IsEnum ? Enum.ToObject(property.PropertyType, @int.value) : ((object)@int.value));
			}
		}
		if (intArrays != null)
		{
			foreach (SavedProperty<int[]> intArray in intArrays)
			{
				PropertyInfo property2 = type.GetProperty(intArray.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (property2 == null)
				{
					continue;
				}
				Type elementType = property2.PropertyType.GetElementType();
				if (elementType.IsEnum)
				{
					Array array = Array.CreateInstance(elementType, intArray.value.Length);
					for (int i = 0; i < intArray.value.Length; i++)
					{
						array.SetValue(Enum.ToObject(elementType, intArray.value[i]), i);
					}
					property2.SetValue(model, array);
				}
				else
				{
					property2.SetValue(model, intArray.value);
				}
			}
		}
		if (bools != null)
		{
			foreach (SavedProperty<bool> @bool in bools)
			{
				type.GetProperty(@bool.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(model, @bool.value);
			}
		}
		if (modelIds != null)
		{
			foreach (SavedProperty<ModelId> modelId in modelIds)
			{
				type.GetProperty(modelId.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(model, modelId.value);
			}
		}
		if (cards != null)
		{
			foreach (SavedProperty<SerializableCard> card in cards)
			{
				type.GetProperty(card.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(model, card.value);
			}
		}
		if (cardArrays != null)
		{
			foreach (SavedProperty<SerializableCard[]> cardArray in cardArrays)
			{
				type.GetProperty(cardArray.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(model, cardArray.value.ToList());
			}
		}
		if (strings == null)
		{
			return;
		}
		foreach (SavedProperty<string> @string in strings)
		{
			type.GetProperty(@string.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(model, @string.value);
		}
	}

	private static void WritePropertyName(PacketWriter writer, string propertyName)
	{
		writer.WriteInt(SavedPropertiesTypeCache.GetNetIdForPropertyName(propertyName), SavedPropertiesTypeCache.NetIdBitSize);
	}

	private static string ReadPropertyName(PacketReader reader)
	{
		int netId = reader.ReadInt(SavedPropertiesTypeCache.NetIdBitSize);
		return SavedPropertiesTypeCache.GetPropertyNameForNetId(netId);
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteBool(ints != null);
		if (ints != null)
		{
			writer.WriteInt(ints.Count, 8);
			foreach (SavedProperty<int> @int in ints)
			{
				WritePropertyName(writer, @int.name);
				writer.WriteInt(@int.value);
			}
		}
		writer.WriteBool(intArrays != null);
		if (intArrays != null)
		{
			writer.WriteInt(intArrays.Count, 8);
			foreach (SavedProperty<int[]> intArray in intArrays)
			{
				WritePropertyName(writer, intArray.name);
				writer.WriteInt(intArray.value.Length);
				int[] value = intArray.value;
				foreach (int val in value)
				{
					writer.WriteInt(val);
				}
			}
		}
		writer.WriteBool(bools != null);
		if (bools != null)
		{
			writer.WriteInt(bools.Count, 8);
			foreach (SavedProperty<bool> @bool in bools)
			{
				WritePropertyName(writer, @bool.name);
				writer.WriteBool(@bool.value);
			}
		}
		writer.WriteBool(modelIds != null);
		if (modelIds != null)
		{
			writer.WriteInt(modelIds.Count, 8);
			foreach (SavedProperty<ModelId> modelId in modelIds)
			{
				WritePropertyName(writer, modelId.name);
				writer.WriteFullModelId(modelId.value);
			}
		}
		writer.WriteBool(cards != null);
		if (cards != null)
		{
			writer.WriteInt(cards.Count, 8);
			foreach (SavedProperty<SerializableCard> card in cards)
			{
				WritePropertyName(writer, card.name);
				writer.Write(card.value);
			}
		}
		writer.WriteBool(cardArrays != null);
		if (cardArrays != null)
		{
			writer.WriteInt(cardArrays.Count, 8);
			foreach (SavedProperty<SerializableCard[]> cardArray in cardArrays)
			{
				WritePropertyName(writer, cardArray.name);
				writer.WriteInt(cardArray.value.Length);
				SerializableCard[] value2 = cardArray.value;
				foreach (SerializableCard val2 in value2)
				{
					writer.Write(val2);
				}
			}
		}
		writer.WriteBool(strings != null);
		if (strings == null)
		{
			return;
		}
		writer.WriteInt(strings.Count, 8);
		foreach (SavedProperty<string> @string in strings)
		{
			WritePropertyName(writer, @string.name);
			writer.WriteString(@string.value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		if (reader.ReadBool())
		{
			ints = new List<SavedProperty<int>>();
			int num = reader.ReadInt(8);
			for (int i = 0; i < num; i++)
			{
				ints.Add(new SavedProperty<int>(ReadPropertyName(reader), reader.ReadInt()));
			}
		}
		if (reader.ReadBool())
		{
			intArrays = new List<SavedProperty<int[]>>();
			int num2 = reader.ReadInt(8);
			for (int j = 0; j < num2; j++)
			{
				string name = ReadPropertyName(reader);
				int num3 = reader.ReadInt();
				int[] array = new int[num3];
				for (int k = 0; k < num3; k++)
				{
					array[k] = reader.ReadInt();
				}
				intArrays.Add(new SavedProperty<int[]>(name, array));
			}
		}
		if (reader.ReadBool())
		{
			bools = new List<SavedProperty<bool>>();
			int num4 = reader.ReadInt(8);
			for (int l = 0; l < num4; l++)
			{
				bools.Add(new SavedProperty<bool>(ReadPropertyName(reader), reader.ReadBool()));
			}
		}
		if (reader.ReadBool())
		{
			modelIds = new List<SavedProperty<ModelId>>();
			int num5 = reader.ReadInt(8);
			for (int m = 0; m < num5; m++)
			{
				modelIds.Add(new SavedProperty<ModelId>(ReadPropertyName(reader), reader.ReadFullModelId()));
			}
		}
		if (reader.ReadBool())
		{
			cards = new List<SavedProperty<SerializableCard>>();
			int num6 = reader.ReadInt(8);
			for (int n = 0; n < num6; n++)
			{
				cards.Add(new SavedProperty<SerializableCard>(ReadPropertyName(reader), reader.Read<SerializableCard>()));
			}
		}
		if (reader.ReadBool())
		{
			cardArrays = new List<SavedProperty<SerializableCard[]>>();
			int num7 = reader.ReadInt(8);
			for (int num8 = 0; num8 < num7; num8++)
			{
				string name2 = ReadPropertyName(reader);
				int num9 = reader.ReadInt();
				SerializableCard[] array2 = new SerializableCard[num9];
				for (int num10 = 0; num10 < num9; num10++)
				{
					array2[num10] = reader.Read<SerializableCard>();
				}
				cardArrays.Add(new SavedProperty<SerializableCard[]>(name2, array2));
			}
		}
		if (reader.ReadBool())
		{
			strings = new List<SavedProperty<string>>();
			int num11 = reader.ReadInt(8);
			for (int num12 = 0; num12 < num11; num12++)
			{
				strings.Add(new SavedProperty<string>(ReadPropertyName(reader), reader.ReadString()));
			}
		}
	}

	public override string ToString()
	{
		if (!Any())
		{
			return "<none>";
		}
		List<string> list = new List<string>();
		if (ints != null)
		{
			foreach (SavedProperty<int> @int in ints)
			{
				list.Add($"{@int.name}={@int.value}");
			}
		}
		if (bools != null)
		{
			foreach (SavedProperty<bool> @bool in bools)
			{
				list.Add($"{@bool.name}={@bool.value}");
			}
		}
		if (strings != null)
		{
			foreach (SavedProperty<string> @string in strings)
			{
				list.Add(@string.name + "=" + @string.value);
			}
		}
		if (intArrays != null)
		{
			foreach (SavedProperty<int[]> intArray in intArrays)
			{
				list.Add(intArray.name + "=" + string.Join(",", intArray.value));
			}
		}
		if (modelIds != null)
		{
			foreach (SavedProperty<ModelId> modelId in modelIds)
			{
				list.Add($"{modelId.name}={modelId.value}");
			}
		}
		if (cards != null)
		{
			foreach (SavedProperty<SerializableCard> card in cards)
			{
				list.Add($"{card.name}={card.value}");
			}
		}
		if (cardArrays != null)
		{
			foreach (SavedProperty<SerializableCard[]> cardArray in cardArrays)
			{
				list.Add(cardArray.name + "=" + string.Join(",", cardArray.value.Select((SerializableCard c) => c.Id.Entry)));
			}
		}
		return string.Join(",", list);
	}
}
