using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

[ScriptPath("res://src/Core/Nodes/CommonUi/NVerticalPopup.cs")]
public class NVerticalPopup : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetText = "SetText";

		public static readonly StringName Close = "Close";

		public static readonly StringName HideNoButton = "HideNoButton";

		public static readonly StringName DisconnectSignals = "DisconnectSignals";

		public static readonly StringName DisconnectHotkeys = "DisconnectHotkeys";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName TitleLabel = "TitleLabel";

		public static readonly StringName BodyLabel = "BodyLabel";

		public static readonly StringName YesButton = "YesButton";

		public static readonly StringName NoButton = "NoButton";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/vertical_popup");

	private Callable? _yesCallable;

	private Callable? _noCallable;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public MegaLabel TitleLabel { get; private set; }

	public MegaRichTextLabel BodyLabel { get; private set; }

	public NPopupYesNoButton YesButton { get; private set; }

	public NPopupYesNoButton NoButton { get; private set; }

	public override void _Ready()
	{
		TitleLabel = GetNode<MegaLabel>("Header");
		BodyLabel = GetNode<MegaRichTextLabel>("Description");
		YesButton = GetNode<NPopupYesNoButton>("YesButton");
		NoButton = GetNode<NPopupYesNoButton>("NoButton");
	}

	public void SetText(LocString title, LocString body)
	{
		TitleLabel.SetTextAutoSize(title.GetFormattedText());
		BodyLabel.Text = "[center]" + body.GetFormattedText() + "[/center]";
	}

	public void SetText(string title, string body)
	{
		TitleLabel.SetTextAutoSize(title);
		BodyLabel.Text = "[center]" + body + "[/center]";
	}

	public void InitYesButton(LocString yesButton, Action<NButton> onPressed)
	{
		_yesCallable = Callable.From(onPressed);
		YesButton.IsYes = true;
		YesButton.SetText(yesButton.GetFormattedText());
		YesButton.Connect(NClickableControl.SignalName.Released, _yesCallable.Value);
		YesButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(Close));
	}

	public void InitNoButton(LocString noButton, Action<NButton> onPressed)
	{
		_noCallable = Callable.From(onPressed);
		NoButton.Visible = true;
		NoButton.IsYes = false;
		NoButton.SetText(noButton.GetFormattedText());
		NoButton.Connect(NClickableControl.SignalName.Released, _noCallable.Value);
		NoButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(Close));
	}

	private void Close(NButton _)
	{
		NModalContainer.Instance.Clear();
	}

	public void HideNoButton()
	{
		NoButton.Visible = false;
	}

	public void DisconnectSignals()
	{
		if (_yesCallable.HasValue)
		{
			YesButton.Disconnect(NClickableControl.SignalName.Released, _yesCallable.Value);
			YesButton.Disconnect(NClickableControl.SignalName.Released, Callable.From<NButton>(Close));
		}
		if (_noCallable.HasValue)
		{
			NoButton.Disconnect(NClickableControl.SignalName.Released, _noCallable.Value);
			NoButton.Disconnect(NClickableControl.SignalName.Released, Callable.From<NButton>(Close));
		}
	}

	public void DisconnectHotkeys()
	{
		if (_yesCallable.HasValue)
		{
			YesButton.DisconnectHotkeys();
		}
		if (_noCallable.HasValue)
		{
			NoButton.DisconnectHotkeys();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "title", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "body", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideNoButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectHotkeys, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetText && args.Count == 2)
		{
			SetText(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 1)
		{
			Close(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideNoButton && args.Count == 0)
		{
			HideNoButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectSignals && args.Count == 0)
		{
			DisconnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectHotkeys && args.Count == 0)
		{
			DisconnectHotkeys();
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
		if (method == MethodName.SetText)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.HideNoButton)
		{
			return true;
		}
		if (method == MethodName.DisconnectSignals)
		{
			return true;
		}
		if (method == MethodName.DisconnectHotkeys)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.TitleLabel)
		{
			TitleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName.BodyLabel)
		{
			BodyLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName.YesButton)
		{
			YesButton = VariantUtils.ConvertTo<NPopupYesNoButton>(in value);
			return true;
		}
		if (name == PropertyName.NoButton)
		{
			NoButton = VariantUtils.ConvertTo<NPopupYesNoButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.TitleLabel)
		{
			value = VariantUtils.CreateFrom<MegaLabel>(TitleLabel);
			return true;
		}
		if (name == PropertyName.BodyLabel)
		{
			value = VariantUtils.CreateFrom<MegaRichTextLabel>(BodyLabel);
			return true;
		}
		NPopupYesNoButton from;
		if (name == PropertyName.YesButton)
		{
			from = YesButton;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.NoButton)
		{
			from = NoButton;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TitleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BodyLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.YesButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.NoButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.TitleLabel, Variant.From<MegaLabel>(TitleLabel));
		info.AddProperty(PropertyName.BodyLabel, Variant.From<MegaRichTextLabel>(BodyLabel));
		info.AddProperty(PropertyName.YesButton, Variant.From<NPopupYesNoButton>(YesButton));
		info.AddProperty(PropertyName.NoButton, Variant.From<NPopupYesNoButton>(NoButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.TitleLabel, out var value))
		{
			TitleLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName.BodyLabel, out var value2))
		{
			BodyLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName.YesButton, out var value3))
		{
			YesButton = value3.As<NPopupYesNoButton>();
		}
		if (info.TryGetProperty(PropertyName.NoButton, out var value4))
		{
			NoButton = value4.As<NPopupYesNoButton>();
		}
	}
}
