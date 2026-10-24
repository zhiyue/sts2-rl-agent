using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MegaCrit.Sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarMapButton.cs")]
public class NTopBarMapButton : NTopBarButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NTopBarButton.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'IsOpen' method.
		/// </summary>
		public new static readonly StringName IsOpen = "IsOpen";

		/// <summary>
		/// Cached name for the 'StartOscillation' method.
		/// </summary>
		public static readonly StringName StartOscillation = "StartOscillation";

		/// <summary>
		/// Cached name for the 'StopOscillation' method.
		/// </summary>
		public static readonly StringName StopOscillation = "StopOscillation";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NTopBarButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the '_oscillateTween' field.
		/// </summary>
		public static readonly StringName _oscillateTween = "_oscillateTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NTopBarButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private const float _defaultV = 0.9f;

	private Tween? _oscillateTween;

	protected override string[] Hotkeys => new string[1] { MegaInput.viewMap };

	protected override void OnRelease()
	{
		base.OnRelease();
		if (IsOpen())
		{
			NCapstoneContainer? instance = NCapstoneContainer.Instance;
			if (instance != null && instance.InUse)
			{
				NCapstoneContainer.Instance.Close();
			}
			else
			{
				NMapScreen.Instance.Close();
			}
		}
		else
		{
			NCapstoneContainer.Instance.Close();
			NMapScreen.Instance.Open(isOpenedFromTopBar: true);
		}
		_hsv?.SetShaderParameter(_v, 0.9f);
	}

	protected override bool IsOpen()
	{
		return NMapScreen.Instance.Visible;
	}

	public void StartOscillation()
	{
		_oscillateTween?.Kill();
		_oscillateTween = CreateTween();
		_oscillateTween.SetLoops();
		_oscillateTween.TweenProperty(_icon, "rotation", -0.12f, 0.8).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		_oscillateTween.TweenProperty(_icon, "rotation", 0.12f, 0.8).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	}

	public void StopOscillation()
	{
		_oscillateTween?.Kill();
		_oscillateTween = CreateTween();
		_oscillateTween.TweenProperty(_icon, "rotation", 0f, 0.5).SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		LocString locString = new LocString("static_hover_tips", "MAP.title");
		locString.Add("Hotkey", NInputManager.Instance.GetShortcutKey(MegaInput.viewMap).ToString());
		HoverTip hoverTip = new HoverTip(locString, new LocString("static_hover_tips", "MAP.description"));
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTip);
		nHoverTipSet?.SetGlobalPosition(base.GlobalPosition + new Vector2(base.Size.X - nHoverTipSet.Size.X, base.Size.Y + 20f));
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		NHoverTipSet.Remove(this);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsOpen, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartOscillation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopOscillation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsOpen && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsOpen());
			return true;
		}
		if (method == MethodName.StartOscillation && args.Count == 0)
		{
			StartOscillation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopOscillation && args.Count == 0)
		{
			StopOscillation();
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.IsOpen)
		{
			return true;
		}
		if (method == MethodName.StartOscillation)
		{
			return true;
		}
		if (method == MethodName.StopOscillation)
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._oscillateTween)
		{
			_oscillateTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName._oscillateTween)
		{
			value = VariantUtils.CreateFrom(in _oscillateTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._oscillateTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._oscillateTween, Variant.From(in _oscillateTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._oscillateTween, out var value))
		{
			_oscillateTween = value.As<Tween>();
		}
	}
}
