using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[Tool]
[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NJoinFriendScreenButtonLayout.cs")]
public class NJoinFriendScreenButtonLayout : Container
{
	public new class MethodName : Container.MethodName
	{
		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName LayoutChildren = "LayoutChildren";
	}

	public new class PropertyName : Container.PropertyName
	{
	}

	public new class SignalName : Container.SignalName
	{
	}

	public override void _Notification(int what)
	{
		if ((long)what == 51)
		{
			LayoutChildren();
		}
	}

	private void LayoutChildren()
	{
		Control[] array = GetChildren().OfType<Control>().ToArray();
		if (array.Length != 0)
		{
			Vector2 size = array[0].Size;
			int num = (int)(base.Size.Y / size.Y);
			int num2 = (int)Math.Ceiling((float)array.Length / (float)num);
			float num3 = (base.Size.X - (float)num2 * size.X) * 0.5f;
			for (int i = 0; i < array.Length; i++)
			{
				int num4 = i / num2;
				int num5 = i - num4 * num2;
				array[i].Position = new Vector2(num5, num4) * size + Vector2.Right * num3;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.LayoutChildren, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LayoutChildren && args.Count == 0)
		{
			LayoutChildren();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.LayoutChildren)
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
