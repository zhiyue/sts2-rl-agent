using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

/// <summary>
/// Many of this VFX are spawned via NHellraiserVfx.
/// This represents a single sword particle and maybe useful for re-use if we want these to spawn when the
/// Hellraiser power procs.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NHellraiserAttackVfx.cs")]
public class NHellraiserAttackVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnTweenFinished' method.
		/// </summary>
		public static readonly StringName OnTweenFinished = "OnTweenFinished";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	public override void _Ready()
	{
		TextureRect node = GetNode<TextureRect>("%Sword");
		node.FlipH = Rng.Chaotic.NextBool();
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(node, "position:y", 90, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(this, "modulate", Colors.White, 0.30000001192092896).From(Colors.Red);
		tween.Chain().TweenInterval(0.10000000149011612);
		tween.Chain();
		tween.TweenProperty(node, "position:y", 300, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(this, "modulate:a", 0f, 0.30000001192092896);
		tween.Chain().TweenCallback(Callable.From(OnTweenFinished));
	}

	private void OnTweenFinished()
	{
		this.QueueFreeSafely();
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
		list.Add(new MethodInfo(MethodName.OnTweenFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnTweenFinished && args.Count == 0)
		{
			OnTweenFinished();
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
		if (method == MethodName.OnTweenFinished)
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
