using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NTimelineTutorial.cs")]
public class NTimelineTutorial : Control, IScreenContext
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'Init' method.
		/// </summary>
		public static readonly StringName Init = "Init";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'CloseTutorial' method.
		/// </summary>
		public static readonly StringName CloseTutorial = "CloseTutorial";

		/// <summary>
		/// Cached name for the 'AnimateTutorial' method.
		/// </summary>
		public static readonly StringName AnimateTutorial = "AnimateTutorial";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_text' field.
		/// </summary>
		public static readonly StringName _text = "_text";

		/// <summary>
		/// Cached name for the '_acknowledgeButton' field.
		/// </summary>
		public static readonly StringName _acknowledgeButton = "_acknowledgeButton";

		/// <summary>
		/// Cached name for the '_timeline' field.
		/// </summary>
		public static readonly StringName _timeline = "_timeline";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private MegaRichTextLabel _text;

	private NAcknowledgeButton _acknowledgeButton;

	private NTimelineScreen _timeline;

	private Tween? _tween;

	public Control? DefaultFocusedControl => null;

	public void Init(NTimelineScreen screen)
	{
		_timeline = screen;
		screen.HideBackButtonImmediately();
	}

	public override void _Ready()
	{
		SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_unlock");
		_text = GetNode<MegaRichTextLabel>("%TutorialText");
		_text.Text = "[center]" + new LocString("timeline", "TUTORIAL_TEXT").GetRawText() + "[/center]";
		_acknowledgeButton = GetNode<NAcknowledgeButton>("%AcknowledgeButton");
		_acknowledgeButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(CloseTutorial));
		AnimateTutorial();
	}

	private void CloseTutorial(NButton _)
	{
		_acknowledgeButton.Disable();
		_tween?.FastForwardToCompletion();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.5);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			TaskHelper.RunSafely(_timeline.SpawnFirstTimeTimeline());
			this.QueueFreeSafely();
		}));
	}

	private void AnimateTutorial()
	{
		_acknowledgeButton.Disable();
		_text.VisibleRatio = 0f;
		MegaRichTextLabel text = _text;
		Color modulate = _text.Modulate;
		modulate.A = 0f;
		text.Modulate = modulate;
		_tween?.FastForwardToCompletion();
		_tween = CreateTween();
		_tween.TweenProperty(_text, "visible_ratio", 1f, 2.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		_tween.Parallel().TweenProperty(_text, "modulate:a", 1f, 1.0);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			_acknowledgeButton.Enable();
		}));
		_tween.Parallel().TweenProperty(_acknowledgeButton, "modulate:a", 1f, 0.3).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(1.0);
		_tween.Parallel().TweenProperty(_acknowledgeButton, "position:y", 920f, 0.3).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back)
			.SetDelay(1.0);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Init, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "screen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CloseTutorial, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateTutorial, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Init && args.Count == 1)
		{
			Init(VariantUtils.ConvertTo<NTimelineScreen>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CloseTutorial && args.Count == 1)
		{
			CloseTutorial(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateTutorial && args.Count == 0)
		{
			AnimateTutorial();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Init)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.CloseTutorial)
		{
			return true;
		}
		if (method == MethodName.AnimateTutorial)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._text)
		{
			_text = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._acknowledgeButton)
		{
			_acknowledgeButton = VariantUtils.ConvertTo<NAcknowledgeButton>(in value);
			return true;
		}
		if (name == PropertyName._timeline)
		{
			_timeline = VariantUtils.ConvertTo<NTimelineScreen>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._text)
		{
			value = VariantUtils.CreateFrom(in _text);
			return true;
		}
		if (name == PropertyName._acknowledgeButton)
		{
			value = VariantUtils.CreateFrom(in _acknowledgeButton);
			return true;
		}
		if (name == PropertyName._timeline)
		{
			value = VariantUtils.CreateFrom(in _timeline);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._text, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._acknowledgeButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._timeline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._text, Variant.From(in _text));
		info.AddProperty(PropertyName._acknowledgeButton, Variant.From(in _acknowledgeButton));
		info.AddProperty(PropertyName._timeline, Variant.From(in _timeline));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._text, out var value))
		{
			_text = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._acknowledgeButton, out var value2))
		{
			_acknowledgeButton = value2.As<NAcknowledgeButton>();
		}
		if (info.TryGetProperty(PropertyName._timeline, out var value3))
		{
			_timeline = value3.As<NTimelineScreen>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
