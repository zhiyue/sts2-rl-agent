using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;

[ScriptPath("res://src/Core/Nodes/Screens/PotionLab/NPotionLabCategory.cs")]
public class NPotionLabCategory : VBoxContainer
{
	public new class MethodName : VBoxContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ClearPotions = "ClearPotions";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _headerLabel = "_headerLabel";

		public static readonly StringName _potionContainer = "_potionContainer";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
	}

	private MegaRichTextLabel _headerLabel;

	private GridContainer _potionContainer;

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_potionContainer.GetChildCount() <= 0)
			{
				return null;
			}
			return _potionContainer.GetChild<Control>(0);
		}
	}

	public override void _Ready()
	{
		_headerLabel = GetNode<MegaRichTextLabel>("Header");
		_potionContainer = GetNode<GridContainer>("%PotionsContainer");
	}

	public void LoadPotions(PotionRarity potionRarity, LocString header, HashSet<PotionModel> seenPotions, UnlockState unlockState, HashSet<PotionModel> allUnlockedPotions, PotionRarity? secondRarity = null)
	{
		_headerLabel.Text = header.GetFormattedText();
		IEnumerable<PotionModel> enumerable = ModelDb.AllPotions.Where((PotionModel relic) => relic.Rarity == potionRarity || relic.Rarity == secondRarity);
		List<PotionModel> list = new List<PotionModel>();
		List<PotionModel> list2 = new List<PotionModel>();
		foreach (PotionPoolModel allCharacterPotionPool in ModelDb.AllCharacterPotionPools)
		{
			foreach (PotionModel item in enumerable)
			{
				if (allCharacterPotionPool.AllPotionIds.Contains(item.Id))
				{
					list.Add(item);
				}
			}
		}
		foreach (PotionModel item2 in enumerable)
		{
			if (!list.Contains(item2))
			{
				list2.Add(item2);
			}
		}
		StringComparer comparer = StringComparer.Create(LocManager.Instance.CultureInfo, CompareOptions.None);
		list2.Sort((PotionModel p1, PotionModel p2) => comparer.Compare(p1.Title.GetFormattedText(), p2.Title.GetFormattedText()));
		foreach (PotionModel item3 in list2.Concat(list))
		{
			ModelVisibility visibility = ((!allUnlockedPotions.Contains(item3)) ? ModelVisibility.Locked : (seenPotions.Contains(item3) ? ModelVisibility.Visible : ModelVisibility.NotSeen));
			NLabPotionHolder child = NLabPotionHolder.Create(item3.ToMutable(), visibility);
			_potionContainer.AddChildSafely(child);
		}
	}

	public void ClearPotions()
	{
		foreach (Node child in _potionContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
	}

	public List<IReadOnlyList<Control>> GetGridItems()
	{
		List<IReadOnlyList<Control>> list = new List<IReadOnlyList<Control>>();
		for (int i = 0; i < _potionContainer.GetChildren().Count; i += _potionContainer.Columns)
		{
			list.Add(_potionContainer.GetChildren().OfType<Control>().Skip(i)
				.Take(_potionContainer.Columns)
				.ToList());
		}
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearPotions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearPotions && args.Count == 0)
		{
			ClearPotions();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ClearPotions)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._headerLabel)
		{
			_headerLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			_potionContainer = VariantUtils.ConvertTo<GridContainer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._headerLabel)
		{
			value = VariantUtils.CreateFrom(in _headerLabel);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			value = VariantUtils.CreateFrom(in _potionContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._headerLabel, Variant.From(in _headerLabel));
		info.AddProperty(PropertyName._potionContainer, Variant.From(in _potionContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._headerLabel, out var value))
		{
			_headerLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._potionContainer, out var value2))
		{
			_potionContainer = value2.As<GridContainer>();
		}
	}
}
