using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NDamageBlockedVfx.cs")]
public class NDamageBlockedVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly LocString _blockedLoc = new LocString("vfx", "BLOCKED");

	private MegaLabel _label;

	private Tween? _tween;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_blocked_text");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NDamageBlockedVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (!creatureNode.IsInteractable)
		{
			return null;
		}
		NDamageBlockedVfx nDamageBlockedVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDamageBlockedVfx>(PackedScene.GenEditState.Disabled);
		nDamageBlockedVfx.GlobalPosition = creatureNode.VfxSpawnPosition + new Vector2(Rng.Chaotic.NextFloat(-20f, 20f), Rng.Chaotic.NextFloat(-60f, -40f));
		nDamageBlockedVfx.RotationDegrees = Rng.Chaotic.NextFloat(-2f, 2f);
		return nDamageBlockedVfx;
	}

	public override void _Ready()
	{
		_label = GetNode<MegaLabel>("Label");
		_label.SetTextAutoSize(_blockedLoc.GetRawText());
		TaskHelper.RunSafely(BlockAnim());
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task BlockAnim()
	{
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "scale", Vector2.One * 0.6f, 2.0).From(Vector2.One).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quad);
		_tween.TweenProperty(this, "position:y", base.Position.Y - 250f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		_tween.TweenProperty(_label, "modulate", Colors.White, 2.0).From(new Color("21C0FF")).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(this, "modulate:a", 0f, 1.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine)
			.SetDelay(0.5);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
	}
}
