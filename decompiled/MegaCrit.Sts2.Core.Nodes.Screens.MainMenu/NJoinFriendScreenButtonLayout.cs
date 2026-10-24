using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

/// <summary>
/// Lays out buttons centered in a vertical column. When that column is full, starts a new column and centers both.
/// This assumes that all children are the same size.
/// </summary>
[Tool]
[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NJoinFriendScreenButtonLayout.cs")]
public class NJoinFriendScreenButtonLayout : Container
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Container.MethodName
	{
		/// <summary>
		/// Cached name for the '_Notification' method.
		/// </summary>
		public new static readonly StringName _Notification = "_Notification";

		/// <summary>
		/// Cached name for the 'LayoutChildren' method.
		/// </summary>
		public static readonly StringName LayoutChildren = "LayoutChildren";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Container.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
