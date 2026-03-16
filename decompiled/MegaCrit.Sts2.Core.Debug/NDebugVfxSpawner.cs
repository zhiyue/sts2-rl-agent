using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Debug;

[ScriptPath("res://src/Core/Debug/NDebugVfxSpawner.cs")]
public class NDebugVfxSpawner : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Spawn = "Spawn";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _viewDeck = new StringName("view_deck");

	private void Spawn()
	{
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(ModelDb.Relic<PaelsEye>());
		if (nRelicFlashVfx != null)
		{
			this.AddChildSafely(nRelicFlashVfx);
			nRelicFlashVfx.Scale = Vector2.One * 2f;
			nRelicFlashVfx.Position = Size * 0.5f;
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustReleased(_viewDeck))
		{
			Spawn();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Spawn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Spawn && args.Count == 0)
		{
			Spawn();
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Spawn)
		{
			return true;
		}
		if (method == MethodName._Process)
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
