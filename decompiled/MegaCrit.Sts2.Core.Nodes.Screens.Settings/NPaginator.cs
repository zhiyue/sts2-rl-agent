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

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NPaginator.cs")]
public class NPaginator : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName OnIndexChanged = "OnIndexChanged";

		public static readonly StringName SetIndex = "SetIndex";

		public static readonly StringName PageLeft = "PageLeft";

		public static readonly StringName PageRight = "PageRight";

		public static readonly StringName IndexChangeHelper = "IndexChangeHelper";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _vfxLabel = "_vfxLabel";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _currentIndex = "_currentIndex";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	protected MegaLabel _label;

	private MegaLabel _vfxLabel;

	private NSelectionReticle _selectionReticle;

	protected readonly List<string> _options = new List<string>();

	protected int _currentIndex;

	private Tween? _tween;

	private const double _animDuration = 0.25;

	private const float _animDistance = 90f;

	public override void _Ready()
	{
		if (GetType() != typeof(NPaginator))
		{
			throw new InvalidOperationException("Don't call base._Ready(). Use ConnectSignals() instead");
		}
		ConnectSignals();
	}

	protected void ConnectSignals()
	{
		_label = GetNode<MegaLabel>("%Label");
		_vfxLabel = GetNode<MegaLabel>("%VfxLabel");
		_selectionReticle = GetNode<NSelectionReticle>("SelectionReticle");
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
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

	protected virtual void OnIndexChanged(int index)
	{
	}

	public void SetIndex(int index)
	{
		if (_currentIndex != index)
		{
			_currentIndex = Mathf.Clamp(index, 0, _options.Count - 1);
			OnIndexChanged(_currentIndex);
		}
	}

	public void PageLeft()
	{
		_currentIndex--;
		if (_currentIndex < 0)
		{
			_currentIndex = _options.Count - 1;
		}
		IndexChangeHelper(pagedLeft: true);
	}

	public void PageRight()
	{
		_currentIndex++;
		if (_currentIndex > _options.Count - 1)
		{
			_currentIndex = 0;
		}
		IndexChangeHelper(pagedLeft: false);
	}

	private void IndexChangeHelper(bool pagedLeft)
	{
		_vfxLabel.SetTextAutoSize(_label.Text);
		_vfxLabel.Modulate = _label.Modulate;
		OnIndexChanged(_currentIndex);
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_label, "position:x", 0f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(pagedLeft ? (-90f) : 90f);
		_tween.TweenProperty(_label, "modulate:a", 1f, 0.25).From(0.75f);
		_tween.TweenProperty(_vfxLabel, "position:x", pagedLeft ? 90f : (-90f), 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
			.From(0f);
		_tween.TweenProperty(_vfxLabel, "modulate", StsColors.transparentBlack, 0.25);
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
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "input", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnIndexChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetIndex, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PageLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PageRight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IndexChangeHelper, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "pagedLeft", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnIndexChanged && args.Count == 1)
		{
			OnIndexChanged(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIndex && args.Count == 1)
		{
			SetIndex(VariantUtils.ConvertTo<int>(in args[0]));
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
		if (method == MethodName.IndexChangeHelper && args.Count == 1)
		{
			IndexChangeHelper(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.OnIndexChanged)
		{
			return true;
		}
		if (method == MethodName.SetIndex)
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
		if (method == MethodName.IndexChangeHelper)
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
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._currentIndex)
		{
			_currentIndex = VariantUtils.ConvertTo<int>(in value);
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
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._currentIndex)
		{
			value = VariantUtils.CreateFrom(in _currentIndex);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._vfxLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._vfxLabel, Variant.From(in _vfxLabel));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._currentIndex, Variant.From(in _currentIndex));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
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
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value3))
		{
			_selectionReticle = value3.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._currentIndex, out var value4))
		{
			_currentIndex = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
	}
}
