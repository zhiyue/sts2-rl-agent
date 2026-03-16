using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NStarCounter.cs")]
public class NStarCounter : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ConnectStarsChangedSignal = "ConnectStarsChangedSignal";

		public static readonly StringName OnHovered = "OnHovered";

		public static readonly StringName OnUnhovered = "OnUnhovered";

		public static readonly StringName OnStarsChanged = "OnStarsChanged";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateStarCount = "UpdateStarCount";

		public static readonly StringName SetStarCountText = "SetStarCountText";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";

		public static readonly StringName RefreshVisibility = "RefreshVisibility";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _rotationLayers = "_rotationLayers";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _lerpingStarCount = "_lerpingStarCount";

		public static readonly StringName _velocity = "_velocity";

		public static readonly StringName _displayedStarCount = "_displayedStarCount";

		public static readonly StringName _hsvTween = "_hsvTween";

		public static readonly StringName _isListeningToCombatState = "_isListeningToCombatState";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly string _starGainVfxPath = SceneHelper.GetScenePath("vfx/star_gain_vfx");

	private Player? _player;

	private MegaRichTextLabel _label;

	private Control _rotationLayers;

	private Control _icon;

	private ShaderMaterial _hsv;

	private float _lerpingStarCount;

	private float _velocity;

	private int _displayedStarCount;

	private Tween? _hsvTween;

	private bool _isListeningToCombatState;

	private HoverTip _hoverTip;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_starGainVfxPath);

	public override void _Ready()
	{
		_label = GetNode<MegaRichTextLabel>("%CountLabel");
		_rotationLayers = GetNode<Control>("%RotationLayers");
		_icon = GetNode<Control>("Icon");
		_hsv = (ShaderMaterial)_icon.Material;
		LocString locString = new LocString("static_hover_tips", "STAR_COUNT.description");
		locString.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
		_hoverTip = new HoverTip(new LocString("static_hover_tips", "STAR_COUNT.title"), locString);
		Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
		base.Visible = false;
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		ConnectStarsChangedSignal();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_player != null && _isListeningToCombatState)
		{
			_player.PlayerCombatState.StarsChanged -= OnStarsChanged;
			_isListeningToCombatState = false;
		}
	}

	private void ConnectStarsChangedSignal()
	{
		if (_player != null && !_isListeningToCombatState)
		{
			_player.PlayerCombatState.StarsChanged += OnStarsChanged;
			_isListeningToCombatState = true;
		}
	}

	public void Initialize(Player player)
	{
		_player = player;
		ConnectStarsChangedSignal();
		RefreshVisibility();
	}

	private void OnHovered()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(-34f, -300f);
	}

	private void OnUnhovered()
	{
		NHoverTipSet.Remove(this);
	}

	private void OnStarsChanged(int oldStars, int newStars)
	{
		UpdateStarCount(oldStars, newStars);
		RefreshVisibility();
	}

	public override void _Process(double delta)
	{
		if (_player != null)
		{
			float num = ((_player.PlayerCombatState.Stars == 0) ? 5f : 30f);
			for (int i = 0; i < _rotationLayers.GetChildCount(); i++)
			{
				_rotationLayers.GetChild<Control>(i).RotationDegrees += (float)delta * num * (float)(i + 1);
			}
			_lerpingStarCount = MathHelper.SmoothDamp(_lerpingStarCount, _player.PlayerCombatState.Stars, ref _velocity, 0.1f, (float)delta);
			SetStarCountText(Mathf.RoundToInt(_lerpingStarCount));
		}
	}

	private void UpdateStarCount(int oldCount, int newCount)
	{
		if (newCount < oldCount)
		{
			_hsvTween?.Kill();
			_hsv.SetShaderParameter(_v, 1f);
			_lerpingStarCount = newCount;
			SetStarCountText(newCount);
		}
		else if (newCount > oldCount)
		{
			_hsvTween?.Kill();
			_hsvTween = CreateTween();
			_hsvTween.TweenMethod(Callable.From<float>(UpdateShaderV), 2f, 1f, 0.20000000298023224);
			Node2D node2D = PreloadManager.Cache.GetAsset<PackedScene>(_starGainVfxPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
			this.AddChildSafely(node2D);
			MoveChild(node2D, 0);
			node2D.Position = base.Size / 2f;
		}
	}

	private void SetStarCountText(int stars)
	{
		if (_displayedStarCount != stars)
		{
			_displayedStarCount = stars;
			_label.AddThemeColorOverride(ThemeConstants.Label.fontColor, (stars == 0) ? StsColors.red : StsColors.cream);
			_label.Text = $"[center]{stars}[/center]";
			if (stars == 0)
			{
				_hsv.SetShaderParameter(_s, 0.5f);
				_hsv.SetShaderParameter(_v, 0.85f);
			}
			else
			{
				_hsv.SetShaderParameter(_s, 1f);
				_hsv.SetShaderParameter(_v, 1f);
			}
		}
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	private void RefreshVisibility()
	{
		if (_player == null)
		{
			base.Visible = false;
			return;
		}
		int stars = _player.PlayerCombatState.Stars;
		base.Visible = base.Visible || _player.Character.ShouldAlwaysShowStarCounter || stars > 0;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectStarsChangedSignal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStarsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldStars", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "newStars", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateStarCount, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldCount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "newCount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetStarCountText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "stars", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectStarsChangedSignal && args.Count == 0)
		{
			ConnectStarsChangedSignal();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHovered && args.Count == 0)
		{
			OnHovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnhovered && args.Count == 0)
		{
			OnUnhovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStarsChanged && args.Count == 2)
		{
			OnStarsChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateStarCount && args.Count == 2)
		{
			UpdateStarCount(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetStarCountText && args.Count == 1)
		{
			SetStarCountText(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshVisibility && args.Count == 0)
		{
			RefreshVisibility();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ConnectStarsChangedSignal)
		{
			return true;
		}
		if (method == MethodName.OnHovered)
		{
			return true;
		}
		if (method == MethodName.OnUnhovered)
		{
			return true;
		}
		if (method == MethodName.OnStarsChanged)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateStarCount)
		{
			return true;
		}
		if (method == MethodName.SetStarCountText)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		if (method == MethodName.RefreshVisibility)
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
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._rotationLayers)
		{
			_rotationLayers = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._lerpingStarCount)
		{
			_lerpingStarCount = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			_velocity = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._displayedStarCount)
		{
			_displayedStarCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._hsvTween)
		{
			_hsvTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isListeningToCombatState)
		{
			_isListeningToCombatState = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._rotationLayers)
		{
			value = VariantUtils.CreateFrom(in _rotationLayers);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._lerpingStarCount)
		{
			value = VariantUtils.CreateFrom(in _lerpingStarCount);
			return true;
		}
		if (name == PropertyName._velocity)
		{
			value = VariantUtils.CreateFrom(in _velocity);
			return true;
		}
		if (name == PropertyName._displayedStarCount)
		{
			value = VariantUtils.CreateFrom(in _displayedStarCount);
			return true;
		}
		if (name == PropertyName._hsvTween)
		{
			value = VariantUtils.CreateFrom(in _hsvTween);
			return true;
		}
		if (name == PropertyName._isListeningToCombatState)
		{
			value = VariantUtils.CreateFrom(in _isListeningToCombatState);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rotationLayers, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._lerpingStarCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._velocity, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._displayedStarCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsvTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isListeningToCombatState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._rotationLayers, Variant.From(in _rotationLayers));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._lerpingStarCount, Variant.From(in _lerpingStarCount));
		info.AddProperty(PropertyName._velocity, Variant.From(in _velocity));
		info.AddProperty(PropertyName._displayedStarCount, Variant.From(in _displayedStarCount));
		info.AddProperty(PropertyName._hsvTween, Variant.From(in _hsvTween));
		info.AddProperty(PropertyName._isListeningToCombatState, Variant.From(in _isListeningToCombatState));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._rotationLayers, out var value2))
		{
			_rotationLayers = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value4))
		{
			_hsv = value4.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._lerpingStarCount, out var value5))
		{
			_lerpingStarCount = value5.As<float>();
		}
		if (info.TryGetProperty(PropertyName._velocity, out var value6))
		{
			_velocity = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._displayedStarCount, out var value7))
		{
			_displayedStarCount = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._hsvTween, out var value8))
		{
			_hsvTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isListeningToCombatState, out var value9))
		{
			_isListeningToCombatState = value9.As<bool>();
		}
	}
}
