using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NRemoteMouseCursorContainer.cs")]
public class NRemoteMouseCursorContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName Deinitialize = "Deinitialize";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ForceUpdateAllCursors = "ForceUpdateAllCursors";

		public static readonly StringName GetCursorPosition = "GetCursorPosition";

		public static readonly StringName OnInputStateAdded = "OnInputStateAdded";

		public static readonly StringName OnInputStateRemoved = "OnInputStateRemoved";

		public static readonly StringName AddCursor = "AddCursor";

		public static readonly StringName OnInputStateChanged = "OnInputStateChanged";

		public static readonly StringName DrawingCursorStateChanged = "DrawingCursorStateChanged";

		public static readonly StringName GetDrawingMode = "GetDrawingMode";

		public static readonly StringName GetCursor = "GetCursor";

		public static readonly StringName RemoveCursor = "RemoveCursor";

		public static readonly StringName UpdateCursorVisibility = "UpdateCursorVisibility";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnGuiFocusChanged = "OnGuiFocusChanged";

		public static readonly StringName ApplyDebugUiVisibility = "ApplyDebugUiVisibility";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static bool _isDebugUiVisible = true;

	private PeerInputSynchronizer? _synchronizer;

	private readonly List<NRemoteMouseCursor> _cursors = new List<NRemoteMouseCursor>();

	public void Initialize(PeerInputSynchronizer synchronizer, IEnumerable<ulong> connectedPlayerIds)
	{
		if (_synchronizer != null)
		{
			Deinitialize();
		}
		_synchronizer = synchronizer;
		_synchronizer.StateAdded += OnInputStateAdded;
		_synchronizer.StateChanged += OnInputStateChanged;
		_synchronizer.StateRemoved += OnInputStateRemoved;
		_synchronizer.NetService.Disconnected += NetServiceDisconnected;
	}

	private void NetServiceDisconnected(NetErrorInfo _)
	{
		Deinitialize();
	}

	public void Deinitialize()
	{
		if (_synchronizer != null)
		{
			_synchronizer.StateAdded -= OnInputStateAdded;
			_synchronizer.StateChanged -= OnInputStateChanged;
			_synchronizer.StateRemoved -= OnInputStateRemoved;
			_synchronizer.NetService.Disconnected -= NetServiceDisconnected;
			_synchronizer.Dispose();
			_synchronizer = null;
		}
		foreach (NRemoteMouseCursor cursor in _cursors)
		{
			cursor.QueueFreeSafely();
		}
		_cursors.Clear();
	}

	public override void _Ready()
	{
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(OnGuiFocusChanged));
	}

	public override void _ExitTree()
	{
		Deinitialize();
	}

	public void ForceUpdateAllCursors()
	{
		foreach (NRemoteMouseCursor cursor in _cursors)
		{
			OnInputStateChanged(cursor.PlayerId);
		}
	}

	public Vector2 GetCursorPosition(ulong playerId)
	{
		return GetCursor(playerId).Position;
	}

	private void OnInputStateAdded(ulong playerId)
	{
		AddCursor(playerId);
	}

	private void OnInputStateRemoved(ulong playerId)
	{
		RemoveCursor(playerId);
	}

	private void AddCursor(ulong playerId)
	{
		if (playerId != _synchronizer?.NetService.NetId)
		{
			if (_cursors.Any((NRemoteMouseCursor c) => c.PlayerId == playerId))
			{
				Log.Error($"Tried to add cursor for player {playerId} twice!");
			}
			else
			{
				NRemoteMouseCursor nRemoteMouseCursor = NRemoteMouseCursor.Create(playerId);
				_cursors.Add(nRemoteMouseCursor);
				this.AddChildSafely(nRemoteMouseCursor);
			}
		}
	}

	private void OnInputStateChanged(ulong playerId)
	{
		if (playerId == _synchronizer?.NetService.NetId)
		{
			UpdateCursorVisibility();
			return;
		}
		Vector2 controlSpaceFocusPosition = _synchronizer.GetControlSpaceFocusPosition(playerId, this);
		NRemoteMouseCursor cursor = GetCursor(playerId);
		cursor.SetNextPosition(controlSpaceFocusPosition);
		cursor.UpdateImage(_synchronizer.GetMouseDown(playerId), GetDrawingMode(playerId));
		UpdateCursorVisibility();
	}

	public void DrawingCursorStateChanged(ulong playerId)
	{
		GetCursor(playerId)?.UpdateImage(_synchronizer.GetMouseDown(playerId), GetDrawingMode(playerId));
	}

	private static DrawingMode GetDrawingMode(ulong playerId)
	{
		if (NRun.Instance != null)
		{
			return NRun.Instance.GlobalUi.MapScreen.Drawings.GetDrawingMode(playerId);
		}
		return DrawingMode.None;
	}

	private NRemoteMouseCursor? GetCursor(ulong playerId)
	{
		return _cursors.FirstOrDefault((NRemoteMouseCursor c) => c.PlayerId == playerId);
	}

	private void RemoveCursor(ulong playerId)
	{
		NRemoteMouseCursor cursor = GetCursor(playerId);
		if (cursor != null)
		{
			cursor.QueueFreeSafely();
			_cursors.Remove(cursor);
		}
	}

	private void UpdateCursorVisibility()
	{
		NetScreenType screenType = _synchronizer.GetScreenType(_synchronizer.NetService.NetId);
		foreach (NRemoteMouseCursor cursor in _cursors)
		{
			NetScreenType screenType2 = _synchronizer.GetScreenType(cursor.PlayerId);
			bool flag = screenType == screenType2;
			bool flag2 = (((uint)(screenType2 - 5) <= 3u || screenType2 == NetScreenType.RemotePlayerExpandedState) ? true : false);
			bool flag3 = flag2;
			bool flag4 = screenType2 == NetScreenType.SharedRelicPicking;
			cursor.Visible = flag && !flag3 && !flag4;
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (_synchronizer == null)
		{
			return;
		}
		if (inputEvent.IsActionReleased(DebugHotkey.hideMpCursors))
		{
			_isDebugUiVisible = !_isDebugUiVisible;
			ApplyDebugUiVisibility();
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugUiVisible ? "Show MP Cursors" : "Hide MP Cursors"));
		}
		if (inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			_synchronizer.SyncLocalIsUsingController(isUsingController: false);
			if (!NGame.Instance.ReactionWheel.Visible)
			{
				_synchronizer.SyncLocalMousePos(inputEventMouseMotion.Position, this);
			}
		}
		else if (inputEvent is InputEventMouseButton inputEventMouseButton)
		{
			_synchronizer.SyncLocalIsUsingController(isUsingController: false);
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				_synchronizer.SyncLocalMouseDown(inputEventMouseButton.Pressed);
			}
		}
	}

	private void OnGuiFocusChanged(Control focused)
	{
		if (_synchronizer != null)
		{
			NControllerManager? instance = NControllerManager.Instance;
			if (instance != null && instance.IsUsingController)
			{
				_synchronizer.SyncLocalIsUsingController(isUsingController: true);
				_synchronizer.SyncLocalControllerFocus(focused.GlobalPosition + focused.Size * 0.5f, this);
			}
		}
	}

	private void ApplyDebugUiVisibility()
	{
		base.Visible = _isDebugUiVisible;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(17);
		list.Add(new MethodInfo(MethodName.Deinitialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ForceUpdateAllCursors, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetCursorPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnInputStateAdded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnInputStateRemoved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnInputStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DrawingCursorStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetDrawingMode, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCursor, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCursorVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnGuiFocusChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focused", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyDebugUiVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Deinitialize && args.Count == 0)
		{
			Deinitialize();
			ret = default(godot_variant);
			return true;
		}
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
		if (method == MethodName.ForceUpdateAllCursors && args.Count == 0)
		{
			ForceUpdateAllCursors();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCursorPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetCursorPosition(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.OnInputStateAdded && args.Count == 1)
		{
			OnInputStateAdded(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInputStateRemoved && args.Count == 1)
		{
			OnInputStateRemoved(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddCursor && args.Count == 1)
		{
			AddCursor(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInputStateChanged && args.Count == 1)
		{
			OnInputStateChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DrawingCursorStateChanged && args.Count == 1)
		{
			DrawingCursorStateChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetDrawingMode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<DrawingMode>(GetDrawingMode(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.GetCursor && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NRemoteMouseCursor>(GetCursor(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveCursor && args.Count == 1)
		{
			RemoveCursor(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCursorVisibility && args.Count == 0)
		{
			UpdateCursorVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnGuiFocusChanged && args.Count == 1)
		{
			OnGuiFocusChanged(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDebugUiVisibility && args.Count == 0)
		{
			ApplyDebugUiVisibility();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetDrawingMode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<DrawingMode>(GetDrawingMode(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Deinitialize)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ForceUpdateAllCursors)
		{
			return true;
		}
		if (method == MethodName.GetCursorPosition)
		{
			return true;
		}
		if (method == MethodName.OnInputStateAdded)
		{
			return true;
		}
		if (method == MethodName.OnInputStateRemoved)
		{
			return true;
		}
		if (method == MethodName.AddCursor)
		{
			return true;
		}
		if (method == MethodName.OnInputStateChanged)
		{
			return true;
		}
		if (method == MethodName.DrawingCursorStateChanged)
		{
			return true;
		}
		if (method == MethodName.GetDrawingMode)
		{
			return true;
		}
		if (method == MethodName.GetCursor)
		{
			return true;
		}
		if (method == MethodName.RemoveCursor)
		{
			return true;
		}
		if (method == MethodName.UpdateCursorVisibility)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnGuiFocusChanged)
		{
			return true;
		}
		if (method == MethodName.ApplyDebugUiVisibility)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
