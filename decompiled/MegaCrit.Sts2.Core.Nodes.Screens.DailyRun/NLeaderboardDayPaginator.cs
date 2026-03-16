using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NLeaderboardDayPaginator.cs")]
public class NLeaderboardDayPaginator : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName PageLeft = "PageLeft";

		public static readonly StringName PageRight = "PageRight";

		public static readonly StringName DayChangeHelper = "DayChangeHelper";

		public static readonly StringName OnDayChanged = "OnDayChanged";

		public static readonly StringName Disable = "Disable";

		public static readonly StringName Enable = "Enable";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _vfxLabel = "_vfxLabel";

		public static readonly StringName _leftArrow = "_leftArrow";

		public static readonly StringName _rightArrow = "_rightArrow";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _leaderboard = "_leaderboard";
	}

	public new class SignalName : Control.SignalName
	{
	}

	protected MegaLabel _label;

	private MegaLabel _vfxLabel;

	private NLeaderboardPageArrow _leftArrow;

	private NLeaderboardPageArrow _rightArrow;

	private NSelectionReticle _selectionReticle;

	private Tween? _tween;

	private const double _animDuration = 0.25;

	private const float _animDistance = 90f;

	private DateTimeOffset _currentDay;

	private NDailyRunLeaderboard? _leaderboard;

	public override void _Ready()
	{
		_label = GetNode<MegaLabel>("LabelContainer/Mask/Label");
		_vfxLabel = GetNode<MegaLabel>("LabelContainer/Mask/VfxLabel");
		_selectionReticle = GetNode<NSelectionReticle>("SelectionReticle");
		_leftArrow = GetNode<NLeaderboardPageArrow>("LeftArrow");
		_rightArrow = GetNode<NLeaderboardPageArrow>("RightArrow");
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		_leftArrow.Connect(PageLeft);
		_rightArrow.Connect(PageRight);
	}

	public void Initialize(NDailyRunLeaderboard leaderboard, DateTimeOffset dateTime, bool showArrows)
	{
		_currentDay = dateTime;
		_leaderboard = leaderboard;
		OnDayChanged(changeLeaderboardDay: false);
		_leftArrow.Visible = showArrows;
		_rightArrow.Visible = showArrows;
	}

	public override void _GuiInput(InputEvent input)
	{
		base._GuiInput(input);
		if (input.IsActionPressed(MegaInput.left))
		{
			PageLeft();
		}
		if (input.IsActionPressed(MegaInput.right))
		{
			PageRight();
		}
	}

	private void PageLeft()
	{
		_currentDay -= TimeSpan.FromDays(1);
		DayChangeHelper(pagedLeft: true);
	}

	private void PageRight()
	{
		_currentDay += TimeSpan.FromDays(1);
		DayChangeHelper(pagedLeft: false);
	}

	private void DayChangeHelper(bool pagedLeft)
	{
		_vfxLabel.SetTextAutoSize(_label.Text);
		_vfxLabel.Modulate = _label.Modulate;
		OnDayChanged(changeLeaderboardDay: true);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "position:x", 0f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(pagedLeft ? (-90f) : 90f);
		_tween.TweenProperty(_label, "modulate:a", 1f, 0.25).From(0.75f);
		_tween.TweenProperty(_vfxLabel, "position:x", pagedLeft ? 90f : (-90f), 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(0f);
		_tween.TweenProperty(_vfxLabel, "modulate", StsColors.transparentBlack, 0.25);
	}

	private void OnDayChanged(bool changeLeaderboardDay)
	{
		_label.SetTextAutoSize(_currentDay.ToString(NDailyRunScreen.dateFormat));
		if (changeLeaderboardDay)
		{
			_leaderboard.SetDay(_currentDay);
		}
	}

	public void Disable()
	{
		_leftArrow.Disable();
		_rightArrow.Disable();
	}

	public void Enable(bool leftArrowEnabled, bool rightArrowEnabled)
	{
		if (leftArrowEnabled)
		{
			_leftArrow.Enable();
		}
		if (rightArrowEnabled)
		{
			_rightArrow.Enable();
		}
	}

	private void OnFocus()
	{
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
	}

	private void OnUnfocus()
	{
		_selectionReticle.OnDeselect();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PageLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PageRight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DayChangeHelper, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "pagedLeft", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnDayChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "changeLeaderboardDay", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Disable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Enable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "leftArrowEnabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "rightArrowEnabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PageLeft && args.Count == 0)
		{
			PageLeft();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PageRight && args.Count == 0)
		{
			PageRight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DayChangeHelper && args.Count == 1)
		{
			DayChangeHelper(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDayChanged && args.Count == 1)
		{
			OnDayChanged(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disable && args.Count == 0)
		{
			Disable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enable && args.Count == 2)
		{
			Enable(VariantUtils.ConvertTo<bool>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.PageLeft)
		{
			return true;
		}
		if (method == MethodName.PageRight)
		{
			return true;
		}
		if (method == MethodName.DayChangeHelper)
		{
			return true;
		}
		if (method == MethodName.OnDayChanged)
		{
			return true;
		}
		if (method == MethodName.Disable)
		{
			return true;
		}
		if (method == MethodName.Enable)
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
		if (name == PropertyName._vfxLabel)
		{
			_vfxLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			_leftArrow = VariantUtils.ConvertTo<NLeaderboardPageArrow>(in value);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			_rightArrow = VariantUtils.ConvertTo<NLeaderboardPageArrow>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			_leaderboard = VariantUtils.ConvertTo<NDailyRunLeaderboard>(in value);
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
		if (name == PropertyName._vfxLabel)
		{
			value = VariantUtils.CreateFrom(in _vfxLabel);
			return true;
		}
		if (name == PropertyName._leftArrow)
		{
			value = VariantUtils.CreateFrom(in _leftArrow);
			return true;
		}
		if (name == PropertyName._rightArrow)
		{
			value = VariantUtils.CreateFrom(in _rightArrow);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._leaderboard)
		{
			value = VariantUtils.CreateFrom(in _leaderboard);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._vfxLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._vfxLabel, Variant.From(in _vfxLabel));
		info.AddProperty(PropertyName._leftArrow, Variant.From(in _leftArrow));
		info.AddProperty(PropertyName._rightArrow, Variant.From(in _rightArrow));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._leaderboard, Variant.From(in _leaderboard));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._vfxLabel, out var value2))
		{
			_vfxLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._leftArrow, out var value3))
		{
			_leftArrow = value3.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._rightArrow, out var value4))
		{
			_rightArrow = value4.As<NLeaderboardPageArrow>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value5))
		{
			_selectionReticle = value5.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._leaderboard, out var value7))
		{
			_leaderboard = value7.As<NDailyRunLeaderboard>();
		}
	}
}
