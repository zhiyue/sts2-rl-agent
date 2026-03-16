using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NRunHistoryArrowButton.cs")]
public class NRunHistoryArrowButton : NGoldArrowButton
{
	public new class MethodName : NGoldArrowButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : NGoldArrowButton.PropertyName
	{
		public static readonly StringName IsLeft = "IsLeft";

		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _isLeft = "_isLeft";
	}

	public new class SignalName : NGoldArrowButton.SignalName
	{
	}

	private bool _isLeft;

	public bool IsLeft
	{
		get
		{
			return _isLeft;
		}
		set
		{
			if (_isLeft != value)
			{
				UnregisterHotkeys();
				_isLeft = value;
				RegisterHotkeys();
				_icon.FlipH = !IsLeft;
				UpdateControllerButton();
			}
		}
	}

	protected override string[] Hotkeys => new string[1] { _isLeft ? MegaInput.viewDeckAndTabLeft : MegaInput.viewExhaustPileAndTabRight };

	public override void _Ready()
	{
		base._Ready();
		_icon.FlipH = !IsLeft;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsLeft)
		{
			IsLeft = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isLeft)
		{
			_isLeft = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsLeft)
		{
			value = VariantUtils.CreateFrom<bool>(IsLeft);
			return true;
		}
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName._isLeft)
		{
			value = VariantUtils.CreateFrom(in _isLeft);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLeft, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsLeft, Variant.From<bool>(IsLeft));
		info.AddProperty(PropertyName._isLeft, Variant.From(in _isLeft));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsLeft, out var value))
		{
			IsLeft = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isLeft, out var value2))
		{
			_isLeft = value2.As<bool>();
		}
	}
}
