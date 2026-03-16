using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NAncientMapPoint.cs")]
public class NAncientMapPoint : NMapPoint
{
	public new class MethodName : NMapPoint.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnSelected = "OnSelected";

		public new static readonly StringName RefreshColorInstantly = "RefreshColorInstantly";

		public static readonly StringName AnimHover = "AnimHover";

		public static readonly StringName AnimUnhover = "AnimUnhover";

		public static readonly StringName AnimPressDown = "AnimPressDown";
	}

	public new class PropertyName : NMapPoint.PropertyName
	{
		public new static readonly StringName TraveledColor = "TraveledColor";

		public new static readonly StringName UntravelableColor = "UntravelableColor";

		public new static readonly StringName HoveredColor = "HoveredColor";

		public new static readonly StringName HoverScale = "HoverScale";

		public new static readonly StringName DownScale = "DownScale";

		public static readonly StringName TargetMaterial = "TargetMaterial";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _elapsedTime = "_elapsedTime";
	}

	public new class SignalName : NMapPoint.SignalName
	{
	}

	private TextureRect _icon;

	private TextureRect _outline;

	private Tween? _tween;

	private const float _pulseSpeed = 4f;

	private const float _scaleAmount = 0.05f;

	private const float _scaleBase = 1f;

	private float _elapsedTime = Rng.Chaotic.NextFloat(3140f);

	protected override Color TraveledColor => StsColors.pathDotTraveled;

	protected override Color UntravelableColor => StsColors.bossNodeUntraveled;

	protected override Color HoveredColor => StsColors.pathDotTraveled;

	protected override Vector2 HoverScale => Vector2.One * 1.1f;

	protected override Vector2 DownScale => Vector2.One * 0.9f;

	private static string UntravelableMaterialPath => "res://materials/boss_map_point_unavailable.tres";

	private static string AncientMapPointPath => SceneHelper.GetScenePath("ui/ancient_map_point");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { UntravelableMaterialPath, AncientMapPointPath });

	private Material? TargetMaterial
	{
		get
		{
			if (base.IsTravelable || base.State == MapPointState.Traveled)
			{
				return null;
			}
			return PreloadManager.Cache.GetMaterial(UntravelableMaterialPath);
		}
	}

	public static NAncientMapPoint Create(MapPoint point, NMapScreen screen, IRunState runState)
	{
		NAncientMapPoint nAncientMapPoint = PreloadManager.Cache.GetScene(AncientMapPointPath).Instantiate<NAncientMapPoint>(PackedScene.GenEditState.Disabled);
		nAncientMapPoint.Point = point;
		nAncientMapPoint._screen = screen;
		nAncientMapPoint._runState = runState;
		return nAncientMapPoint;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_icon = GetNode<TextureRect>("Icon");
		_icon.Texture = _runState.Act.Ancient.MapIcon;
		_outline = GetNode<TextureRect>("Icon/Outline");
		_outline.Texture = _runState.Act.Ancient.MapIconOutline;
		_outline.Modulate = _runState.Act.MapBgColor;
		RefreshColorInstantly();
	}

	public override void _Process(double delta)
	{
		if (_isEnabled)
		{
			if (!base.IsFocused && IsInputAllowed())
			{
				_elapsedTime += (float)delta * 4f;
				base.Scale = Vector2.One * (Mathf.Sin(_elapsedTime) * 0.05f + 1f);
			}
			else
			{
				base.Scale = base.Scale.Lerp(Vector2.One, 0.5f);
			}
		}
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		if (IsInputAllowed())
		{
			AnimHover();
			if (NControllerManager.Instance.IsUsingController)
			{
				_controllerSelectionReticle.OnSelect();
			}
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		if (IsInputAllowed())
		{
			AnimUnhover();
			_controllerSelectionReticle.OnDeselect();
		}
	}

	protected override void OnPress()
	{
		if (base.IsTravelable)
		{
			AnimPressDown();
			_controllerSelectionReticle.OnDeselect();
		}
	}

	public override void OnSelected()
	{
		base.State = MapPointState.Traveled;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_icon, "self_modulate", base.TargetColor, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void RefreshColorInstantly()
	{
		_icon.SelfModulate = base.TargetColor;
	}

	private void AnimHover()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", HoverScale, 0.05);
		_tween.TweenProperty(_icon, "self_modulate", HoveredColor, 0.05);
		if (base.IsTravelable)
		{
			_tween.TweenProperty(_outline, "modulate", _outlineColor, 0.05);
		}
	}

	private void AnimUnhover()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_icon, "self_modulate", base.TargetColor, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void AnimPressDown()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", DownScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshColorInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimUnhover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimPressDown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelected && args.Count == 0)
		{
			OnSelected();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshColorInstantly && args.Count == 0)
		{
			RefreshColorInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHover && args.Count == 0)
		{
			AnimHover();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimUnhover && args.Count == 0)
		{
			AnimUnhover();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimPressDown && args.Count == 0)
		{
			AnimPressDown();
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
		if (method == MethodName._Process)
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
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnSelected)
		{
			return true;
		}
		if (method == MethodName.RefreshColorInstantly)
		{
			return true;
		}
		if (method == MethodName.AnimHover)
		{
			return true;
		}
		if (method == MethodName.AnimUnhover)
		{
			return true;
		}
		if (method == MethodName.AnimPressDown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			_elapsedTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Color from;
		if (name == PropertyName.TraveledColor)
		{
			from = TraveledColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.UntravelableColor)
		{
			from = UntravelableColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HoveredColor)
		{
			from = HoveredColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		Vector2 from2;
		if (name == PropertyName.HoverScale)
		{
			from2 = HoverScale;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.DownScale)
		{
			from2 = DownScale;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.TargetMaterial)
		{
			value = VariantUtils.CreateFrom<Material>(TargetMaterial);
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
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._elapsedTime)
		{
			value = VariantUtils.CreateFrom(in _elapsedTime);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.TraveledColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.UntravelableColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.HoveredColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.DownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TargetMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._elapsedTime, Variant.From(in _elapsedTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._icon, out var value))
		{
			_icon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value2))
		{
			_outline = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._elapsedTime, out var value4))
		{
			_elapsedTime = value4.As<float>();
		}
	}
}
