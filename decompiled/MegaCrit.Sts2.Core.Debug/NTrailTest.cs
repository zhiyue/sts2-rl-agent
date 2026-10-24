using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Debug;

/// <summary>
/// This is used strictly for debugging CardTrailVfx and related
/// </summary>
[ScriptPath("res://src/Core/Debug/NTrailTest.cs")]
public class NTrailTest : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	public override void _Ready()
	{
		DelaySpawn();
	}

	private async Task DelaySpawn()
	{
		await Task.Delay(100);
		Control node = GetNode<Control>("Ironclad");
		NCardTrailVfx child = NCardTrailVfx.Create(node, SceneHelper.GetScenePath("vfx/card_trail_ironclad"));
		GetParent().AddChildSafely(child);
		Control node2 = GetNode<Control>("Silent");
		child = NCardTrailVfx.Create(node2, SceneHelper.GetScenePath("vfx/card_trail_silent"));
		GetParent().AddChildSafely(child);
		Control node3 = GetNode<Control>("Defect");
		child = NCardTrailVfx.Create(node3, SceneHelper.GetScenePath("vfx/card_trail_defect"));
		GetParent().AddChildSafely(child);
		Control node4 = GetNode<Control>("Regent");
		child = NCardTrailVfx.Create(node4, SceneHelper.GetScenePath("vfx/card_trail_regent"));
		GetParent().AddChildSafely(child);
		Control node5 = GetNode<Control>("Binder");
		child = NCardTrailVfx.Create(node5, SceneHelper.GetScenePath("vfx/card_trail_necrobinder"));
		GetParent().AddChildSafely(child);
	}

	public override void _Process(double delta)
	{
		Vector2 mousePosition = GetViewport().GetMousePosition();
		base.GlobalPosition = mousePosition;
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
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Process)
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
