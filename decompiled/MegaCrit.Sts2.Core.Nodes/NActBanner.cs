using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NActBanner.cs")]
public class NActBanner : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _actNumber = "_actNumber";

		public static readonly StringName _actName = "_actName";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _actIndex = "_actIndex";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private MegaLabel _actNumber;

	private MegaLabel _actName;

	private ColorRect _banner;

	private static readonly string _path = SceneHelper.GetScenePath("ui/act_banner");

	private ActModel _act;

	private int _actIndex;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_path);

	public static NActBanner? Create(ActModel act, int actIndex)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NActBanner nActBanner = PreloadManager.Cache.GetScene(_path).Instantiate<NActBanner>(PackedScene.GenEditState.Disabled);
		nActBanner._act = act;
		nActBanner._actIndex = actIndex;
		return nActBanner;
	}

	public override void _Ready()
	{
		_actNumber = GetNode<MegaLabel>("ActNumber");
		_actName = GetNode<MegaLabel>("ActName");
		_banner = GetNode<ColorRect>("%Banner");
		LocString locString = new LocString("gameplay_ui", "ACT_NUMBER");
		locString.Add("actNumber", _actIndex + 1);
		_actNumber.SetTextAutoSize(locString.GetFormattedText());
		_actName.SetTextAutoSize(_act.Title.GetFormattedText());
		TaskHelper.RunSafely(AnimateVfx());
	}

	private async Task AnimateVfx()
	{
		_banner.Modulate = StsColors.transparentBlack;
		_actName.Modulate = StsColors.transparentWhite;
		_actNumber.Modulate = StsColors.transparentWhite;
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_banner, "modulate:a", 0.25f, 0.5).SetDelay(0.5);
		tween.TweenProperty(_actName, "modulate:a", 1f, 1.0).SetDelay(0.25);
		tween.TweenProperty(_actNumber, "modulate:a", 1f, 1.0).SetDelay(0.5);
		tween.TweenProperty(_actNumber, "position:y", 440f, 1.25).SetDelay(0.5).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quad)
			.From(450f);
		tween.Chain();
		tween.TweenInterval((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.5 : 2.0);
		tween.Chain();
		tween.TweenProperty(this, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._actNumber)
		{
			_actNumber = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._actName)
		{
			_actName = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._actIndex)
		{
			_actIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._actNumber)
		{
			value = VariantUtils.CreateFrom(in _actNumber);
			return true;
		}
		if (name == PropertyName._actName)
		{
			value = VariantUtils.CreateFrom(in _actName);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._actIndex)
		{
			value = VariantUtils.CreateFrom(in _actIndex);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actNumber, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actName, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._actIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._actNumber, Variant.From(in _actNumber));
		info.AddProperty(PropertyName._actName, Variant.From(in _actName));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._actIndex, Variant.From(in _actIndex));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._actNumber, out var value))
		{
			_actNumber = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._actName, out var value2))
		{
			_actName = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value3))
		{
			_banner = value3.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._actIndex, out var value4))
		{
			_actIndex = value4.As<int>();
		}
	}
}
