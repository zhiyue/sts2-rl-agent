using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.MapDrawing;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapDrawings.cs")]
public class NMapDrawings : Control
{
	private class DrawingState
	{
		public DrawingMode? overrideDrawingMode;

		public DrawingMode drawingMode;

		public ulong playerId;

		public Line2D? currentlyDrawingLine;

		public required SubViewport drawViewport;

		public bool IsDrawing => currentlyDrawingLine != null;

		public DrawingMode CurrentDrawingMode => overrideDrawingMode ?? drawingMode;
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateCurrentLinePositionLocal = "UpdateCurrentLinePositionLocal";

		public static readonly StringName StopLineLocal = "StopLineLocal";

		public static readonly StringName SetDrawingModeLocal = "SetDrawingModeLocal";

		public static readonly StringName ClearDrawnLinesLocal = "ClearDrawnLinesLocal";

		public static readonly StringName IsDrawing = "IsDrawing";

		public static readonly StringName IsLocalDrawing = "IsLocalDrawing";

		public static readonly StringName GetDrawingMode = "GetDrawingMode";

		public static readonly StringName GetLocalDrawingMode = "GetLocalDrawingMode";

		public static readonly StringName ToNetPosition = "ToNetPosition";

		public static readonly StringName FromNetPosition = "FromNetPosition";

		public static readonly StringName ClearAllLines = "ClearAllLines";

		public static readonly StringName OnPlayerScreenChanged = "OnPlayerScreenChanged";

		public static readonly StringName TrySendSyncMessage = "TrySendSyncMessage";

		public static readonly StringName SendSyncMessage = "SendSyncMessage";

		public static readonly StringName UpdateLocalCursor = "UpdateLocalCursor";

		public static readonly StringName RepositionBasedOnBackground = "RepositionBasedOnBackground";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _lineDrawScene = "_lineDrawScene";

		public static readonly StringName _lineEraseScene = "_lineEraseScene";

		public static readonly StringName _cursorManager = "_cursorManager";

		public static readonly StringName _eraserMaterial = "_eraserMaterial";

		public static readonly StringName _defaultSize = "_defaultSize";

		public static readonly StringName _lastMessageMsec = "_lastMessageMsec";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const int _minUpdateMsec = 50;

	private static readonly string _lineDrawScenePath = SceneHelper.GetScenePath("screens/map/map_line_draw");

	private static readonly string _lineEraseScenePath = SceneHelper.GetScenePath("screens/map/map_line_erase");

	private static readonly string _playerDrawingPath = SceneHelper.GetScenePath("screens/map/map_drawing");

	public const string drawingCursorPath = "res://images/packed/common_ui/cursor_quill.png";

	public const string drawingCursorTiltedPath = "res://images/packed/common_ui/cursor_quill_tilted.png";

	public static readonly Vector2 drawingCursorHotspot = new Vector2(2f, 56f);

	public const string erasingCursorPath = "res://images/packed/common_ui/cursor_eraser.png";

	public const string erasingCursorTiltedPath = "res://images/packed/common_ui/cursor_eraser_tilted.png";

	public static readonly Vector2 erasingCursorHotspot = new Vector2(24f, 58f);

	private const float _minimumPointDistance = 2f;

	private INetGameService _netService;

	private IPlayerCollection _playerCollection;

	private PeerInputSynchronizer _inputSynchronizer;

	private PackedScene _lineDrawScene;

	private PackedScene _lineEraseScene;

	private NCursorManager _cursorManager;

	private Material _eraserMaterial;

	private Vector2 _defaultSize;

	private readonly List<DrawingState> _drawingStates = new List<DrawingState>();

	private MapDrawingMessage? _queuedMessage;

	private ulong _lastMessageMsec;

	private Task? _sendMessageTask;

	private static IEnumerable<string> SelfAssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[7] { _lineDrawScenePath, _lineEraseScenePath, _playerDrawingPath, "res://images/packed/common_ui/cursor_quill.png", "res://images/packed/common_ui/cursor_quill_tilted.png", "res://images/packed/common_ui/cursor_eraser.png", "res://images/packed/common_ui/cursor_eraser_tilted.png" });

	public static IEnumerable<string> AssetPaths => SelfAssetPaths.Concat(NMapDrawButton.AssetPaths);

