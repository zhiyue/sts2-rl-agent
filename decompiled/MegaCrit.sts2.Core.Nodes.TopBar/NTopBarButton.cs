using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarButton.cs")]
public abstract class NTopBarButton : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'InitTopBarButton' method.
		/// </summary>
		public static readonly StringName InitTopBarButton = "InitTopBarButton";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnEnable' method.
		/// </summary>
		public new static readonly StringName OnEnable = "OnEnable";

		/// <summary>
		/// Cached name for the 'OnDisable' method.
		/// </summary>
		public new static readonly StringName OnDisable = "OnDisable";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'UpdateScreenOpen' method.
		/// </summary>
		public static readonly StringName UpdateScreenOpen = "UpdateScreenOpen";

		/// <summary>
		/// Cached name for the 'OnScreenClosed' method.
		/// </summary>
		public static readonly StringName OnScreenClosed = "OnScreenClosed";

		/// <summary>
		/// Cached name for the 'CancelAnimations' method.
		/// </summary>
		public static readonly StringName CancelAnimations = "CancelAnimations";

		/// <summary>
		/// Cached name for the 'IsOpen' method.
		/// </summary>
		public static readonly StringName IsOpen = "IsOpen";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsScreenOpen' property.
		/// </summary>
		public static readonly StringName IsScreenOpen = "IsScreenOpen";

		/// <summary>
		/// Cached name for the '_icon' field.
		/// </summary>
		public static readonly StringName _icon = "_icon";

		/// <summary>
		/// Cached name for the '_hsv' field.
		/// </summary>
		public static readonly StringName _hsv = "_hsv";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	protected Control _icon;

	protected ShaderMaterial? _hsv;

	private const float _hoverAngle = -(float)Math.PI / 15f;

	private const float _hoverShaderV = 1.1f;

	protected const float _hoverAnimDur = 0.5f;

	protected static readonly Vector2 _hoverScale = Vector2.One * 1.1f;

	private CancellationTokenSource? _hoverAnimCancelToken;

	private const float _defaultV = 1f;

	protected const float _unhoverAnimDur = 1f;

	private CancellationTokenSource? _unhoverAnimCancelToken;

	private const float _pressDownV = 0.4f;

	protected const float _pressDownDur = 0.25f;

	private CancellationTokenSource? _pressDownCancelToken;

	protected bool IsScreenOpen { get; private set; }

	public override void _Ready()
	{
		ConnectSignals();
		_icon = GetNode<Control>("Control/Icon");
		_hsv = (ShaderMaterial)_icon.Material;
	}

	protected void InitTopBarButton()
	{
		ConnectSignals();
		_icon = GetNode<Control>("Control/Icon");
		_hsv = (ShaderMaterial)_icon.Material;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CancelAnimations();
	}

	protected override void OnRelease()
	{
		_pressDownCancelToken?.Cancel();
		_hoverAnimCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimHover(_hoverAnimCancelToken));
	}

	protected override void OnPress()
	{
		base.OnPress();
		_hoverAnimCancelToken?.Cancel();
		_pressDownCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimPressDown(_pressDownCancelToken));
	}

	protected virtual async Task AnimPressDown(CancellationTokenSource cancelToken)
	{
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			float targetAngle = startAngle + (float)Math.PI * 2f / 15f;
			while (num < 0.25f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, targetAngle, Ease.CubicOut(num / 0.25f));
				_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 0.4f, Ease.CubicOut(num / 0.25f)));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_icon.Rotation = targetAngle;
			_hsv?.SetShaderParameter(_v, 0.4f);
		}
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		if (IsScreenOpen)
		{
			_hsv?.SetShaderParameter(_v, 1.1f);
			_icon.Scale = _hoverScale;
			return;
		}
		_hsv?.SetShaderParameter(_v, 1.1f);
		_icon.Scale = _hoverScale;
		_unhoverAnimCancelToken?.Cancel();
		_hoverAnimCancelToken?.Cancel();
		_hoverAnimCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimHover(_hoverAnimCancelToken));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.Modulate = Colors.White;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.Modulate = StsColors.disabledTopBarButton;
	}

	protected virtual async Task AnimHover(CancellationTokenSource cancelToken)
	{
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			while (num < 0.5f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, -(float)Math.PI / 15f, Ease.BackOut(num / 0.5f));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_icon.Rotation = -(float)Math.PI / 15f;
		}
	}

	protected override void OnUnfocus()
	{
		if (IsScreenOpen)
		{
			_pressDownCancelToken?.Cancel();
			_hsv?.SetShaderParameter(_v, 1f);
			_icon.Scale = Vector2.One;
		}
		else
		{
			_hoverAnimCancelToken?.Cancel();
			_pressDownCancelToken?.Cancel();
			_unhoverAnimCancelToken?.Cancel();
			_unhoverAnimCancelToken = new CancellationTokenSource();
			TaskHelper.RunSafely(AnimUnhover(_unhoverAnimCancelToken));
		}
	}

	protected virtual async Task AnimUnhover(CancellationTokenSource cancelToken)
	{
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			while (num < 1f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, 0f, Ease.ElasticOut(num / 1f));
				_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 1f, Ease.ExpoOut(num / 1f)));
				_icon.Scale = _hoverScale.Lerp(Vector2.One, Ease.ExpoOut(num / 1f));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_hsv?.SetShaderParameter(_v, 1f);
			_icon.Rotation = 0f;
			_icon.Scale = Vector2.One;
		}
	}

	protected void UpdateScreenOpen()
	{
		bool flag = IsOpen();
		if (IsScreenOpen != flag)
		{
			IsScreenOpen = flag;
			if (!IsScreenOpen)
			{
				OnScreenClosed();
			}
		}
	}

	private void OnScreenClosed()
	{
		CancelAnimations();
		_unhoverAnimCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimUnhover(_unhoverAnimCancelToken));
	}

	private void CancelAnimations()
	{
		_hoverAnimCancelToken?.Cancel();
		_pressDownCancelToken?.Cancel();
		_unhoverAnimCancelToken?.Cancel();
	}

	protected abstract bool IsOpen();

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitTopBarButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateScreenOpen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelAnimations, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsOpen, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitTopBarButton && args.Count == 0)
		{
			InitTopBarButton();
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
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
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScreenOpen && args.Count == 0)
		{
			UpdateScreenOpen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenClosed && args.Count == 0)
		{
			OnScreenClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelAnimations && args.Count == 0)
		{
			CancelAnimations();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsOpen && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsOpen());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.InitTopBarButton)
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
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
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
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.UpdateScreenOpen)
		{
			return true;
		}
		if (method == MethodName.OnScreenClosed)
		{
			return true;
		}
		if (method == MethodName.CancelAnimations)
		{
			return true;
		}
		if (method == MethodName.IsOpen)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsScreenOpen)
		{
			IsScreenOpen = VariantUtils.ConvertTo<bool>(in value);
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
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsScreenOpen)
		{
			value = VariantUtils.CreateFrom<bool>(IsScreenOpen);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsScreenOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsScreenOpen, Variant.From<bool>(IsScreenOpen));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsScreenOpen, out var value))
		{
			IsScreenOpen = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value3))
		{
			_hsv = value3.As<ShaderMaterial>();
		}
	}
}
