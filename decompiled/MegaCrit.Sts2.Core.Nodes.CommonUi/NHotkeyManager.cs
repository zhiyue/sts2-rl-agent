using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// Class that manages Hotkeys/shortcuts for various buttons and UI in the game.
/// Manages which buttons get priority if multiple of them share the same Hotkey
/// (typically the last one added gets priority)
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NHotkeyManager.cs")]
public class NHotkeyManager : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the 'AddBlockingScreen' method.
		/// </summary>
		public static readonly StringName AddBlockingScreen = "AddBlockingScreen";

		/// <summary>
		/// Cached name for the 'RemoveBlockingScreen' method.
		/// </summary>
		public static readonly StringName RemoveBlockingScreen = "RemoveBlockingScreen";

		/// <summary>
		/// Cached name for the '_UnhandledInput' method.
		/// </summary>
		public new static readonly StringName _UnhandledInput = "_UnhandledInput";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private readonly Dictionary<StringName, List<Action>> _hotkeyPressedBindings = new Dictionary<StringName, List<Action>>();

	private readonly Dictionary<StringName, List<Action>> _hotkeyReleasedBindings = new Dictionary<StringName, List<Action>>();

	private Dictionary<Node, Action> _blockingScreens = new Dictionary<Node, Action>();

	public static NHotkeyManager? Instance
	{
		get
		{
			if (NGame.Instance == null)
			{
				return null;
			}
			return NGame.Instance.HotkeyManager;
		}
	}

	public void PushHotkeyPressedBinding(string hotkey, Action action)
	{
		if (!_hotkeyPressedBindings.ContainsKey(hotkey))
		{
			_hotkeyPressedBindings.Add(hotkey, new List<Action>());
		}
		if (!_hotkeyPressedBindings[hotkey].Contains(action))
		{
			_hotkeyPressedBindings[hotkey].Add(action);
		}
	}

	public void RemoveHotkeyPressedBinding(string hotkey, Action action)
	{
		if (_hotkeyPressedBindings.TryGetValue(hotkey, out List<Action> value))
		{
			value.Remove(action);
			if (_hotkeyPressedBindings[hotkey].Count == 0)
			{
				_hotkeyPressedBindings.Remove(hotkey);
			}
		}
	}

	public void PushHotkeyReleasedBinding(string hotkey, Action action)
	{
		if (!_hotkeyReleasedBindings.ContainsKey(hotkey))
		{
			_hotkeyReleasedBindings.Add(hotkey, new List<Action>());
		}
		if (!_hotkeyReleasedBindings[hotkey].Contains(action))
		{
			_hotkeyReleasedBindings[hotkey].Add(action);
		}
	}

	public void RemoveHotkeyReleasedBinding(string hotkey, Action action)
	{
		if (_hotkeyReleasedBindings.TryGetValue(hotkey, out List<Action> value))
		{
			value.Remove(action);
			if (_hotkeyReleasedBindings[hotkey].Count == 0)
			{
				_hotkeyReleasedBindings.Remove(hotkey);
			}
		}
	}

	/// <summary>
	/// we want to block all previous hotkeys while this screen is open
	/// New hotkeys can be added on top though.
	/// ex: we don't want to be able to hotkey to view our draw pile if the pause menu is open
	/// </summary>
	/// <param name="screen">Screen that is block all previous hotkey input</param>
	public void AddBlockingScreen(Node screen)
	{
		Action action = delegate
		{
		};
		string[] allInputs = MegaInput.AllInputs;
		foreach (string hotkey in allInputs)
		{
			PushHotkeyPressedBinding(hotkey, action);
		}
		string[] allInputs2 = MegaInput.AllInputs;
		foreach (string hotkey2 in allInputs2)
		{
			PushHotkeyReleasedBinding(hotkey2, action);
		}
		_blockingScreens.Add(screen, action);
	}

	/// <summary>
	/// Removes the all hotkey blockers related to a screen.
	/// </summary>
	/// <param name="screen">Screen that want to remove the hotkey blockage from</param>
	public void RemoveBlockingScreen(Node screen)
	{
		if (_blockingScreens.TryGetValue(screen, out Action value))
		{
			string[] allInputs = MegaInput.AllInputs;
			foreach (string hotkey in allInputs)
			{
				RemoveHotkeyPressedBinding(hotkey, value);
			}
			string[] allInputs2 = MegaInput.AllInputs;
			foreach (string hotkey2 in allInputs2)
			{
				RemoveHotkeyReleasedBinding(hotkey2, value);
			}
			_blockingScreens.Remove(screen);
		}
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (!NGame.IsGameFocusedWindow() || NDevConsole.Instance.Visible)
		{
			return;
		}
		Viewport viewport = GetViewport();
		Control control = viewport?.GuiGetFocusOwner();
		if (control != null && ((control is LineEdit lineEdit && lineEdit.IsEditing()) || (control is NMegaTextEdit nMegaTextEdit && nMegaTextEdit.IsEditing())))
		{
			return;
		}
		foreach (KeyValuePair<StringName, List<Action>> hotkeyPressedBinding in _hotkeyPressedBindings)
		{
			if (inputEvent.IsActionPressed(hotkeyPressedBinding.Key) && !inputEvent.IsEcho())
			{
				Action action = hotkeyPressedBinding.Value.LastOrDefault();
				if (action != null)
				{
					Callable.From(action.Invoke).CallDeferred();
					viewport?.SetInputAsHandled();
				}
			}
		}
		foreach (KeyValuePair<StringName, List<Action>> hotkeyReleasedBinding in _hotkeyReleasedBindings)
		{
			if (inputEvent.IsActionReleased(hotkeyReleasedBinding.Key))
			{
				Action action2 = hotkeyReleasedBinding.Value.LastOrDefault();
				if (action2 != null)
				{
					Callable.From(action2.Invoke).CallDeferred();
					viewport?.SetInputAsHandled();
				}
			}
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
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.AddBlockingScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "screen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveBlockingScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "screen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.AddBlockingScreen && args.Count == 1)
		{
			AddBlockingScreen(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveBlockingScreen && args.Count == 1)
		{
			RemoveBlockingScreen(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.AddBlockingScreen)
		{
			return true;
		}
		if (method == MethodName.RemoveBlockingScreen)
		{
			return true;
		}
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
