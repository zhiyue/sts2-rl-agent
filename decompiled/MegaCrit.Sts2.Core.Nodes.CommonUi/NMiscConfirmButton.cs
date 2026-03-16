using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NMiscConfirmButton.cs")]
public class NMiscConfirmButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnWindowChange = "OnWindowChange";

		public new static readonly StringName OnEnable = "OnEnable";

		public new static readonly StringName OnDisable = "OnDisable";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _buttonImage = "_buttonImage";

		public static readonly StringName _downColor = "_downColor";

		public static readonly StringName _showPos = "_showPos";

		public static readonly StringName _hidePos = "_hidePos";

		public static readonly StringName _moveTween = "_moveTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private Control _buttonImage;

	private Color _downColor = Colors.Gray;

	private static readonly Vector2 _hoverScale = new Vector2(1.05f, 1.05f);

	private static readonly Vector2 _downScale = new Vector2(0.95f, 0.95f);

	private const float _pressDownDur = 0.25f;

	private const float _unhoverAnimDur = 0.5f;

	private const float _animInOutDur = 0.35f;

	private Vector2 _showPos;

	private Vector2 _hidePos;

	private Tween? _moveTween;

	private CancellationTokenSource? _pressDownCancelToken;

	private CancellationTokenSource? _unhoverAnimCancelToken;

	public override void _Ready()
	{
		ConnectSignals();
		_isEnabled = false;
		_buttonImage = GetNode<Control>("Image");
		GetTree().Root.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		OnWindowChange();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_pressDownCancelToken?.Cancel();
		_unhoverAnimCancelToken?.Cancel();
	}

	private void OnWindowChange()
	{
		_showPos = base.Position;
		_hidePos = base.Position + new Vector2(0f, 64f);
	}

	protected override void OnEnable()
	{
		_buttonImage.Modulate = Colors.White;
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "position", _showPos, 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(_hidePos);
	}

	protected override void OnDisable()
	{
		_moveTween?.Kill();
		_moveTween = CreateTween();
		_moveTween.TweenProperty(this, "position", _hidePos, 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(_showPos);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_unhoverAnimCancelToken?.Cancel();
		base.Scale = _hoverScale;
		_buttonImage.Modulate = Colors.White;
	}

	protected override void OnUnfocus()
	{
		_pressDownCancelToken?.Cancel();
		_unhoverAnimCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimUnhover(_unhoverAnimCancelToken));
	}

	private async Task AnimUnhover(CancellationTokenSource cancelToken)
	{
		float timer = 0f;
		Vector2 startScale = base.Scale;
		Color startButtonColor = _buttonImage.Modulate;
		for (; timer < 0.5f; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			base.Scale = startScale.Lerp(Vector2.One, Ease.ExpoOut(timer / 0.5f));
			_buttonImage.Modulate = startButtonColor.Lerp(Colors.White, Ease.ExpoOut(timer / 0.5f));
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		base.Scale = Vector2.One;
		_buttonImage.Modulate = Colors.White;
	}

	protected override void OnPress()
	{
		base.OnPress();
		_pressDownCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimPressDown(_pressDownCancelToken));
	}

	private async Task AnimPressDown(CancellationTokenSource cancelToken)
	{
		float timer = 0f;
		_buttonImage.Modulate = Colors.White;
		base.Scale = _hoverScale;
		for (; timer < 0.25f; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			base.Scale = _hoverScale.Lerp(_downScale, Ease.CubicOut(timer / 0.25f));
			_buttonImage.Modulate = Colors.White.Lerp(_downColor, Ease.CubicOut(timer / 0.25f));
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		base.Scale = _downScale;
		_buttonImage.Modulate = _downColor;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnWindowChange)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._buttonImage)
		{
			_buttonImage = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			_downColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			_showPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			_hidePos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			_moveTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._buttonImage)
		{
			value = VariantUtils.CreateFrom(in _buttonImage);
			return true;
		}
		if (name == PropertyName._downColor)
		{
			value = VariantUtils.CreateFrom(in _downColor);
			return true;
		}
		if (name == PropertyName._showPos)
		{
			value = VariantUtils.CreateFrom(in _showPos);
			return true;
		}
		if (name == PropertyName._hidePos)
		{
			value = VariantUtils.CreateFrom(in _hidePos);
			return true;
		}
		if (name == PropertyName._moveTween)
		{
			value = VariantUtils.CreateFrom(in _moveTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._buttonImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._downColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._moveTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._buttonImage, Variant.From(in _buttonImage));
		info.AddProperty(PropertyName._downColor, Variant.From(in _downColor));
		info.AddProperty(PropertyName._showPos, Variant.From(in _showPos));
		info.AddProperty(PropertyName._hidePos, Variant.From(in _hidePos));
		info.AddProperty(PropertyName._moveTween, Variant.From(in _moveTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._buttonImage, out var value))
		{
			_buttonImage = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._downColor, out var value2))
		{
			_downColor = value2.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._showPos, out var value3))
		{
			_showPos = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePos, out var value4))
		{
			_hidePos = value4.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._moveTween, out var value5))
		{
			_moveTween = value5.As<Tween>();
		}
	}
}