	public override void _Ready()
	{
		_lineDrawScene = PreloadManager.Cache.GetScene(_lineDrawScenePath);
		_lineEraseScene = PreloadManager.Cache.GetScene(_lineEraseScenePath);
		_cursorManager = NGame.Instance.CursorManager;
		Line2D line2D = _lineEraseScene.Instantiate<Line2D>(PackedScene.GenEditState.Disabled);
		_eraserMaterial = line2D.Material;
		line2D.QueueFreeSafely();
		_defaultSize = base.Size;
	}

	public void Initialize(INetGameService netService, IPlayerCollection playerCollection, PeerInputSynchronizer inputSynchronizer)
	{
		_netService = netService;
		_playerCollection = playerCollection;
		_inputSynchronizer = inputSynchronizer;
		_netService.RegisterMessageHandler<MapDrawingMessage>(HandleDrawingMessage);
		_netService.RegisterMessageHandler<ClearMapDrawingsMessage>(HandleClearMapDrawingsMessage);
		_netService.RegisterMessageHandler<MapDrawingModeChangedMessage>(HandleMapDrawingModeChangedMessage);
		inputSynchronizer.ScreenChanged += OnPlayerScreenChanged;
	}

	public override void _ExitTree()
	{
		_netService.UnregisterMessageHandler<MapDrawingMessage>(HandleDrawingMessage);
		_netService.UnregisterMessageHandler<ClearMapDrawingsMessage>(HandleClearMapDrawingsMessage);
		_netService.UnregisterMessageHandler<MapDrawingModeChangedMessage>(HandleMapDrawingModeChangedMessage);
		_inputSynchronizer.ScreenChanged -= OnPlayerScreenChanged;
	}

	public void BeginLineLocal(Vector2 position, DrawingMode? overrideDrawingMode)
	{
		BeginLine(GetDrawingStateForPlayer(_netService.NetId), position, overrideDrawingMode);
		NetMapDrawingEvent ev = new NetMapDrawingEvent
		{
			type = MapDrawingEventType.BeginLine,
			position = ToNetPosition(position),
			overrideDrawingMode = overrideDrawingMode
		};
		QueueOrSendEvent(ev);
	}

	public void UpdateCurrentLinePositionLocal(Vector2 position)
	{
		DrawingState drawingStateForPlayer = GetDrawingStateForPlayer(_netService.NetId);
		UpdateCurrentLinePosition(drawingStateForPlayer, position);
		NetMapDrawingEvent ev = new NetMapDrawingEvent
		{
			type = MapDrawingEventType.ContinueLine,
			position = ToNetPosition(position),
			overrideDrawingMode = drawingStateForPlayer.overrideDrawingMode
		};
		QueueOrSendEvent(ev);
	}

	public void StopLineLocal()
	{
		StopDrawingLine(GetDrawingStateForPlayer(_netService.NetId));
		NetMapDrawingEvent ev = new NetMapDrawingEvent
		{
			type = MapDrawingEventType.EndLine
		};
		QueueOrSendEvent(ev);
	}

	public void SetDrawingModeLocal(DrawingMode drawingMode)
	{
		SetDrawingMode(GetDrawingStateForPlayer(_netService.NetId), drawingMode);
		MapDrawingModeChangedMessage message = new MapDrawingModeChangedMessage
		{
			drawingMode = drawingMode
		};
		_netService.SendMessage(message);
		UpdateLocalCursor();
	}

	public void ClearDrawnLinesLocal()
	{
		ClearAllLinesForPlayer(GetDrawingStateForPlayer(_netService.NetId));
		UpdateLocalCursor();
		_netService.SendMessage(default(ClearMapDrawingsMessage));
	}

	public bool IsDrawing(ulong playerId)
	{
		return GetDrawingStateForPlayer(playerId).IsDrawing;
	}

	public bool IsLocalDrawing()
	{
		return GetDrawingStateForPlayer(_netService.NetId).IsDrawing;
	}

	public DrawingMode GetDrawingMode(ulong playerId)
	{
		return GetDrawingStateForPlayer(playerId).CurrentDrawingMode;
	}

	public DrawingMode GetLocalDrawingMode(bool useOverride = true)
	{
		if (!useOverride)
		{
			return GetDrawingStateForPlayer(_netService.NetId).drawingMode;
		}
		return GetDrawingStateForPlayer(_netService.NetId).CurrentDrawingMode;
	}

