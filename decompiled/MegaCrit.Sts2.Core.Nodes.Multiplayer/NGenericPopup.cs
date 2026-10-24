using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

/// <summary>
/// Vertical popup used in several places to get confirmation for actions.
/// Renders above the capstone screens (above top bar).
/// </summary>
[ScriptPath("res://src/Core/Nodes/Multiplayer/NGenericPopup.cs")]
public class NGenericPopup : Control, IScreenContext
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'OnYesButtonPressed' method.
		/// </summary>
		public static readonly StringName OnYesButtonPressed = "OnYesButtonPressed";

		/// <summary>
		/// Cached name for the 'OnNoButtonPressed' method.
		/// </summary>
		public static readonly StringName OnNoButtonPressed = "OnNoButtonPressed";
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
		/// Cached name for the '_verticalPopup' field.
		/// </summary>
		public static readonly StringName _verticalPopup = "_verticalPopup";

		/// <summary>
		/// Cached name for the '_steamId' field.
		/// </summary>
		public static readonly StringName _steamId = "_steamId";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/generic_popup");

	private NVerticalPopup _verticalPopup;

	private TaskCompletionSource<bool> _confirmationCompletionSource;

	private ulong _steamId;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public Control? DefaultFocusedControl => null;

	public static NGenericPopup? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NGenericPopup>(PackedScene.GenEditState.Disabled);
	}

	public Task<bool> WaitForConfirmation(LocString body, LocString header, LocString? noButton, LocString yesButton)
	{
		_verticalPopup = GetNode<NVerticalPopup>("VerticalPopup");
		_confirmationCompletionSource = new TaskCompletionSource<bool>();
		_verticalPopup.SetText(header, body);
		_verticalPopup.InitYesButton(yesButton, OnYesButtonPressed);
		if (noButton != null)
		{
			_verticalPopup.InitNoButton(noButton, OnNoButtonPressed);
		}
		else
		{
			_verticalPopup.HideNoButton();
		}
		return _confirmationCompletionSource.Task;
	}

	private void OnYesButtonPressed(NButton _)
	{
		_confirmationCompletionSource.SetResult(result: true);
		this.QueueFreeSafely();
	}

	private void OnNoButtonPressed(NButton _)
	{
		_confirmationCompletionSource.SetResult(result: false);
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.OnYesButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnNoButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NGenericPopup>(Create());
			return true;
		}
		if (method == MethodName.OnYesButtonPressed && args.Count == 1)
		{
			OnYesButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnNoButtonPressed && args.Count == 1)
		{
			OnNoButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NGenericPopup>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.OnYesButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnNoButtonPressed)
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
		if (name == PropertyName._steamId)
		{
			_steamId = VariantUtils.ConvertTo<ulong>(in value);
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
		if (name == PropertyName._steamId)
		{
			value = VariantUtils.CreateFrom(in _steamId);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalPopup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._steamId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._verticalPopup, Variant.From(in _verticalPopup));
		info.AddProperty(PropertyName._steamId, Variant.From(in _steamId));
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
		if (info.TryGetProperty(PropertyName._steamId, out var value2))
		{
			_steamId = value2.As<ulong>();
		}
	}
}
