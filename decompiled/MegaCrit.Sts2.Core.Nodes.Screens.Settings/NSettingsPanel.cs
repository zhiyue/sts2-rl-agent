using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Settings;

[ScriptPath("res://src/Core/Nodes/Screens/Settings/NSettingsPanel.cs")]
public class NSettingsPanel : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshSize = "RefreshSize";

		public static readonly StringName OnVisibilityChange = "OnVisibilityChange";

		public static readonly StringName IsSettingsOption = "IsSettingsOption";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Content = "Content";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _minPadding = "_minPadding";

		public static readonly StringName _firstControl = "_firstControl";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private float _minPadding = 50f;

	protected Control? _firstControl;

	private Tween? _tween;

	public VBoxContainer Content { get; private set; }

	public Control? DefaultFocusedControl => _firstControl;

	public override void _Ready()
	{
		Content = GetNode<VBoxContainer>("VBoxContainer");
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnVisibilityChange));
		GetViewport().Connect(Viewport.SignalName.SizeChanged, Callable.From(RefreshSize));
		RefreshSize();
		List<Control> list = new List<Control>();
		GetSettingsOptionsRecursive(Content, list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].FocusNeighborLeft = list[i].GetPath();
			list[i].FocusNeighborRight = list[i].GetPath();
			list[i].FocusNeighborTop = ((i > 0) ? list[i - 1].GetPath() : list[i].GetPath());
			list[i].FocusNeighborBottom = ((i < list.Count - 1) ? list[i + 1].GetPath() : list[i].GetPath());
		}
		_firstControl = list.First();
	}

	private void RefreshSize()
	{
		Vector2 size = GetParent<Control>().Size;
		Vector2 minimumSize = Content.GetMinimumSize();
		if (minimumSize.Y + _minPadding >= size.Y)
		{
			base.Size = new Vector2(Content.Size.X, minimumSize.Y + size.Y * 0.4f);
		}
		else
		{
			base.Size = new Vector2(Content.Size.X, minimumSize.Y);
		}
	}

	protected virtual void OnVisibilityChange()
	{
		if (base.Visible)
		{
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "modulate", Colors.White, 0.5).From(StsColors.transparentBlack).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
		}
	}

	private void GetSettingsOptionsRecursive(Control parent, List<Control> ancestors)
	{
		foreach (Control item in parent.GetChildren().OfType<Control>())
		{
			if (!IsSettingsOption(item))
			{
				GetSettingsOptionsRecursive(item, ancestors);
			}
			else if (item.GetParent<Control>().IsVisible() && item.FocusMode == FocusModeEnum.All)
			{
				ancestors.Add(item);
			}
		}
	}

	private bool IsSettingsOption(Control c)
	{
		if (c is NButton nButton)
		{
			return nButton.IsEnabled;
		}
		if (c is NPaginator || c is NTickbox || c is NButton || c is NDropdownPositioner || c is NSettingsSlider)
		{
			return true;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshSize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsSettingsOption, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "c", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.RefreshSize && args.Count == 0)
		{
			RefreshSize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChange && args.Count == 0)
		{
			OnVisibilityChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsSettingsOption && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsSettingsOption(VariantUtils.ConvertTo<Control>(in args[0])));
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
		if (method == MethodName.RefreshSize)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChange)
		{
			return true;
		}
		if (method == MethodName.IsSettingsOption)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Content)
		{
			Content = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._minPadding)
		{
			_minPadding = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._firstControl)
		{
			_firstControl = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName.Content)
		{
			value = VariantUtils.CreateFrom<VBoxContainer>(Content);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._minPadding)
		{
			value = VariantUtils.CreateFrom(in _minPadding);
			return true;
		}
		if (name == PropertyName._firstControl)
		{
			value = VariantUtils.CreateFrom(in _firstControl);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._minPadding, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._firstControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Content, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Content, Variant.From<VBoxContainer>(Content));
		info.AddProperty(PropertyName._minPadding, Variant.From(in _minPadding));
		info.AddProperty(PropertyName._firstControl, Variant.From(in _firstControl));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Content, out var value))
		{
			Content = value.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._minPadding, out var value2))
		{
			_minPadding = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._firstControl, out var value3))
		{
			_firstControl = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