	private void QueueOrSendEvent(NetMapDrawingEvent ev)
	{
		if (_queuedMessage == null)
		{
			_queuedMessage = new MapDrawingMessage();
		}
		if (!_queuedMessage.TryAddEvent(ev))
		{
			_queuedMessage.drawingMode = GetDrawingStateForPlayer(_netService.NetId).drawingMode;
			_netService.SendMessage(_queuedMessage);
			_queuedMessage = new MapDrawingMessage();
			if (!_queuedMessage.TryAddEvent(ev))
			{
				throw new InvalidOperationException();
			}
		}
		TrySendSyncMessage();
		UpdateLocalCursor();
	}

	private Vector2 ToNetPosition(Vector2 pos)
	{
		pos.X -= base.Size.X * 0.5f;
		pos /= new Vector2(960f, base.Size.Y);
		return pos;
	}

	private Vector2 FromNetPosition(Vector2 pos)
	{
		pos *= new Vector2(960f, base.Size.Y);
		pos.X += base.Size.X * 0.5f;
		return pos;
	}

	private void HandleDrawingMessage(MapDrawingMessage message, ulong senderId)
	{
		DrawingState drawingStateForPlayer = GetDrawingStateForPlayer(senderId);
		foreach (NetMapDrawingEvent @event in message.Events)
		{
			if (@event.type == MapDrawingEventType.BeginLine)
			{
				if (GetDrawingMode(senderId) != DrawingMode.None)
				{
					StopDrawingLine(drawingStateForPlayer);
				}
				BeginLine(drawingStateForPlayer, FromNetPosition(@event.position), @event.overrideDrawingMode);
			}
			else if (@event.type == MapDrawingEventType.ContinueLine)
			{
				if (!drawingStateForPlayer.IsDrawing)
				{
					if (message.drawingMode.HasValue && drawingStateForPlayer.drawingMode != message.drawingMode)
					{
						SetDrawingMode(drawingStateForPlayer, message.drawingMode.Value);
					}
					BeginLine(drawingStateForPlayer, FromNetPosition(@event.position), @event.overrideDrawingMode);
				}
				UpdateCurrentLinePosition(drawingStateForPlayer, FromNetPosition(@event.position));
			}
			else
			{
				StopDrawingLine(drawingStateForPlayer);
			}
		}
	}

	private void HandleClearMapDrawingsMessage(ClearMapDrawingsMessage message, ulong senderId)
	{
		ClearAllLinesForPlayer(GetDrawingStateForPlayer(senderId));
	}

	private void HandleMapDrawingModeChangedMessage(MapDrawingModeChangedMessage message, ulong senderId)
	{
		SetDrawingMode(GetDrawingStateForPlayer(senderId), message.drawingMode);
	}

	private void BeginLine(DrawingState state, Vector2 position, DrawingMode? overrideDrawingMode)
	{
		Player player = _playerCollection.GetPlayer(state.playerId);
		DrawingMode drawingMode = overrideDrawingMode ?? state.drawingMode;
		if (drawingMode == DrawingMode.None)
		{
			throw new InvalidOperationException($"Player {state.playerId} is not currently in a drawing mode and no override was passed!");
		}
		state.overrideDrawingMode = overrideDrawingMode;
		state.currentlyDrawingLine = CreateLineForPlayer(player, drawingMode == DrawingMode.Erasing);
		state.currentlyDrawingLine.AddPoint(position * 0.5f);
		state.currentlyDrawingLine.AddPoint(position * 0.5f + new Vector2(0f, 0.5f));
		state.drawViewport.AddChildSafely(state.currentlyDrawingLine);
		NGame.Instance.RemoteCursorContainer.DrawingCursorStateChanged(state.playerId);
	}

	private Line2D CreateLineForPlayer(Player player, bool isErasing)
	{
		PackedScene packedScene = (isErasing ? _lineEraseScene : _lineDrawScene);
		Line2D line2D = packedScene.Instantiate<Line2D>(PackedScene.GenEditState.Disabled);
		line2D.DefaultColor = player.Character.MapDrawingColor;
		line2D.ClearPoints();
		line2D.Position = Vector2.Zero;
		return line2D;
	}

	private void StopDrawingLine(DrawingState state)
	{
		state.overrideDrawingMode = null;
		state.currentlyDrawingLine = null;
		NGame.Instance.RemoteCursorContainer.DrawingCursorStateChanged(state.playerId);
	}

