using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NNormalMapPoint.cs")]
public class NNormalMapPoint : NMapPoint
{
	public new class MethodName : NMapPoint.MethodName
	{
		public static readonly StringName IconName = "IconName";

		public static readonly StringName IconPath = "IconPath";

		public static readonly StringName OutlinePath = "OutlinePath";

		public static readonly StringName UnknownIconPath = "UnknownIconPath";

		public static readonly StringName UnknownOutlinePath = "UnknownOutlinePath";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public static readonly StringName RefreshMarkedIconVisibility = "RefreshMarkedIconVisibility";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName OnSelected = "OnSelected";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public static readonly StringName SetAngle = "SetAngle";

		public new static readonly StringName RefreshColorInstantly = "RefreshColorInstantly";

		public new static readonly StringName RefreshState = "RefreshState";

		public static readonly StringName UpdateIcon = "UpdateIcon";

		public static readonly StringName ShowCircleVfx = "ShowCircleVfx";

		public static readonly StringName AnimHover = "AnimHover";

		public static readonly StringName AnimUnhover = "AnimUnhover";

		public static readonly StringName AnimPressDown = "AnimPressDown";

		public static readonly StringName OnHighlightPointType = "OnHighlightPointType";
	}

	public new class PropertyName : NMapPoint.PropertyName
	{
		public new static readonly StringName TraveledColor = "TraveledColor";

		public new static readonly StringName UntravelableColor = "UntravelableColor";

		public new static readonly StringName HoveredColor = "HoveredColor";

		public new static readonly StringName HoverScale = "HoverScale";

		public new static readonly StringName DownScale = "DownScale";

		public static readonly StringName _iconContainer = "_iconContainer";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _questIcon = "_questIcon";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _circleVfx = "_circleVfx";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _pulseTween = "_pulseTween";

		public static readonly StringName _elapsedTime = "_elapsedTime";
	}

	public new class SignalName : NMapPoint.SignalName
	{
	}

	private static readonly StringName _mapColor = new StringName("map_color");

	private Control _iconContainer;

	private TextureRect _icon;

	private TextureRect _questIcon;

	private TextureRect _outline;

	private NMapCircleVfx? _circleVfx;

	private Tween? _tween;

	private Tween? _pulseTween;

	private const float _pulseSpeed = 4f;

	private const float _scaleAmount = 0.25f;

	private const float _scaleBase = 1.2f;

	private float _elapsedTime = Rng.Chaotic.NextFloat(3140f);

	protected override Color TraveledColor => Colors.White;

	protected override Color UntravelableColor => StsColors.halfTransparentWhite;

	protected override Color HoveredColor => Colors.White;

	protected override Vector2 HoverScale => Vector2.One * 1.45f;

	protected override Vector2 DownScale => Vector2.One * 0.9f;

	private static string ScenePath => SceneHelper.GetScenePath("/ui/normal_map_point");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private static string IconName(MapPointType pointType)
	{
		return pointType switch
		{
			MapPointType.Unassigned => "map_unknown", 
			MapPointType.Monster => "map_monster", 
			MapPointType.Elite => "map_elite", 
			MapPointType.Boss => string.Empty, 
			MapPointType.Ancient => "map_unknown", 
			MapPointType.Treasure => "map_chest", 
			MapPointType.Shop => "map_shop", 
			MapPointType.RestSite => "map_rest", 
			MapPointType.Unknown => "map_unknown", 
			_ => throw new ArgumentOutOfRangeException(pointType.ToString()), 
		};
	}

	private static string IconPath(string filename)
	{
		return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/map/icons/" + filename + ".tres");
	}

	private static string OutlinePath(string filename)
	{
		return ImageHelper.GetImagePath("atlases/compressed.sprites/map/" + filename + "_outline.tres");
	}

	private static string UnknownIconPath(RoomType pointType)
	{
		return ImageHelper.GetImagePath("atlases/ui_atlas.sprites/map/icons/map_" + pointType switch
		{
			RoomType.Treasure => "unknown_chest", 
			RoomType.Monster => "unknown_monster", 
			RoomType.Shop => "unknown_shop", 
			RoomType.Elite => "unknown_elite", 
			_ => "unknown", 
		} + ".tres");
	}

	private static string UnknownOutlinePath(RoomType pointType)
	{
		return OutlinePath(pointType switch
		{
			RoomType.Treasure => "map_chest", 
			RoomType.Monster => "map_monster", 
			RoomType.Shop => "map_shop", 
			RoomType.Elite => "map_elite", 
			_ => "map_unknown", 
		});
	}

