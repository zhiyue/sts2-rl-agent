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

/// <summary>
/// A popup ui/modal which has a Yes and No button.
/// Used for important popups like Abandon Run confirmation and the "Enable Tutorials?" popup.
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NVerticalPopup.cs")]
public class NVerticalPopup : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'EnsureNodesAreSet' method.
		/// </summary>
		public static readonly StringName EnsureNodesAreSet = "EnsureNodesAreSet";

		/// <summary>
		/// Cached name for the 'SetText' method.
		/// </summary>
		public static readonly StringName SetText = "SetText";

		/// <summary>
		/// Cached name for the 'Close' method.
		/// </summary>
		public static readonly StringName Close = "Close";

		/// <summary>
		/// Cached name for the 'HideNoButton' method.
		/// </summary>
		public static readonly StringName HideNoButton = "HideNoButton";

		/// <summary>
		/// Cached name for the 'DisconnectSignals' method.
		/// </summary>
		public static readonly StringName DisconnectSignals = "DisconnectSignals";

		/// <summary>
		/// Cached name for the 'DisconnectHotkeys' method.
		/// </summary>
		public static readonly StringName DisconnectHotkeys = "DisconnectHotkeys";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'TitleLabel' property.
		/// </summary>
		public static readonly StringName TitleLabel = "TitleLabel";

		/// <summary>
		/// Cached name for the 'BodyLabel' property.
		/// </summary>
		public static readonly StringName BodyLabel = "BodyLabel";

		/// <summary>
		/// Cached name for the 'YesButton' property.
		/// </summary>
		public static readonly StringName YesButton = "YesButton";

		/// <summary>
		/// Cached name for the 'NoButton' property.
		/// </summary>
		public static readonly StringName NoButton = "NoButton";

		/// <summary>
		/// Cached name for the '_nodesAreSet' field.
		/// </summary>
		public static readonly StringName _nodesAreSet = "_nodesAreSet";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/vertical_popup");

	private bool _nodesAreSet;

	private Callable? _yesCallable;

	private Callable? _noCallable;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	private MegaLabel TitleLabel { get; set; }

	private MegaRichTextLabel BodyLabel { get; set; }

	public NPopupYesNoButton YesButton { get; private set; }

	public NPopupYesNoButton NoButton { get; private set; }

	public override void _Ready()
	{
		EnsureNodesAreSet();
	}

	/// <summary>
	/// This is to ensure that the node parameters are set. We have to do this because
	/// AddChildSafely may defer the call to add child (and thus defer the _Ready), and
	/// can lead to timing issues in NGenericPopup where we try to SetText before the label
	/// parameters have been set. While this is a vulnerability that can techinically happen
	/// for any node added via AddChildSafely, I think we are seeing a particularly high
	/// number of errors here because the vertical popup is bieng created at the start of the game
	/// for a startup errors.
	/// </summary>
	private void EnsureNodesAreSet()
	{
		if (!_nodesAreSet)
		{
			TitleLabel = GetNode<MegaLabel>("Header");
			BodyLabel = GetNode<MegaRichTextLabel>("Description");
			YesButton = GetNode<NPopupYesNoButton>("YesButton");
			NoButton = GetNode<NPopupYesNoButton>("NoButton");
			_nodesAreSet = true;
		}
	}

	public void SetText(LocString title, LocString body)
	{
		EnsureNodesAreSet();
		TitleLabel.SetTextAutoSize(title.GetFormattedText());
		BodyLabel.SetTextAutoSize(body.GetFormattedText());
	}

	/// <summary>
	/// Sets the popup text using raw strings instead of localization.
	/// Use this when localization may be broken (e.g., showing localization errors).
	/// </summary>
	public void SetText(string title, string body)
	{
		EnsureNodesAreSet();
		TitleLabel.SetTextAutoSize(title);
		BodyLabel.SetTextAutoSize(body);
	}

	/// <summary>
	/// Initializes the yes button.
	/// If this is not called, then the yes button is hidden.
	/// </summary>
	public void InitYesButton(LocString yesButton, Action<NButton> onPressed)
	{
		EnsureNodesAreSet();
		_yesCallable = Callable.From(onPressed);
		YesButton.IsYes = true;
		YesButton.SetText(yesButton.GetFormattedText());
		YesButton.Connect(NClickableControl.SignalName.Released, _yesCallable.Value);
		YesButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(Close));
	}

	public void InitNoButton(LocString noButton, Action<NButton> onPressed)
	{
		EnsureNodesAreSet();
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnsureNodesAreSet, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.EnsureNodesAreSet && args.Count == 0)
		{
			EnsureNodesAreSet();
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.EnsureNodesAreSet)
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

	/// <inheritdoc />
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
		if (name == PropertyName._nodesAreSet)
		{
			_nodesAreSet = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
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
		if (name == PropertyName._nodesAreSet)
		{
			value = VariantUtils.CreateFrom(in _nodesAreSet);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TitleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BodyLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.YesButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.NoButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._nodesAreSet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.TitleLabel, Variant.From<MegaLabel>(TitleLabel));
		info.AddProperty(PropertyName.BodyLabel, Variant.From<MegaRichTextLabel>(BodyLabel));
		info.AddProperty(PropertyName.YesButton, Variant.From<NPopupYesNoButton>(YesButton));
		info.AddProperty(PropertyName.NoButton, Variant.From<NPopupYesNoButton>(NoButton));
		info.AddProperty(PropertyName._nodesAreSet, Variant.From(in _nodesAreSet));
	}

	/// <inheritdoc />
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
		if (info.TryGetProperty(PropertyName._nodesAreSet, out var value5))
		{
			_nodesAreSet = value5.As<bool>();
		}
	}
}