	private void SetDrawingMode(DrawingState state, DrawingMode drawingMode)
	{
		if (state.drawingMode != drawingMode)
		{
			state.drawingMode = drawingMode;
			NGame.Instance.RemoteCursorContainer.DrawingCursorStateChanged(state.playerId);
		}
	}

	private void UpdateCurrentLinePosition(DrawingState state, Vector2 position)
	{
		if (state.currentlyDrawingLine == null)
		{
			throw new InvalidOperationException($"Tried to update current line position for player {state.playerId}, but they are not currently drawing a line!");
		}
		Vector2 vector = state.currentlyDrawingLine.Points[^1];
		if (!(vector.DistanceSquaredTo(position) < 4f))
		{
			state.currentlyDrawingLine.AddPoint(position * 0.5f);
		}
	}

	private DrawingState GetDrawingStateForPlayer(ulong playerId)
	{
		DrawingState drawingState = _drawingStates.FirstOrDefault((DrawingState s) => s.playerId == playerId);
		if (drawingState == null)
		{
			Control control = PreloadManager.Cache.GetScene(_playerDrawingPath).Instantiate<Control>(PackedScene.GenEditState.Disabled);
			this.AddChildSafely(control);
			drawingState = new DrawingState
			{
				playerId = playerId,
				drawViewport = control.GetNode<SubViewport>("DrawViewport")
			};
			TaskHelper.RunSafely(SetVisibleLater(control));
			_drawingStates.Add(drawingState);
		}
		return drawingState;
	}

	private async Task SetVisibleLater(Control mapDrawingScene)
	{
		TextureRect drawingTexture = mapDrawingScene.GetNode<TextureRect>("DrawViewportTextureRect");
		SubViewport drawViewport = mapDrawingScene.GetNode<SubViewport>("DrawViewport");
		drawViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
		drawingTexture.Visible = false;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		drawingTexture.Visible = true;
		drawViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible;
	}

	public void ClearAllLines()
	{
		foreach (DrawingState drawingState in _drawingStates)
		{
			foreach (Line2D item in drawingState.drawViewport.GetChildren().OfType<Line2D>())
			{
				item.QueueFreeSafely();
			}
		}
	}

	public SerializableMapDrawings GetSerializableMapDrawings()
	{
		SerializableMapDrawings serializableMapDrawings = new SerializableMapDrawings();
		foreach (DrawingState drawingState in _drawingStates)
		{
			SerializablePlayerMapDrawings serializablePlayerMapDrawings = new SerializablePlayerMapDrawings
			{
				playerId = drawingState.playerId
			};
			serializableMapDrawings.drawings.Add(serializablePlayerMapDrawings);
			foreach (Line2D item in drawingState.drawViewport.GetChildren().OfType<Line2D>())
			{
				SerializableMapDrawingLine serializableMapDrawingLine = new SerializableMapDrawingLine
				{
					mapPoints = new List<Vector2>()
				};
				serializableMapDrawingLine.isEraser = item.Material == _eraserMaterial;
				serializablePlayerMapDrawings.lines.Add(serializableMapDrawingLine);
				Vector2[] points = item.Points;
				foreach (Vector2 pos in points)
				{
					serializableMapDrawingLine.mapPoints.Add(ToNetPosition(pos));
				}
			}
		}
		return serializableMapDrawings;
	}

	public void LoadDrawings(SerializableMapDrawings drawings)
	{
		foreach (SerializablePlayerMapDrawings drawing in drawings.drawings)
		{
			Player player = _playerCollection.GetPlayer(drawing.playerId);
			if (player == null)
			{
				Log.Warn($"Player {drawing.playerId} has map drawings, but doesn't exist in the run!");
				continue;
			}
			DrawingState drawingStateForPlayer = GetDrawingStateForPlayer(drawing.playerId);
			foreach (SerializableMapDrawingLine line in drawing.lines)
			{
				Line2D line2D = CreateLineForPlayer(player, line.isEraser);
				drawingStateForPlayer.drawViewport.AddChildSafely(line2D);
				foreach (Vector2 mapPoint in line.mapPoints)
				{
					line2D.AddPoint(FromNetPosition(mapPoint));
				}
			}
		}
	}

	private void ClearAllLinesForPlayer(DrawingState state)
	{
		foreach (Line2D item in state.drawViewport.GetChildren().OfType<Line2D>())
		{
			item.QueueFreeSafely();
		}
		SetDrawingMode(state, DrawingMode.None);
	}