	public static NNormalMapPoint Create(MapPoint point, NMapScreen screen, IRunState runState)
	{
		NNormalMapPoint nNormalMapPoint = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NNormalMapPoint>(PackedScene.GenEditState.Disabled);
		nNormalMapPoint.Point = point;
		nNormalMapPoint._screen = screen;
		nNormalMapPoint._runState = runState;
		return nNormalMapPoint;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_iconContainer = GetNode<Control>("%IconContainer");
		_icon = GetNode<TextureRect>("%Icon");
		_outline = GetNode<TextureRect>("%Outline");
		_questIcon = GetNode<TextureRect>("%QuestIcon");
		UpdateIcon();
		Color mapBgColor = _runState.Act.MapBgColor;
		_outline.Modulate = mapBgColor;
		ShaderMaterial shaderMaterial = (ShaderMaterial)_icon.Material;
		shaderMaterial.SetShaderParameter(_mapColor, mapBgColor.Lerp(Colors.Gray, 0.5f));
		RefreshMarkedIconVisibility();
		RefreshColorInstantly();
		Disable();
	}

	public override void _EnterTree()
	{
		base.Point.NodeMarkedChanged += RefreshMarkedIconVisibility;
		NMapScreen.Instance.PointTypeHighlighted += OnHighlightPointType;
	}

	private void RefreshMarkedIconVisibility()
	{
		_questIcon.Visible = base.Point.Quests.Count > 0;
	}

	public override void _ExitTree()
	{
		base.Point.NodeMarkedChanged -= RefreshMarkedIconVisibility;
		NMapScreen.Instance.PointTypeHighlighted -= OnHighlightPointType;
	}

	public override void _Process(double delta)
	{
		if (_isEnabled)
		{
			if (!base.IsFocused && IsInputAllowed())
			{
				_elapsedTime += (float)delta * 4f;
				_iconContainer.Scale = Vector2.One * (Mathf.Sin(_elapsedTime) * 0.25f + 1.2f);
			}
			else
			{
				_iconContainer.Scale = _iconContainer.Scale.Lerp(Vector2.One, 0.5f);
			}
		}
	}

