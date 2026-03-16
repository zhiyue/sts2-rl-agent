using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPingButton.cs")]
public class NPingButton : NButton
{
	private enum State
	{
		Enabled,
		Disabled,
		Hidden
	}

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public static readonly StringName AnimOut = "AnimOut";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName OnCombatEnded = "OnCombatEnded";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";

		public static readonly StringName SetState = "SetState";

		public static readonly StringName RefreshEnabled = "RefreshEnabled";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName ShowPos = "ShowPos";

		public static readonly StringName HidePos = "HidePos";

		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _state = "_state";

		public static readonly StringName _visuals = "_visuals";

		public static readonly StringName _image = "_image";

		public static readonly StringName _label = "_label";

		public static readonly StringName _viewport = "_viewport";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _positionTween = "_positionTween";

		public static readonly StringName _hoverTween = "_hoverTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private const double _flyInOutDuration = 0.5;

	private State _state = State.Hidden;

	private Control _visuals;

	private TextureRect _image;

	private MegaLabel _label;

	private Viewport _viewport;

	private ShaderMaterial _hsv;

	private CancellationTokenSource? _showCancelTokenSource;

	private static readonly Vector2 _showPosRatio = new Vector2(1536f, 932f) / NGame.devResolution;

	private static readonly Vector2 _hidePosRatio = _showPosRatio + new Vector2(0f, 250f) / NGame.devResolution;

	private Tween? _positionTween;

	private Tween? _hoverTween;

	private Vector2 ShowPos => _showPosRatio * _viewport.GetVisibleRect().Size;

	private Vector2 HidePos => _hidePosRatio * _viewport.GetVisibleRect().Size;

	protected override string[] Hotkeys => new string[1] { MegaInput.select };

	public override void _Ready()
	{
		ConnectSignals();
		_visuals = GetNode<Control>("Visuals");
		_image = GetNode<TextureRect>("Visuals/Image");
		_label = GetNode<MegaLabel>("Visuals/Label");
		_viewport = GetViewport();
		_hsv = (ShaderMaterial)_image.Material;
		LocString locString = new LocString("gameplay_ui", "PING_BUTTON");
		_label.SetTextAutoSize(locString.GetFormattedText());
		base.Position = HidePos;
		Disable();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.AboutToSwitchToEnemyTurn += OnAboutToSwitchToEnemyTurn;
		CombatManager.Instance.PlayerEndedTurn += AfterPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn += AfterPlayerUnendedTurn;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CombatManager.Instance.AboutToSwitchToEnemyTurn -= OnAboutToSwitchToEnemyTurn;
		CombatManager.Instance.PlayerEndedTurn -= AfterPlayerEndedTurn;
		CombatManager.Instance.PlayerUnendedTurn -= AfterPlayerUnendedTurn;
	}

	private void AfterPlayerEndedTurn(Player player, bool _)
	{
		if (CombatManager.Instance.AllPlayersReadyToEndTurn())
		{
			SetState(State.Disabled);
		}
		else if (LocalContext.IsMe(player))
		{
			_showCancelTokenSource = new CancellationTokenSource();
			TaskHelper.RunSafely(AnimInAfterDelay());
		}
	}

	private void AfterPlayerUnendedTurn(Player player)
	{
		if (LocalContext.IsMe(player))
		{
			_showCancelTokenSource?.Cancel();
			SetState(State.Hidden);
		}
	}

	private async Task AnimInAfterDelay()
	{
		await Task.Delay(500, _showCancelTokenSource.Token);
		if (!_showCancelTokenSource.IsCancellationRequested)
		{
			SetState(State.Enabled);
		}
	}

	private void OnAboutToSwitchToEnemyTurn(CombatState _)
	{
		SetState(State.Hidden);
	}

	protected override void OnRelease()
	{
		if (_state == State.Enabled)
		{
			RunManager.Instance.FlavorSynchronizer.SendEndTurnPing();
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), base.IsFocused ? 1.5f : 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_hoverTween.TweenProperty(_visuals, "position", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_hoverTween.TweenProperty(_label, "modulate", base.IsEnabled ? StsColors.cream : StsColors.gray, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	protected override void OnEnable()
	{
		_image.Modulate = Colors.White;
		_label.Modulate = StsColors.cream;
	}

	protected override void OnDisable()
	{
		NHoverTipSet.Remove(this);
		_image.Modulate = StsColors.gray;
		_label.Modulate = StsColors.gray;
	}

	private void AnimOut()
	{
		_showCancelTokenSource?.Cancel();
		_hoverTween?.Kill();
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", HidePos, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void AnimIn()
	{
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", ShowPos, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void OnCombatEnded()
	{
		SetState(State.Hidden);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_hoverTween?.Kill();
		_hsv.SetShaderParameter(_v, 1.5);
		_visuals.Position = new Vector2(0f, -2f);
	}

	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(this);
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_visuals, "position", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_label, "modulate", base.IsEnabled ? StsColors.cream : StsColors.gray, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_visuals, "position", new Vector2(0f, 4f), 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_hoverTween.TweenProperty(_label, "modulate", Colors.DarkGray, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	private void SetState(State newState)
	{
		if (_state != newState)
		{
			if (newState == State.Hidden)
			{
				AnimOut();
			}
			if (newState == State.Enabled && _state == State.Hidden)
			{
				AnimIn();
			}
			_state = newState;
			RefreshEnabled();
		}
	}

	public void RefreshEnabled()
	{
		bool flag = NCombatRoom.Instance == null || NCombatRoom.Instance.Mode != CombatRoomMode.ActiveCombat || !ActiveScreenContext.Instance.IsCurrent(NCombatRoom.Instance) || NCombatRoom.Instance.Ui.Hand.IsInCardSelection;
		if (_state == State.Enabled && !flag)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCombatEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "newState", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnable && args.Count == 0)
		{
			OnEnable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisable && args.Count == 0)
		{
			OnDisable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCombatEnded && args.Count == 0)
		{
			OnCombatEnded();
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
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetState && args.Count == 1)
		{
			SetState(VariantUtils.ConvertTo<State>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshEnabled && args.Count == 0)
		{
			RefreshEnabled();
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnEnable)
		{
			return true;
		}
		if (method == MethodName.OnDisable)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.OnCombatEnded)
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
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		if (method == MethodName.SetState)
		{
			return true;
		}
		if (method == MethodName.RefreshEnabled)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<State>(in value);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			_visuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._image)
		{
			_image = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			_positionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Vector2 from;
		if (name == PropertyName.ShowPos)
		{
			from = ShowPos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HidePos)
		{
			from = HidePos;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._visuals)
		{
			value = VariantUtils.CreateFrom(in _visuals);
			return true;
		}
		if (name == PropertyName._image)
		{
			value = VariantUtils.CreateFrom(in _image);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			value = VariantUtils.CreateFrom(in _positionTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._visuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._image, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.ShowPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._positionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._visuals, Variant.From(in _visuals));
		info.AddProperty(PropertyName._image, Variant.From(in _image));
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._positionTween, Variant.From(in _positionTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._state, out var value))
		{
			_state = value.As<State>();
		}
		if (info.TryGetProperty(PropertyName._visuals, out var value2))
		{
			_visuals = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._image, out var value3))
		{
			_image = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._label, out var value4))
		{
			_label = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value5))
		{
			_viewport = value5.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value6))
		{
			_hsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._positionTween, out var value7))
		{
			_positionTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value8))
		{
			_hoverTween = value8.As<Tween>();
		}
	}
}
