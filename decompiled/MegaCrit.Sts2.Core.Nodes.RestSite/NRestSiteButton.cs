using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.RestSite;

[ScriptPath("res://src/Core/Nodes/RestSite/NRestSiteButton.cs")]
public class NRestSiteButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Reload = "Reload";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName RefreshTextState = "RefreshTextState";

		public static readonly StringName UpdateShaderParam = "UpdateShaderParam";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _visuals = "_visuals";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _label = "_label";

		public static readonly StringName _labelPosition = "_labelPosition";

		public static readonly StringName _isUnclickable = "_isUnclickable";

		public static readonly StringName _executingOption = "_executingOption";

		public static readonly StringName _currentTween = "_currentTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private ShaderMaterial _hsv;

	private Control _visuals;

	private TextureRect _icon;

	private Control _outline;

	private MegaLabel _label;

	private Vector2 _labelPosition;

	private bool _isUnclickable;

	private bool _executingOption;

	private const double _unfocusAnimDur = 1.0;

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	private Tween? _currentTween;

	private RestSiteOption? _option;

	private static readonly string _scenePath = SceneHelper.GetScenePath("rest_site/rest_site_button");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public RestSiteOption Option
	{
		get
		{
			return _option ?? throw new InvalidOperationException("Option accessed before being set");
		}
		set
		{
			_option = value;
			Reload();
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		base.Modulate = StsColors.transparentBlack;
		_visuals = GetNode<Control>("%Visuals");
		_icon = GetNode<TextureRect>("%Icon");
		_outline = GetNode<Control>("%Outline");
		_label = GetNode<MegaLabel>("%Label");
		_hsv = (ShaderMaterial)_icon.Material;
		_labelPosition = _label.Position;
		TaskHelper.RunSafely(AnimateIn());
		Reload();
	}

	private async Task AnimateIn()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate", Colors.White, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		await tween.AwaitFinished(_cts.Token);
		if (this.IsValid())
		{
			base.MouseFilter = MouseFilterEnum.Stop;
		}
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		_cts.Dispose();
		_currentTween?.Kill();
	}

	public static NRestSiteButton Create(RestSiteOption option)
	{
		NRestSiteButton nRestSiteButton = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRestSiteButton>(PackedScene.GenEditState.Disabled);
		nRestSiteButton.Option = option;
		nRestSiteButton._isUnclickable = !option.IsEnabled;
		return nRestSiteButton;
	}

	private void Reload()
	{
		if (IsNodeReady() && !(_option == null))
		{
			_icon.Texture = Option.Icon;
			_label.SetTextAutoSize(Option.Title.GetFormattedText());
			if (!_option.IsEnabled)
			{
				_hsv.SetShaderParameter(_s, 0f);
				_hsv.SetShaderParameter(_v, 0.6f);
			}
			else
			{
				_hsv.SetShaderParameter(_s, 1f);
				_hsv.SetShaderParameter(_v, 1f);
			}
		}
	}

	protected override void OnRelease()
	{
		if (!_isUnclickable)
		{
			base.OnRelease();
			TaskHelper.RunSafely(SelectOption(Option));
		}
	}

	private async Task SelectOption(RestSiteOption option)
	{
		int num = NRestSiteRoom.Instance.Options.IndexOf(option);
		if (num < 0)
		{
			throw new InvalidOperationException($"Rest site option {option} was selected, but it was not in the list of rest site options!");
		}
		_executingOption = true;
		NRestSiteRoom.Instance.DisableOptions();
		RefreshTextState();
		bool success = false;
		try
		{
			success = await RunManager.Instance.RestSiteSynchronizer.ChooseLocalOption(num);
			if (success)
			{
				NRestSiteRoom.Instance.AfterSelectingOption(option);
			}
		}
		finally
		{
			_executingOption = false;
			if (this.IsValid())
			{
				RefreshTextState();
			}
			if (!success && this.IsValid())
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				NRestSiteRoom.Instance?.EnableOptions();
			}
		}
	}

	protected override void OnPress()
	{
		if (!_isUnclickable)
		{
			base.OnPress();
			_currentTween?.Kill();
			_currentTween = CreateTween().SetParallel();
			_currentTween.TweenProperty(_visuals, "scale", Vector2.One * 0.9f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_currentTween.TweenProperty(_label, "position", _labelPosition, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_currentTween.TweenProperty(_outline, "scale", Vector2.One * 0.9f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), 1.2f, 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_currentTween?.Kill();
		_currentTween = CreateTween().SetParallel();
		_currentTween.TweenProperty(_visuals, "scale", Vector2.One * 1.1f, 0.05);
		_currentTween.TweenProperty(_outline, "scale", Vector2.One, 0.05);
		_currentTween.TweenProperty(_label, "position", _labelPosition + new Vector2(0f, 6f), 0.05);
		if (!_isUnclickable)
		{
			_hsv.SetShaderParameter(_v, 1.2f);
		}
		RefreshTextState();
	}

	protected override void OnUnfocus()
	{
		_currentTween?.Kill();
		_currentTween = CreateTween().SetParallel();
		_currentTween.TweenProperty(_visuals, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenProperty(_label, "position", _labelPosition, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_currentTween.TweenProperty(_outline, "scale", Vector2.One * 0.9f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		if (!_isUnclickable)
		{
			_currentTween.TweenMethod(Callable.From<float>(UpdateShaderParam), 1.2f, 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		RefreshTextState();
	}

	public void RefreshTextState()
	{
		NRestSiteRoom instance = NRestSiteRoom.Instance;
		if (instance != null)
		{
			if (base.IsFocused || _executingOption)
			{
				instance.SetText(Option.Description.GetFormattedText());
			}
			else
			{
				instance.FadeOutOptionDescription();
			}
		}
	}

	private void UpdateShaderParam(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshTextState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshTextState && args.Count == 0)
		{
			RefreshTextState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderParam && args.Count == 1)
		{
			UpdateShaderParam(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.RefreshTextState)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderParam)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			_visuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._labelPosition)
		{
			_labelPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isUnclickable)
		{
			_isUnclickable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._executingOption)
		{
			_executingOption = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			_currentTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			value = VariantUtils.CreateFrom(in _visuals);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._labelPosition)
		{
			value = VariantUtils.CreateFrom(in _labelPosition);
			return true;
		}
		if (name == PropertyName._isUnclickable)
		{
			value = VariantUtils.CreateFrom(in _isUnclickable);
			return true;
		}
		if (name == PropertyName._executingOption)
		{
			value = VariantUtils.CreateFrom(in _executingOption);
			return true;
		}
		if (name == PropertyName._currentTween)
		{
			value = VariantUtils.CreateFrom(in _currentTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._labelPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isUnclickable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._executingOption, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._visuals, Variant.From(in _visuals));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._labelPosition, Variant.From(in _labelPosition));
		info.AddProperty(PropertyName._isUnclickable, Variant.From(in _isUnclickable));
		info.AddProperty(PropertyName._executingOption, Variant.From(in _executingOption));
		info.AddProperty(PropertyName._currentTween, Variant.From(in _currentTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hsv, out var value))
		{
			_hsv = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._visuals, out var value2))
		{
			_visuals = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value4))
		{
			_outline = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value5))
		{
			_label = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._labelPosition, out var value6))
		{
			_labelPosition = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isUnclickable, out var value7))
		{
			_isUnclickable = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._executingOption, out var value8))
		{
			_executingOption = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._currentTween, out var value9))
		{
			_currentTween = value9.As<Tween>();
		}
	}
}
