using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NGainEpochVfx.cs")]
public class NGainEpochVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _epoch = "_epoch";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _label = "_label";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static int _vfxCount;

	private Control _epoch;

	private EpochModel _model;

	private TextureRect _portrait;

	private MegaLabel _label;

	private Tween? _tween;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_gain_epoch");

	public override void _Ready()
	{
		_epoch = GetNode<Control>("%EpochContainer");
		_portrait = GetNode<TextureRect>("%Portrait");
		_portrait.Texture = _model.Portrait;
		_label = GetNode<MegaLabel>("%Label");
		_label.SetTextAutoSize(new LocString("vfx", "EPOCH_GAIN").GetRawText());
		TaskHelper.RunSafely(AnimateVfx());
	}

	private async Task AnimateVfx()
	{
		if (_vfxCount > 1)
		{
			int num = 3000 * (_vfxCount - 1);
			Log.Info($"Delaying Gain Epoch Vfx by: {num}ms");
			await Task.Delay(num);
		}
		_epoch.RotationDegrees = -30f;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_epoch, "position:x", 164f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_epoch, "rotation", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.Chain();
		_tween.TweenInterval(1.5);
		_tween.Chain();
		_tween.TweenProperty(this, "modulate:a", 0f, 1.0);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	public static NGainEpochVfx Create(EpochModel model)
	{
		_vfxCount++;
		NGainEpochVfx nGainEpochVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NGainEpochVfx>(PackedScene.GenEditState.Disabled);
		nGainEpochVfx._model = model;
		return nGainEpochVfx;
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
		_vfxCount--;
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
		if (name == PropertyName._epoch)
		{
			_epoch = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
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
		if (name == PropertyName._epoch)
		{
			value = VariantUtils.CreateFrom(in _epoch);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epoch, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._epoch, Variant.From(in _epoch));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._epoch, out var value))
		{
			_epoch = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value2))
		{
			_portrait = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value3))
		{
			_label = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