	public override void OnSelected()
	{
		ShowCircleVfx(playAnim: true);
		base.State = MapPointState.Traveled;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_iconContainer, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_icon, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_questIcon, "scale", Vector2.One, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_icon, "self_modulate", base.TargetColor, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		if (IsInputAllowed())
		{
			AnimHover();
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		NHoverTipSet.Remove(this);
		if (_isEnabled)
		{
			_elapsedTime = 3.926991f;
		}
		if (IsInputAllowed())
		{
			AnimUnhover();
		}
	}

	protected override void OnPress()
	{
		if (base.IsTravelable)
		{
			AnimPressDown();
			NHoverTipSet.Remove(this);
		}
	}

	public void SetAngle(float degrees)
	{
		_iconContainer.RotationDegrees = degrees;
	}

	protected override void RefreshColorInstantly()
	{
		_icon.SelfModulate = base.TargetColor;
	}

	protected override void RefreshState()
	{
		base.RefreshState();
		UpdateIcon();
		if (base.State == MapPointState.Traveled)
		{
			ShowCircleVfx(playAnim: false);
		}
		if (!base.IsFocused)
		{
			_iconContainer.Scale = Vector2.One;
		}
	}

	private void UpdateIcon()
	{
		if (base.Point.PointType != MapPointType.Unknown || base.State != MapPointState.Traveled)
		{
			string filename = IconName(base.Point.PointType);
			string path = IconPath(filename);
			_icon.Texture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
			string path2 = OutlinePath(IconName(base.Point.PointType));
			_outline.Texture = ResourceLoader.Load<Texture2D>(path2, null, ResourceLoader.CacheMode.Reuse);
		}
		else if (_runState.MapPointHistory.Count > _runState.CurrentActIndex)
		{
			IReadOnlyList<MapPointHistoryEntry> readOnlyList = _runState.MapPointHistory[_runState.CurrentActIndex];
			if (readOnlyList.Count > base.Point.coord.row)
			{
				RoomType roomType = readOnlyList[base.Point.coord.row].Rooms.First().RoomType;
				_icon.Texture = ResourceLoader.Load<Texture2D>(UnknownIconPath(roomType), null, ResourceLoader.CacheMode.Reuse);
				_outline.Texture = ResourceLoader.Load<Texture2D>(UnknownOutlinePath(roomType), null, ResourceLoader.CacheMode.Reuse);
			}
		}
	}

	private void ShowCircleVfx(bool playAnim)
	{
		if (_circleVfx == null)
		{
			_circleVfx = NMapCircleVfx.Create(playAnim);
			this.AddChildSafely(_circleVfx);
			_circleVfx.Position += base.PivotOffset;
		}
	}

	private void AnimHover()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", HoverScale, 0.05);
		_tween.TweenProperty(_questIcon, "scale", HoverScale, 0.05);
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
		_tween.TweenProperty(_questIcon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_icon, "self_modulate", base.TargetColor, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	private void AnimPressDown()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", DownScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_questIcon, "scale", DownScale, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_outline, "modulate", _runState.Act.MapBgColor, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void OnHighlightPointType(MapPointType pointType)
	{
		if (pointType == base.Point.PointType)
		{
			AnimHover();
		}
		else
		{
			AnimUnhover();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(23);
		list.Add(new MethodInfo(MethodName.IconName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IconPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "filename", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OutlinePath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "filename", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UnknownIconPath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UnknownOutlinePath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshMarkedIconVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAngle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "degrees", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshColorInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateIcon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowCircleVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "playAnim", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimHover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimUnhover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimPressDown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHighlightPointType, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IconName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(IconName(VariantUtils.ConvertTo<MapPointType>(in args[0])));
			return true;
		}
		if (method == MethodName.IconPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(IconPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.OutlinePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(OutlinePath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.UnknownIconPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(UnknownIconPath(VariantUtils.ConvertTo<RoomType>(in args[0])));
			return true;
		}
		if (method == MethodName.UnknownOutlinePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(UnknownOutlinePath(VariantUtils.ConvertTo<RoomType>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshMarkedIconVisibility && args.Count == 0)
		{
			RefreshMarkedIconVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelected && args.Count == 0)
		{
			OnSelected();
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
		if (method == MethodName.SetAngle && args.Count == 1)
		{
			SetAngle(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshColorInstantly && args.Count == 0)
		{
			RefreshColorInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshState && args.Count == 0)
		{
			RefreshState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateIcon && args.Count == 0)
		{
			UpdateIcon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowCircleVfx && args.Count == 1)
		{
			ShowCircleVfx(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.OnHighlightPointType && args.Count == 1)
		{
			OnHighlightPointType(VariantUtils.ConvertTo<MapPointType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.IconName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(IconName(VariantUtils.ConvertTo<MapPointType>(in args[0])));
			return true;
		}
		if (method == MethodName.IconPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(IconPath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.OutlinePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(OutlinePath(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.UnknownIconPath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(UnknownIconPath(VariantUtils.ConvertTo<RoomType>(in args[0])));
			return true;
		}
		if (method == MethodName.UnknownOutlinePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(UnknownOutlinePath(VariantUtils.ConvertTo<RoomType>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.IconName)
		{
			return true;
		}
		if (method == MethodName.IconPath)
		{
			return true;
		}
		if (method == MethodName.OutlinePath)
		{
			return true;
		}
		if (method == MethodName.UnknownIconPath)
		{
			return true;
		}
		if (method == MethodName.UnknownOutlinePath)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.RefreshMarkedIconVisibility)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.OnSelected)
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
		if (method == MethodName.SetAngle)
		{
			return true;
		}
		if (method == MethodName.RefreshColorInstantly)
		{
			return true;
		}
		if (method == MethodName.RefreshState)
		{
			return true;
		}
		if (method == MethodName.UpdateIcon)
		{
			return true;
		}
		if (method == MethodName.ShowCircleVfx)
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
		if (method == MethodName.OnHighlightPointType)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._iconContainer)
		{
			_iconContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._questIcon)
		{
			_questIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._circleVfx)
		{
			_circleVfx = VariantUtils.ConvertTo<NMapCircleVfx>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._pulseTween)
		{
			_pulseTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._iconContainer)
		{
			value = VariantUtils.CreateFrom(in _iconContainer);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._questIcon)
		{
			value = VariantUtils.CreateFrom(in _questIcon);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._circleVfx)
		{
			value = VariantUtils.CreateFrom(in _circleVfx);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._pulseTween)
		{
			value = VariantUtils.CreateFrom(in _pulseTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._questIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._circleVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.TraveledColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.UntravelableColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.HoveredColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.DownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pulseTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._iconContainer, Variant.From(in _iconContainer));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._questIcon, Variant.From(in _questIcon));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._circleVfx, Variant.From(in _circleVfx));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._pulseTween, Variant.From(in _pulseTween));
		info.AddProperty(PropertyName._elapsedTime, Variant.From(in _elapsedTime));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._iconContainer, out var value))
		{
			_iconContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._questIcon, out var value3))
		{
			_questIcon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value4))
		{
			_outline = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._circleVfx, out var value5))
		{
			_circleVfx = value5.As<NMapCircleVfx>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._pulseTween, out var value7))
		{
			_pulseTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._elapsedTime, out var value8))
		{
			_elapsedTime = value8.As<float>();
		}
	}
}
