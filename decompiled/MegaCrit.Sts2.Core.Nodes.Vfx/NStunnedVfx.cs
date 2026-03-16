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
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NStunnedVfx.cs")]
public class NStunnedVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _textTween = "_textTween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/stunned_vfx.tscn";

	private static LocString _stunnedLoc = new LocString("vfx", "STUNNED");

	private MegaLabel _label;

	private Creature _creature;

	private Tween? _textTween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/stunned_vfx.tscn");

	public override void _Ready()
	{
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(_creature);
		if (nCreature == null)
		{
			this.QueueFreeSafely();
			return;
		}
		_label = GetNode<MegaLabel>("%Label");
		base.GlobalPosition = nCreature.GetTopOfHitbox();
		TaskHelper.RunSafely(StartVfx());
	}

	public override void _ExitTree()
	{
		_textTween?.Kill();
	}

	private async Task StartVfx()
	{
		_label.SetTextAutoSize(_stunnedLoc.GetFormattedText());
		_textTween = CreateTween();
		_textTween.TweenProperty(_label, "modulate:a", 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_textTween.TweenInterval(0.5);
		_textTween.TweenProperty(_label, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Linear);
		CreateTween().TweenProperty(_label, "position:y", _label.Position.Y - 100f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart);
		await ToSignal(_textTween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public static NStunnedVfx? Create(Creature creature)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (NCombatUi.IsDebugHideTextVfx)
		{
			return null;
		}
		NStunnedVfx nStunnedVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/stunned_vfx.tscn").Instantiate<NStunnedVfx>(PackedScene.GenEditState.Disabled);
		nStunnedVfx._creature = creature;
		return nStunnedVfx;
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
		if (name == PropertyName._textTween)
		{
			_textTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._textTween)
		{
			value = VariantUtils.CreateFrom(in _textTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._textTween, Variant.From(in _textTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._textTween, out var value2))
		{
			_textTween = value2.As<Tween>();
		}
	}
}