	private void OnPlayerScreenChanged(ulong playerId, NetScreenType oldScreenType)
	{
		if (playerId == _netService.NetId)
		{
			return;
		}
		NetScreenType screenType = _inputSynchronizer.GetScreenType(playerId);
		if (oldScreenType == NetScreenType.Map && screenType != NetScreenType.Map)
		{
			DrawingState drawingStateForPlayer = GetDrawingStateForPlayer(playerId);
			if (drawingStateForPlayer.IsDrawing)
			{
				StopDrawingLine(drawingStateForPlayer);
			}
			if (drawingStateForPlayer.drawingMode != DrawingMode.None)
			{
				SetDrawingMode(drawingStateForPlayer, DrawingMode.None);
			}
		}
	}

	private void TrySendSyncMessage()
	{
		if (_sendMessageTask == null && _netService.IsConnected)
		{
			int num = (int)(_lastMessageMsec + 50 - Time.GetTicksMsec());
			if (num <= 0)
			{
				_sendMessageTask = TaskHelper.RunSafely(SendSyncMessageAfterSmallDelay());
			}
			else
			{
				_sendMessageTask = TaskHelper.RunSafely(QueueSyncMessage(num));
			}
		}
	}

	private async Task QueueSyncMessage(int delayMsec)
	{
		await Task.Delay(delayMsec);
		SendSyncMessage();
	}

	private async Task SendSyncMessageAfterSmallDelay()
	{
		await Task.Yield();
		SendSyncMessage();
	}

	private void SendSyncMessage()
	{
		if (_netService.IsConnected)
		{
			_queuedMessage.drawingMode = GetDrawingStateForPlayer(_netService.NetId).drawingMode;
			_netService.SendMessage(_queuedMessage);
			_lastMessageMsec = Time.GetTicksMsec();
			_queuedMessage = null;
			_sendMessageTask = null;
		}
	}

	private void UpdateLocalCursor()
	{
		DrawingState drawingStateForPlayer = GetDrawingStateForPlayer(_netService.NetId);
		if (drawingStateForPlayer.CurrentDrawingMode == DrawingMode.Drawing)
		{
			Image asset = PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_quill.png");
			Image asset2 = PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_quill_tilted.png");
			_cursorManager.OverrideCursor(asset2, asset, drawingCursorHotspot);
		}
		else if (drawingStateForPlayer.CurrentDrawingMode == DrawingMode.Erasing)
		{
			Image asset3 = PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_eraser.png");
			Image asset4 = PreloadManager.Cache.GetAsset<Image>("res://images/packed/common_ui/cursor_eraser_tilted.png");
			_cursorManager.OverrideCursor(asset4, asset3, erasingCursorHotspot);
		}
		else
		{
			_cursorManager.StopOverridingCursor();
		}
	}

