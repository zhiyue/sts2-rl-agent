using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NLogoAnimation.cs")]
public class NLogoAnimation : Control, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _bg = "_bg";

		public static readonly StringName _logoContainer = "_logoContainer";

		public static readonly StringName _logoSpineNode = "_logoSpineNode";

		public static readonly StringName _logoBgColor = "_logoBgColor";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _cancelled = "_cancelled";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/screens/main_menu/logo_animation.tscn";

	private Control _bg;

	private Control _logoContainer;

	private Node2D _logoSpineNode;

	private MegaSprite _spineSprite;

	private Color _logoBgColor = new Color("074254FF");

	private Tween? _tween;

	private bool _cancelled;

	public static string[] AssetPaths => new string[1] { "res://scenes/screens/main_menu/logo_animation.tscn" };

	public Control? DefaultFocusedControl => null;

	public static NLogoAnimation Create()
	{
		return PreloadManager.Cache.GetScene("res://scenes/screens/main_menu/logo_animation.tscn").Instantiate<NLogoAnimation>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_bg = GetNode<Control>("%Bg");
		_logoContainer = GetNode<Control>("%Container");
		_logoSpineNode = GetNode<Node2D>("Container/SpineSprite");
		_spineSprite = new MegaSprite(_logoSpineNode);
		_logoSpineNode.Visible = false;
		Rect2 bounds = _spineSprite.GetSkeleton().GetBounds();
		float num = Math.Min(base.Size.X * 0.33f / bounds.Size.X, base.Size.Y * 0.33f / bounds.Size.Y);
		_logoSpineNode.Scale = num * Vector2.One;
		_logoSpineNode.Position = -bounds.Size * _logoSpineNode.Scale * 0.5f;
	}

	public async Task PlayAnimation(CancellationToken token)
	{
		if (token.IsCancellationRequested)
		{
			_cancelled = true;
			return;
		}
		_tween = CreateTween();
		_tween.TweenInterval(1.0);
		await ToSignal(_tween, Tween.SignalName.Finished);
		if (token.IsCancellationRequested)
		{
			_cancelled = true;
			return;
		}
		_logoSpineNode.Visible = true;
		_spineSprite.GetAnimationState().SetAnimation("animation", loop: false);
		NDebugAudioManager.Instance.Play("SOTE_Logo_Echoing_ShortTail.mp3");
		_tween.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_logoSpineNode, "position:y", _logoSpineNode.Position.Y, 0.5).From(_logoSpineNode.Position.Y - 800f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_logoContainer, "modulate", Colors.White, 0.5);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		while (!_spineSprite.GetAnimationState().GetCurrent(0).IsComplete())
		{
			if (token.IsCancellationRequested)
			{
				_cancelled = true;
				_tween.Kill();
				_tween = CreateTween().SetParallel();
				_tween.TweenProperty(_logoContainer, "modulate", StsColors.transparentWhite, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
				await ToSignal(_tween, Tween.SignalName.Finished);
				break;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (!_cancelled)
		{
			_tween.Kill();
			_tween = CreateTween();
			_tween.TweenProperty(_bg, "modulate", _logoBgColor, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			_tween.Chain();
			_tween.TweenInterval(1.0);
			await ToSignal(_tween, Tween.SignalName.Finished);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NLogoAnimation>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NLogoAnimation>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._logoContainer)
		{
			_logoContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._logoSpineNode)
		{
			_logoSpineNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._logoBgColor)
		{
			_logoBgColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cancelled)
		{
			_cancelled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		if (name == PropertyName._logoContainer)
		{
			value = VariantUtils.CreateFrom(in _logoContainer);
			return true;
		}
		if (name == PropertyName._logoSpineNode)
		{
			value = VariantUtils.CreateFrom(in _logoSpineNode);
			return true;
		}
		if (name == PropertyName._logoBgColor)
		{
			value = VariantUtils.CreateFrom(in _logoBgColor);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._cancelled)
		{
			value = VariantUtils.CreateFrom(in _cancelled);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._logoContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._logoSpineNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._logoBgColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._cancelled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
		info.AddProperty(PropertyName._logoContainer, Variant.From(in _logoContainer));
		info.AddProperty(PropertyName._logoSpineNode, Variant.From(in _logoSpineNode));
		info.AddProperty(PropertyName._logoBgColor, Variant.From(in _logoBgColor));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._cancelled, Variant.From(in _cancelled));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bg, out var value))
		{
			_bg = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._logoContainer, out var value2))
		{
			_logoContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._logoSpineNode, out var value3))
		{
			_logoSpineNode = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._logoBgColor, out var value4))
		{
			_logoBgColor = value4.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value5))
		{
			_tween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cancelled, out var value6))
		{
			_cancelled = value6.As<bool>();
		}
	}
}
