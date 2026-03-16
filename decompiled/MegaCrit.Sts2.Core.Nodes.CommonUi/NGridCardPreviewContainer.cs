using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NGridCardPreviewContainer.cs")]
public class NGridCardPreviewContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ForceMaxColumnsUntilEmpty = "ForceMaxColumnsUntilEmpty";

		public static readonly StringName ReformatElements = "ReformatElements";

		public static readonly StringName CheckAnyChildrenPresent = "CheckAnyChildrenPresent";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

	public new class SignalName : Control.SignalName
	{
	}

	private int? _forcedMaxColumns;

	public override void _Ready()
	{
		Connect(Node.SignalName.ChildEnteredTree, Callable.From<Node>(ReformatElements));
		Connect(Node.SignalName.ChildExitingTree, Callable.From<Node>(CheckAnyChildrenPresent));
	}

	public void ForceMaxColumnsUntilEmpty(int maxColumns)
	{
		_forcedMaxColumns = maxColumns;
	}

	private void ReformatElements(Node _)
	{
		Vector2 vector = base.Size / 2f + Vector2.Down * 50f;
		int childCount = GetChildCount();
		int num = Mathf.FloorToInt(GetViewportRect().Size.X / (NCard.defaultSize.X + 25f));
		int num2 = Mathf.CeilToInt((float)childCount / (float)num);
		if (_forcedMaxColumns.HasValue)
		{
			num = Math.Min(num, _forcedMaxColumns.Value);
		}
		for (int i = 0; i < childCount; i++)
		{
			int num3 = i / num;
			int num4 = i % num;
			int num5 = Math.Min(num, childCount - num3 * num);
			float num6 = (float)(-(num2 - 1)) * (NCard.defaultSize.Y + 25f) * 0.5f;
			float num7 = (float)(-(num5 - 1)) * (NCard.defaultSize.X + 25f) * 0.5f;
			Vector2 vector2 = vector;
			Vector2 position = vector2 + new Vector2(num7 + (NCard.defaultSize.X + 25f) * (float)num4, num6 + (NCard.defaultSize.Y + 25f) * (float)num3);
			Node child = GetChild(i);
			if (child is Node2D node2D)
			{
				node2D.Position = position;
			}
			else
			{
				((Control)child).Position = position;
			}
		}
	}

	private void CheckAnyChildrenPresent(Node _)
	{
		if (GetChildCount() == 0)
		{
			_forcedMaxColumns = null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ForceMaxColumnsUntilEmpty, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "maxColumns", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ReformatElements, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CheckAnyChildrenPresent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
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
		if (method == MethodName.ForceMaxColumnsUntilEmpty && args.Count == 1)
		{
			ForceMaxColumnsUntilEmpty(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReformatElements && args.Count == 1)
		{
			ReformatElements(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CheckAnyChildrenPresent && args.Count == 1)
		{
			CheckAnyChildrenPresent(VariantUtils.ConvertTo<Node>(in args[0]));
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
		if (method == MethodName.ForceMaxColumnsUntilEmpty)
		{
			return true;
		}
		if (method == MethodName.ReformatElements)
		{
			return true;
		}
		if (method == MethodName.CheckAnyChildrenPresent)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