	public void RepositionBasedOnBackground(Control mapBg)
	{
		base.Position = new Vector2(mapBg.Position.X + (mapBg.Size.X - base.Size.X) * 0.5f, mapBg.Position.Y);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateCurrentLinePositionLocal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopLineLocal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetDrawingModeLocal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "drawingMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearDrawnLinesLocal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsDrawing, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsLocalDrawing, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetDrawingMode, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetLocalDrawingMode, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "useOverride", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToNetPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FromNetPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "pos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearAllLines, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlayerScreenChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "oldScreenType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TrySendSyncMessage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SendSyncMessage, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateLocalCursor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RepositionBasedOnBackground, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mapBg", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.UpdateCurrentLinePositionLocal && args.Count == 1)
		{
			UpdateCurrentLinePositionLocal(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopLineLocal && args.Count == 0)
		{
			StopLineLocal();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDrawingModeLocal && args.Count == 1)
		{
			SetDrawingModeLocal(VariantUtils.ConvertTo<DrawingMode>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearDrawnLinesLocal && args.Count == 0)
		{
			ClearDrawnLinesLocal();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsDrawing && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsDrawing(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.IsLocalDrawing && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsLocalDrawing());
			return true;
		}
		if (method == MethodName.GetDrawingMode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<DrawingMode>(GetDrawingMode(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.GetLocalDrawingMode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<DrawingMode>(GetLocalDrawingMode(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.ToNetPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(ToNetPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.FromNetPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(FromNetPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.ClearAllLines && args.Count == 0)
		{
			ClearAllLines();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerScreenChanged && args.Count == 2)
		{
			OnPlayerScreenChanged(VariantUtils.ConvertTo<ulong>(in args[0]), VariantUtils.ConvertTo<NetScreenType>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TrySendSyncMessage && args.Count == 0)
		{
			TrySendSyncMessage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SendSyncMessage && args.Count == 0)
		{
			SendSyncMessage();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLocalCursor && args.Count == 0)
		{
			UpdateLocalCursor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RepositionBasedOnBackground && args.Count == 1)
		{
			RepositionBasedOnBackground(VariantUtils.ConvertTo<Control>(in args[0]));
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
		if (method == MethodName.UpdateCurrentLinePositionLocal)
		{
			return true;
		}
		if (method == MethodName.StopLineLocal)
		{
			return true;
		}
		if (method == MethodName.SetDrawingModeLocal)
		{
			return true;
		}
		if (method == MethodName.ClearDrawnLinesLocal)
		{
			return true;
		}
		if (method == MethodName.IsDrawing)
		{
			return true;
		}
		if (method == MethodName.IsLocalDrawing)
		{
			return true;
		}
		if (method == MethodName.GetDrawingMode)
		{
			return true;
		}
		if (method == MethodName.GetLocalDrawingMode)
		{
			return true;
		}
		if (method == MethodName.ToNetPosition)
		{
			return true;
		}
		if (method == MethodName.FromNetPosition)
		{
			return true;
		}
		if (method == MethodName.ClearAllLines)
		{
			return true;
		}
		if (method == MethodName.OnPlayerScreenChanged)
		{
			return true;
		}
		if (method == MethodName.TrySendSyncMessage)
		{
			return true;
		}
		if (method == MethodName.SendSyncMessage)
		{
			return true;
		}
		if (method == MethodName.UpdateLocalCursor)
		{
			return true;
		}
		if (method == MethodName.RepositionBasedOnBackground)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._lineDrawScene)
		{
			_lineDrawScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._lineEraseScene)
		{
			_lineEraseScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._cursorManager)
		{
			_cursorManager = VariantUtils.ConvertTo<NCursorManager>(in value);
			return true;
		}
		if (name == PropertyName._eraserMaterial)
		{
			_eraserMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._defaultSize)
		{
			_defaultSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._lastMessageMsec)
		{
			_lastMessageMsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._lineDrawScene)
		{
			value = VariantUtils.CreateFrom(in _lineDrawScene);
			return true;
		}
		if (name == PropertyName._lineEraseScene)
		{
			value = VariantUtils.CreateFrom(in _lineEraseScene);
			return true;
		}
		if (name == PropertyName._cursorManager)
		{
			value = VariantUtils.CreateFrom(in _cursorManager);
			return true;
		}
		if (name == PropertyName._eraserMaterial)
		{
			value = VariantUtils.CreateFrom(in _eraserMaterial);
			return true;
		}
		if (name == PropertyName._defaultSize)
		{
			value = VariantUtils.CreateFrom(in _defaultSize);
			return true;
		}
		if (name == PropertyName._lastMessageMsec)
		{
			value = VariantUtils.CreateFrom(in _lastMessageMsec);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lineDrawScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lineEraseScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cursorManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eraserMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._defaultSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._lastMessageMsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._lineDrawScene, Variant.From(in _lineDrawScene));
		info.AddProperty(PropertyName._lineEraseScene, Variant.From(in _lineEraseScene));
		info.AddProperty(PropertyName._cursorManager, Variant.From(in _cursorManager));
		info.AddProperty(PropertyName._eraserMaterial, Variant.From(in _eraserMaterial));
		info.AddProperty(PropertyName._defaultSize, Variant.From(in _defaultSize));
		info.AddProperty(PropertyName._lastMessageMsec, Variant.From(in _lastMessageMsec));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._lineDrawScene, out var value))
		{
			_lineDrawScene = value.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._lineEraseScene, out var value2))
		{
			_lineEraseScene = value2.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._cursorManager, out var value3))
		{
			_cursorManager = value3.As<NCursorManager>();
		}
		if (info.TryGetProperty(PropertyName._eraserMaterial, out var value4))
		{
			_eraserMaterial = value4.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._defaultSize, out var value5))
		{
			_defaultSize = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._lastMessageMsec, out var value6))
		{
			_lastMessageMsec = value6.As<ulong>();
		}
	}
}
