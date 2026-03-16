using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NDropdownScrollbar.cs")]
public class NDropdownScrollbar : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshTrainBounds = "RefreshTrainBounds";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName OnShow = "OnShow";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName _Input = "_Input";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName ClampTrain = "ClampTrain";

		public static readonly StringName SetTrainPositionFromPercentage = "SetTrainPositionFromPercentage";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _dropdownContainer = "_dropdownContainer";

		public static readonly StringName _train = "_train";

		public static readonly StringName hasControl = "hasControl";

		public static readonly StringName _startDragPos = "_startDragPos";

		public static readonly StringName _targetDragPos = "_targetDragPos";

		public static readonly StringName _scrollLimitTop = "_scrollLimitTop";

		public static readonly StringName _scrollLimitBottom = "_scrollLimitBottom";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private NDropdownContainer _dropdownContainer;

	private Control _train;

	public bool hasControl;

	private Vector2 _startDragPos;

	private Vector2 _targetDragPos;

	private float _scrollLimitTop;

	private float _scrollLimitBottom;

	public override void _Ready()
	{
		ConnectSignals();
		_dropdownContainer = GetParent<NDropdownContainer>();
		_train = GetNode<Control>("Train");
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnShow));
	}

	public void RefreshTrainBounds()
	{
		_scrollLimitTop = 600f - _train.Size.Y - 9f;
		_scrollLimitBottom = 9f;
	}

	protected override void OnFocus()
	{
		_train.Modulate = StsColors.gold;
	}

	protected override void OnUnfocus()
	{
		if (!hasControl)
		{
			_train.Modulate = StsColors.quarterTransparentWhite;
		}
	}

	private void OnShow()
	{
		_train.Modulate = StsColors.quarterTransparentWhite;
	}

	protected override void OnPress()
	{
		hasControl = true;
		_train.Modulate = StsColors.gold;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	public override void _Input(InputEvent inputEvent)
	{
		base._Input(inputEvent);
		if (hasControl && inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.IsReleased())
		{
			hasControl = false;
			Input.MouseMode = Input.MouseModeEnum.Visible;
			_train.Modulate = StsColors.quarterTransparentWhite;
		}
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		base._GuiInput(inputEvent);
		if (hasControl && inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			_train.Position += new Vector2(0f, inputEventMouseMotion.Relative.Y);
			ClampTrain();
			_dropdownContainer.UpdatePositionBasedOnTrain(1f - (_train.Position.Y - _scrollLimitBottom) / (_scrollLimitTop - _scrollLimitBottom));
		}
	}

	private void ClampTrain()
	{
		_train.Position = new Vector2(_train.Position.X, Mathf.Clamp(_train.Position.Y, _scrollLimitBottom, _scrollLimitTop));
	}

	public void SetTrainPositionFromPercentage(float percentage)
	{
		_train.Position = new Vector2(_train.Position.X, _scrollLimitBottom + percentage * (_scrollLimitTop - _scrollLimitBottom));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshTrainBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClampTrain, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetTrainPositionFromPercentage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "percentage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.RefreshTrainBounds && args.Count == 0)
		{
			RefreshTrainBounds();
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
		if (method == MethodName.OnShow && args.Count == 0)
		{
			OnShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClampTrain && args.Count == 0)
		{
			ClampTrain();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTrainPositionFromPercentage && args.Count == 1)
		{
			SetTrainPositionFromPercentage(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.RefreshTrainBounds)
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
		if (method == MethodName.OnShow)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.ClampTrain)
		{
			return true;
		}
		if (method == MethodName.SetTrainPositionFromPercentage)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._dropdownContainer)
		{
			_dropdownContainer = VariantUtils.ConvertTo<NDropdownContainer>(in value);
			return true;
		}
		if (name == PropertyName._train)
		{
			_train = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.hasControl)
		{
			hasControl = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			_startDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			_targetDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._scrollLimitTop)
		{
			_scrollLimitTop = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._scrollLimitBottom)
		{
			_scrollLimitBottom = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._dropdownContainer)
		{
			value = VariantUtils.CreateFrom(in _dropdownContainer);
			return true;
		}
		if (name == PropertyName._train)
		{
			value = VariantUtils.CreateFrom(in _train);
			return true;
		}
		if (name == PropertyName.hasControl)
		{
			value = VariantUtils.CreateFrom(in hasControl);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			value = VariantUtils.CreateFrom(in _startDragPos);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			value = VariantUtils.CreateFrom(in _targetDragPos);
			return true;
		}
		if (name == PropertyName._scrollLimitTop)
		{
			value = VariantUtils.CreateFrom(in _scrollLimitTop);
			return true;
		}
		if (name == PropertyName._scrollLimitBottom)
		{
			value = VariantUtils.CreateFrom(in _scrollLimitBottom);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dropdownContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._train, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.hasControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._scrollLimitTop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._scrollLimitBottom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dropdownContainer, Variant.From(in _dropdownContainer));
		info.AddProperty(PropertyName._train, Variant.From(in _train));
		info.AddProperty(PropertyName.hasControl, Variant.From(in hasControl));
		info.AddProperty(PropertyName._startDragPos, Variant.From(in _startDragPos));
		info.AddProperty(PropertyName._targetDragPos, Variant.From(in _targetDragPos));
		info.AddProperty(PropertyName._scrollLimitTop, Variant.From(in _scrollLimitTop));
		info.AddProperty(PropertyName._scrollLimitBottom, Variant.From(in _scrollLimitBottom));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dropdownContainer, out var value))
		{
			_dropdownContainer = value.As<NDropdownContainer>();
		}
		if (info.TryGetProperty(PropertyName._train, out var value2))
		{
			_train = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.hasControl, out var value3))
		{
			hasControl = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._startDragPos, out var value4))
		{
			_startDragPos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPos, out var value5))
		{
			_targetDragPos = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._scrollLimitTop, out var value6))
		{
			_scrollLimitTop = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._scrollLimitBottom, out var value7))
		{
			_scrollLimitBottom = value7.As<float>();
		}
	}
}
