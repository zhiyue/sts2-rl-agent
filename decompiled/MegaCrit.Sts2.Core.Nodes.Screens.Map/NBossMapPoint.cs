using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NBossMapPoint.cs")]
public class NBossMapPoint : NMapPoint
{
	public new class MethodName : NMapPoint.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnSelected = "OnSelected";

		public new static readonly StringName RefreshColorInstantly = "RefreshColorInstantly";
	}

	public new class PropertyName : NMapPoint.PropertyName
	{
		public new static readonly StringName TraveledColor = "TraveledColor";

		public new static readonly StringName UntravelableColor = "UntravelableColor";

		public new static readonly StringName HoveredColor = "HoveredColor";

		public new static readonly StringName HoverScale = "HoverScale";

		public new static readonly StringName DownScale = "DownScale";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _usesSpine = "_usesSpine";

		public static readonly StringName _spriteContainer = "_spriteContainer";

		public static readonly StringName _spineSprite = "_spineSprite";

		public static readonly StringName _material = "_material";

		public static readonly StringName _placeholderImage = "_placeholderImage";

		public static readonly StringName _placeholderOutline = "_placeholderOutline";
	}

	public new class SignalName : NMapPoint.SignalName
	{
	}

	private static readonly StringName _mapColor = new StringName("map_color");

	private static readonly StringName _blackLayerColor = new StringName("black_layer_color");

	private Tween? _hoverTween;

	private ActModel _act;

	private bool _usesSpine;

	private Node2D _spriteContainer;

	private Node2D _spineSprite;

	private MegaSprite _animController;

	private ShaderMaterial _material;

	private TextureRect _placeholderImage;

	private TextureRect _placeholderOutline;

	protected override Color TraveledColor => StsColors.pathDotTraveled;

	protected override Color UntravelableColor => StsColors.red;

	protected override Color HoveredColor => StsColors.pathDotTraveled;

	protected override Vector2 HoverScale => Vector2.One * 1.05f;

	protected override Vector2 DownScale => Vector2.One * 1.02f;

	private static string BossMapPointPath => SceneHelper.GetScenePath("ui/boss_map_point");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(BossMapPointPath);

	public static NBossMapPoint Create(MapPoint point, NMapScreen screen, IRunState runState)
	{
		NBossMapPoint nBossMapPoint = PreloadManager.Cache.GetScene(BossMapPointPath).Instantiate<NBossMapPoint>(PackedScene.GenEditState.Disabled);
		nBossMapPoint.Point = point;
		nBossMapPoint._screen = screen;
		nBossMapPoint._runState = runState;
		nBossMapPoint._act = runState.Act;
		return nBossMapPoint;
	}

	public override void _Ready()
	{
		ConnectSignals();
		Disable();
		_spriteContainer = GetNode<Node2D>("%SpriteContainer");
		_spineSprite = GetNode<Node2D>("%SpineSprite");
		_animController = new MegaSprite(_spineSprite);
		EncounterModel encounterModel = ((base.Point == _runState.Map.SecondBossMapPoint) ? _runState.Act.SecondBossEncounter : _runState.Act.BossEncounter);
		if (encounterModel.BossNodeSpineResource != null)
		{
			_usesSpine = true;
			_spineSprite.Visible = true;
			_animController.SetSkeletonDataRes(encounterModel.BossNodeSpineResource);
			_animController.GetAnimationState().AddAnimation("animation");
			_material = (ShaderMaterial)_animController.GetNormalMaterial();
		}
		else
		{
			_usesSpine = false;
			_spineSprite.Visible = false;
			_placeholderImage = GetNode<TextureRect>("%PlaceholderImage");
			_placeholderOutline = GetNode<TextureRect>("%PlaceholderOutline");
			_placeholderImage.Visible = true;
			_placeholderImage.Texture = PreloadManager.Cache.GetAsset<Texture2D>(encounterModel.BossNodePath + ".png");
			_placeholderOutline.Texture = PreloadManager.Cache.GetAsset<Texture2D>(encounterModel.BossNodePath + "_outline.png");
			_placeholderImage.SelfModulate = _act.MapTraveledColor;
			_placeholderOutline.SelfModulate = _act.MapBgColor;
		}
		RefreshColorInstantly();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		if (IsInputAllowed() && base.IsTravelable)
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenProperty(_spriteContainer, "scale", HoverScale, 0.05);
			_ = _usesSpine;
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		if (IsInputAllowed())
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenProperty(_spriteContainer, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_ = _usesSpine;
		}
	}

	protected override void OnPress()
	{
		if (base.IsTravelable)
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenProperty(_spriteContainer, "scale", DownScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	public override void OnSelected()
	{
		base.State = MapPointState.Traveled;
	}

	protected override void RefreshColorInstantly()
	{
		if (_usesSpine)
		{
			MapPointState state = base.State;
			if ((uint)(state - 1) <= 1u)
			{
				_material.SetShaderParameter(_blackLayerColor, _act.MapTraveledColor);
			}
			else
			{
				_material.SetShaderParameter(_blackLayerColor, _act.MapUntraveledColor);
			}
			_material.SetShaderParameter(_mapColor, _act.MapBgColor);
		}
		else
		{
			MapPointState state = base.State;
			bool flag = (uint)(state - 1) <= 1u;
			Color selfModulate = (flag ? _act.MapTraveledColor : _act.MapUntraveledColor);
			_placeholderImage.SelfModulate = selfModulate;
			_placeholderOutline.SelfModulate = _act.MapBgColor;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshColorInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._usesSpine)
		{
			_usesSpine = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._spriteContainer)
		{
			_spriteContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._spineSprite)
		{
			_spineSprite = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._material)
		{
			_material = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._placeholderImage)
		{
			_placeholderImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._placeholderOutline)
		{
			_placeholderOutline = VariantUtils.ConvertTo<TextureRect>(in value);
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
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._usesSpine)
		{
			value = VariantUtils.CreateFrom(in _usesSpine);
			return true;
		}
		if (name == PropertyName._spriteContainer)
		{
			value = VariantUtils.CreateFrom(in _spriteContainer);
			return true;
		}
		if (name == PropertyName._spineSprite)
		{
			value = VariantUtils.CreateFrom(in _spineSprite);
			return true;
		}
		if (name == PropertyName._material)
		{
			value = VariantUtils.CreateFrom(in _material);
			return true;
		}
		if (name == PropertyName._placeholderImage)
		{
			value = VariantUtils.CreateFrom(in _placeholderImage);
			return true;
		}
		if (name == PropertyName._placeholderOutline)
		{
			value = VariantUtils.CreateFrom(in _placeholderOutline);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.TraveledColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.UntravelableColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.HoveredColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.DownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._usesSpine, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spriteContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spineSprite, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._material, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._placeholderImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._placeholderOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._usesSpine, Variant.From(in _usesSpine));
		info.AddProperty(PropertyName._spriteContainer, Variant.From(in _spriteContainer));
		info.AddProperty(PropertyName._spineSprite, Variant.From(in _spineSprite));
		info.AddProperty(PropertyName._material, Variant.From(in _material));
		info.AddProperty(PropertyName._placeholderImage, Variant.From(in _placeholderImage));
		info.AddProperty(PropertyName._placeholderOutline, Variant.From(in _placeholderOutline));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hoverTween, out var value))
		{
			_hoverTween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._usesSpine, out var value2))
		{
			_usesSpine = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._spriteContainer, out var value3))
		{
			_spriteContainer = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._spineSprite, out var value4))
		{
			_spineSprite = value4.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._material, out var value5))
		{
			_material = value5.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._placeholderImage, out var value6))
		{
			_placeholderImage = value6.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._placeholderOutline, out var value7))
		{
			_placeholderOutline = value7.As<TextureRect>();
		}
	}
}
