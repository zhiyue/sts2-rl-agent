using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NCursorManager.cs")]
public class NCursorManager : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName StopOverridingCursor = "StopOverridingCursor";

		public static readonly StringName OverrideCursor = "OverrideCursor";

		public static readonly StringName UpdateCursor = "UpdateCursor";

		public static readonly StringName SetIsUsingController = "SetIsUsingController";

		public static readonly StringName SetCursorShown = "SetCursorShown";

		public static readonly StringName RefreshCursorShown = "RefreshCursorShown";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName CursorTilted = "CursorTilted";

		public static readonly StringName CursorNotTilted = "CursorNotTilted";

		public static readonly StringName HotSpot = "HotSpot";

		public static readonly StringName _cursorTilted = "_cursorTilted";

		public static readonly StringName _cursorNotTilted = "_cursorNotTilted";

		public static readonly StringName _cursorInspect = "_cursorInspect";

		public static readonly StringName _overriddenCursorTilted = "_overriddenCursorTilted";

		public static readonly StringName _overriddenCursorNotTilted = "_overriddenCursorNotTilted";

		public static readonly StringName _lastSetCursor = "_lastSetCursor";

		public static readonly StringName _isDown = "_isDown";

		public static readonly StringName _isUsingController = "_isUsingController";

		public static readonly StringName _shouldShowCursor = "_shouldShowCursor";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly Vector2 _defaultHotSpot = new Vector2(14f, 5f);

	private static readonly Vector2 _inspectHotSpot = new Vector2(12f, 12f);

	[Export(PropertyHint.None, "")]
	private Image _cursorTilted;

	[Export(PropertyHint.None, "")]
	private Image _cursorNotTilted;

	[Export(PropertyHint.None, "")]
	private Image _cursorInspect;

	private Image? _overriddenCursorTilted;

	private Image? _overriddenCursorNotTilted;

	private Vector2? _overriddenHotSpot;

	private Image? _lastSetCursor;

	private bool _isDown;

	private bool _isUsingController;

	private bool _shouldShowCursor = true;

	private Image CursorTilted => _overriddenCursorTilted ?? _cursorTilted;

	private Image CursorNotTilted => _overriddenCursorNotTilted ?? _cursorNotTilted;

	private Vector2 HotSpot => _overriddenHotSpot ?? _defaultHotSpot;

	public override void _EnterTree()
	{
		Input.SetCustomMouseCursor(_cursorInspect, Input.CursorShape.Help, _inspectHotSpot);
		UpdateCursor();
	}

	public override void _Ready()
	{
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(delegate
		{
			SetIsUsingController(isUsingController: true);
		}));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(delegate
		{
			SetIsUsingController(isUsingController: false);
		}));
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && (inputEventMouseButton.ButtonIndex == MouseButton.Left || inputEventMouseButton.ButtonIndex == MouseButton.Right || inputEventMouseButton.ButtonIndex == MouseButton.Middle))
		{
			if (inputEventMouseButton.IsPressed() && !_isDown)
			{
				_isDown = true;
				UpdateCursor();
			}
			else if (inputEventMouseButton.IsReleased() && _isDown)
			{
				_isDown = false;
				UpdateCursor();
			}
		}
	}

	public void StopOverridingCursor()
	{
		_overriddenCursorTilted = null;
		_overriddenCursorNotTilted = null;
		_overriddenHotSpot = null;
		UpdateCursor();
	}

	public void OverrideCursor(Image cursorTilted, Image cursorNotTilted, Vector2 hotspot)
	{
		_overriddenCursorTilted = cursorTilted;
		_overriddenCursorNotTilted = cursorNotTilted;
		_overriddenHotSpot = hotspot;
		UpdateCursor();
	}

	private void UpdateCursor()
	{
		if (Input.MouseMode != Input.MouseModeEnum.Hidden)
		{
			Image image = (_isDown ? CursorTilted : CursorNotTilted);
			if (image != _lastSetCursor)
			{
				Input.SetCustomMouseCursor(image, Input.CursorShape.Arrow, HotSpot);
				_lastSetCursor = image;
			}
		}
	}

	private void SetIsUsingController(bool isUsingController)
	{
		_isUsingController = isUsingController;
		RefreshCursorShown();
	}

	public void SetCursorShown(bool show)
	{
		_shouldShowCursor = show;
		RefreshCursorShown();
	}

	private void RefreshCursorShown()
	{
		bool flag = !_isUsingController && _shouldShowCursor;
		Input.MouseMode = (Input.MouseModeEnum)(flag ? 0 : 1);
		if (!flag)
		{
			_lastSetCursor = null;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopOverridingCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OverrideCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cursorTilted", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Image"), exported: false),
			new PropertyInfo(Variant.Type.Object, "cursorNotTilted", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Image"), exported: false),
			new PropertyInfo(Variant.Type.Vector2, "hotspot", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsUsingController, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isUsingController", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetCursorShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "show", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshCursorShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopOverridingCursor && args.Count == 0)
		{
			StopOverridingCursor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OverrideCursor && args.Count == 3)
		{
			OverrideCursor(VariantUtils.ConvertTo<Image>(in args[0]), VariantUtils.ConvertTo<Image>(in args[1]), VariantUtils.ConvertTo<Vector2>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCursor && args.Count == 0)
		{
			UpdateCursor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIsUsingController && args.Count == 1)
		{
			SetIsUsingController(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCursorShown && args.Count == 1)
		{
			SetCursorShown(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshCursorShown && args.Count == 0)
		{
			RefreshCursorShown();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.StopOverridingCursor)
		{
			return true;
		}
		if (method == MethodName.OverrideCursor)
		{
			return true;
		}
		if (method == MethodName.UpdateCursor)
		{
			return true;
		}
		if (method == MethodName.SetIsUsingController)
		{
			return true;
		}
		if (method == MethodName.SetCursorShown)
		{
			return true;
		}
		if (method == MethodName.RefreshCursorShown)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._cursorTilted)
		{
			_cursorTilted = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._cursorNotTilted)
		{
			_cursorNotTilted = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._cursorInspect)
		{
			_cursorInspect = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._overriddenCursorTilted)
		{
			_overriddenCursorTilted = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._overriddenCursorNotTilted)
		{
			_overriddenCursorNotTilted = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._lastSetCursor)
		{
			_lastSetCursor = VariantUtils.ConvertTo<Image>(in value);
			return true;
		}
		if (name == PropertyName._isDown)
		{
			_isDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isUsingController)
		{
			_isUsingController = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._shouldShowCursor)
		{
			_shouldShowCursor = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Image from;
		if (name == PropertyName.CursorTilted)
		{
			from = CursorTilted;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CursorNotTilted)
		{
			from = CursorNotTilted;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HotSpot)
		{
			value = VariantUtils.CreateFrom<Vector2>(HotSpot);
			return true;
		}
		if (name == PropertyName._cursorTilted)
		{
			value = VariantUtils.CreateFrom(in _cursorTilted);
			return true;
		}
		if (name == PropertyName._cursorNotTilted)
		{
			value = VariantUtils.CreateFrom(in _cursorNotTilted);
			return true;
		}
		if (name == PropertyName._cursorInspect)
		{
			value = VariantUtils.CreateFrom(in _cursorInspect);
			return true;
		}
		if (name == PropertyName._overriddenCursorTilted)
		{
			value = VariantUtils.CreateFrom(in _overriddenCursorTilted);
			return true;
		}
		if (name == PropertyName._overriddenCursorNotTilted)
		{
			value = VariantUtils.CreateFrom(in _overriddenCursorNotTilted);
			return true;
		}
		if (name == PropertyName._lastSetCursor)
		{
			value = VariantUtils.CreateFrom(in _lastSetCursor);
			return true;
		}
		if (name == PropertyName._isDown)
		{
			value = VariantUtils.CreateFrom(in _isDown);
			return true;
		}
		if (name == PropertyName._isUsingController)
		{
			value = VariantUtils.CreateFrom(in _isUsingController);
			return true;
		}
		if (name == PropertyName._shouldShowCursor)
		{
			value = VariantUtils.CreateFrom(in _shouldShowCursor);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorTilted, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorNotTilted, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorInspect, PropertyHint.ResourceType, "Image", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overriddenCursorTilted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overriddenCursorNotTilted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CursorTilted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CursorNotTilted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HotSpot, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastSetCursor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isUsingController, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._shouldShowCursor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cursorTilted, Variant.From(in _cursorTilted));
		info.AddProperty(PropertyName._cursorNotTilted, Variant.From(in _cursorNotTilted));
		info.AddProperty(PropertyName._cursorInspect, Variant.From(in _cursorInspect));
		info.AddProperty(PropertyName._overriddenCursorTilted, Variant.From(in _overriddenCursorTilted));
		info.AddProperty(PropertyName._overriddenCursorNotTilted, Variant.From(in _overriddenCursorNotTilted));
		info.AddProperty(PropertyName._lastSetCursor, Variant.From(in _lastSetCursor));
		info.AddProperty(PropertyName._isDown, Variant.From(in _isDown));
		info.AddProperty(PropertyName._isUsingController, Variant.From(in _isUsingController));
		info.AddProperty(PropertyName._shouldShowCursor, Variant.From(in _shouldShowCursor));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cursorTilted, out var value))
		{
			_cursorTilted = value.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._cursorNotTilted, out var value2))
		{
			_cursorNotTilted = value2.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._cursorInspect, out var value3))
		{
			_cursorInspect = value3.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._overriddenCursorTilted, out var value4))
		{
			_overriddenCursorTilted = value4.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._overriddenCursorNotTilted, out var value5))
		{
			_overriddenCursorNotTilted = value5.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._lastSetCursor, out var value6))
		{
			_lastSetCursor = value6.As<Image>();
		}
		if (info.TryGetProperty(PropertyName._isDown, out var value7))
		{
			_isDown = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isUsingController, out var value8))
		{
			_isUsingController = value8.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._shouldShowCursor, out var value9))
		{
			_shouldShowCursor = value9.As<bool>();
		}
	}
}
