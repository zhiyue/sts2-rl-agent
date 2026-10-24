using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// A popup to let the player know a terrible bug has occurred, and to upload a bug report.
/// The wording should be changed before we release.
/// Renders above the capstone screens (above top bar).
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NErrorPopup.cs")]
public class NErrorPopup : NVerticalPopup, IScreenContext
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NVerticalPopup.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'OnOkButtonPressed' method.
		/// </summary>
		public static readonly StringName OnOkButtonPressed = "OnOkButtonPressed";

		/// <summary>
		/// Cached name for the 'OnCancelButtonPressed' method.
		/// </summary>
		public static readonly StringName OnCancelButtonPressed = "OnCancelButtonPressed";

		/// <summary>
		/// Cached name for the 'OnReportBugButtonPressed' method.
		/// </summary>
		public static readonly StringName OnReportBugButtonPressed = "OnReportBugButtonPressed";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NVerticalPopup.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_verticalPopup' field.
		/// </summary>
		public static readonly StringName _verticalPopup = "_verticalPopup";

		/// <summary>
		/// Cached name for the '_title' field.
		/// </summary>
		public static readonly StringName _title = "_title";

		/// <summary>
		/// Cached name for the '_body' field.
		/// </summary>
		public static readonly StringName _body = "_body";

		/// <summary>
		/// Cached name for the '_showReportBugButton' field.
		/// </summary>
		public static readonly StringName _showReportBugButton = "_showReportBugButton";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NVerticalPopup.SignalName
	{
	}

	private NVerticalPopup _verticalPopup;

	private string _title;

	private string _body;

	private LocString? _cancel;

	private bool _showReportBugButton;

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/error_popup");

	public Control? DefaultFocusedControl => null;

	public new static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public override void _Ready()
	{
		_verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		_verticalPopup.SetText(_title, _body);
		if (_showReportBugButton)
		{
			_verticalPopup.InitYesButton(new LocString("main_menu_ui", "NETWORK_ERROR.report_bug"), OnReportBugButtonPressed);
		}
		else
		{
			_verticalPopup.InitYesButton(new LocString("main_menu_ui", "GENERIC_POPUP.ok"), OnOkButtonPressed);
		}
		if (_cancel != null)
		{
			_verticalPopup.InitNoButton(_cancel, OnCancelButtonPressed);
		}
		else
		{
			_verticalPopup.HideNoButton();
		}
	}

	public static NErrorPopup? Create(NetErrorInfo info)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (info.SelfInitiated && info.GetReason() == NetError.Quit)
		{
			return null;
		}
		bool showReportBugButton;
		return Create(new LocString("main_menu_ui", "NETWORK_ERROR.header"), LocStringFromNetError(info, out showReportBugButton), null, showReportBugButton);
	}

	public static NErrorPopup? Create(LocString title, LocString body, LocString? cancel, bool showReportBugButton)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NErrorPopup nErrorPopup = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NErrorPopup>(PackedScene.GenEditState.Disabled);
		nErrorPopup._title = title.GetFormattedText();
		nErrorPopup._body = body.GetFormattedText();
		nErrorPopup._showReportBugButton = showReportBugButton;
		nErrorPopup._cancel = cancel;
		return nErrorPopup;
	}

	/// <summary>
	/// Creates an error popup with hardcoded English text (bypassing localization).
	/// Use this when localization may be broken (e.g., showing localization errors).
	/// </summary>
	public static NErrorPopup? Create(string title, string body, bool showReportBugButton)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NErrorPopup nErrorPopup = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NErrorPopup>(PackedScene.GenEditState.Disabled);
		nErrorPopup._title = title;
		nErrorPopup._body = body;
		nErrorPopup._showReportBugButton = showReportBugButton;
		return nErrorPopup;
	}

	private static LocString LocStringFromNetError(NetErrorInfo info, out bool showReportBugButton)
	{
		NetError reason = info.GetReason();
		string text = default(string);
		switch (reason)
		{
		case NetError.None:
			text = null;
			break;
		case NetError.QuitGameOver:
			text = null;
			break;
		case NetError.CancelledJoin:
			text = null;
			break;
		case NetError.LobbyFull:
			text = "NETWORK_ERROR.LOBBY_FULL.body";
			break;
		case NetError.Quit:
			text = "NETWORK_ERROR.QUIT.body";
			break;
		case NetError.HostAbandoned:
			text = "NETWORK_ERROR.HOST_ABANDONED.body";
			break;
		case NetError.Kicked:
			text = "NETWORK_ERROR.KICKED.body";
			break;
		case NetError.InvalidJoin:
			text = "NETWORK_ERROR.INVALID_JOIN.body";
			break;
		case NetError.RunInProgress:
			text = "NETWORK_ERROR.RUN_IN_PROGRESS.body";
			break;
		case NetError.StateDivergence:
			text = "NETWORK_ERROR.STATE_DIVERGENCE.body";
			break;
		case NetError.ModMismatch:
			text = "NETWORK_ERROR.MOD_MISMATCH.body";
			break;
		case NetError.JoinBlockedByUser:
			text = "NETWORK_ERROR.JOIN_BLOCKED_BY_USER.body";
			break;
		case NetError.NoInternet:
			text = "NETWORK_ERROR.NO_INTERNET.body";
			break;
		case NetError.Timeout:
			text = "NETWORK_ERROR.TIMEOUT.body";
			break;
		case NetError.HandshakeTimeout:
			text = "NETWORK_ERROR.TIMEOUT.body";
			break;
		case NetError.InternalError:
			text = "NETWORK_ERROR.INTERNAL_ERROR.body";
			break;
		case NetError.UnknownNetworkError:
			text = "NETWORK_ERROR.UNKNOWN_ERROR.body";
			break;
		case NetError.RateLimited:
			text = "NETWORK_ERROR.RATE_LIMITED.body";
			break;
		case NetError.TryAgainLater:
			text = "NETWORK_ERROR.TRY_AGAIN_LATER.body";
			break;
		case NetError.SecureConnectionFailed:
			text = "NETWORK_ERROR.SECURE_CONNECTION_FAILED.body";
			break;
		case NetError.FailedToHost:
			text = "NETWORK_ERROR.FAILED_TO_HOST.body";
			break;
		case NetError.NotInSaveGame:
			text = "NETWORK_ERROR.NOT_IN_SAVE_GAME.body";
			break;
		case NetError.VersionMismatch:
			text = "NETWORK_ERROR.VERSION_MISMATCH.body";
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(reason);
			break;
		}
		string text2 = text;
		bool flag = ((reason == NetError.None || reason == NetError.StateDivergence || (uint)(reason - 17) <= 1u) ? true : false);
		showReportBugButton = flag;
		if (text2 == null)
		{
			Log.Error($"Invalid net error passed to {"NErrorPopup"}: {info}!");
			text2 = "NETWORK_ERROR.INTERNAL_ERROR.body";
			showReportBugButton = true;
		}
		LocString locString = new LocString("main_menu_ui", text2);
		locString.Add("info", info.GetErrorString());
		return locString;
	}

	private void OnOkButtonPressed(NButton _)
	{
		this.QueueFreeSafely();
	}

	private void OnCancelButtonPressed(NButton _)
	{
		this.QueueFreeSafely();
	}

	private void OnReportBugButtonPressed(NButton _)
	{
		TaskHelper.RunSafely(OpenFeedbackScreen());
	}

	private async Task OpenFeedbackScreen()
	{
		SceneTree sceneTree = GetTree();
		this.QueueFreeSafely();
		await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
		await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
		await NFeedbackScreenOpener.Instance.OpenFeedbackScreen();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "title", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "body", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "showReportBugButton", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnOkButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCancelButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnReportBugButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NErrorPopup>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName.OnOkButtonPressed && args.Count == 1)
		{
			OnOkButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCancelButtonPressed && args.Count == 1)
		{
			OnCancelButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReportBugButtonPressed && args.Count == 1)
		{
			OnReportBugButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NErrorPopup>(Create(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.OnOkButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnCancelButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnReportBugButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._verticalPopup)
		{
			_verticalPopup = VariantUtils.ConvertTo<NVerticalPopup>(in value);
			return true;
		}
		if (name == PropertyName._title)
		{
			_title = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._body)
		{
			_body = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._showReportBugButton)
		{
			_showReportBugButton = VariantUtils.ConvertTo<bool>(in value);
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
		if (name == PropertyName._verticalPopup)
		{
			value = VariantUtils.CreateFrom(in _verticalPopup);
			return true;
		}
		if (name == PropertyName._title)
		{
			value = VariantUtils.CreateFrom(in _title);
			return true;
		}
		if (name == PropertyName._body)
		{
			value = VariantUtils.CreateFrom(in _body);
			return true;
		}
		if (name == PropertyName._showReportBugButton)
		{
			value = VariantUtils.CreateFrom(in _showReportBugButton);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._title, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._showReportBugButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._verticalPopup, Variant.From(in _verticalPopup));
		info.AddProperty(PropertyName._title, Variant.From(in _title));
		info.AddProperty(PropertyName._body, Variant.From(in _body));
		info.AddProperty(PropertyName._showReportBugButton, Variant.From(in _showReportBugButton));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._verticalPopup, out var value))
		{
			_verticalPopup = value.As<NVerticalPopup>();
		}
		if (info.TryGetProperty(PropertyName._title, out var value2))
		{
			_title = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._body, out var value3))
		{
			_body = value3.As<string>();
		}
		if (info.TryGetProperty(PropertyName._showReportBugButton, out var value4))
		{
			_showReportBugButton = value4.As<bool>();
		}
	}
}
