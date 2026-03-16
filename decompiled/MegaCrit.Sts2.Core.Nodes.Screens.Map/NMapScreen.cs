using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapScreen.cs")]
public class NMapScreen : Control, IScreenContext, INetCursorPositionTranslator
{
	[Signal]
	public delegate void OpenedEventHandler();

	[Signal]
	public delegate void ClosedEventHandler();

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName GetLineEndpoint = "GetLineEndpoint";

		public static readonly StringName RecalculateTravelability = "RecalculateTravelability";

		public static readonly StringName InitMapVotes = "InitMapVotes";

		public static readonly StringName OnMapPointSelectedLocally = "OnMapPointSelectedLocally";

		public static readonly StringName RefreshAllMapPointVotes = "RefreshAllMapPointVotes";

		public static readonly StringName RemoveAllMapPointsAndPaths = "RemoveAllMapPointsAndPaths";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName UpdateScrollPosition = "UpdateScrollPosition";

		public new static readonly StringName _GuiInput = "_GuiInput";

		public static readonly StringName ProcessMouseEvent = "ProcessMouseEvent";

		public static readonly StringName ProcessMouseDrawingEvent = "ProcessMouseDrawingEvent";

		public static readonly StringName ProcessScrollEvent = "ProcessScrollEvent";

		public static readonly StringName ProcessControllerEvent = "ProcessControllerEvent";

		public static readonly StringName SetTravelEnabled = "SetTravelEnabled";

		public static readonly StringName SetDebugTravelEnabled = "SetDebugTravelEnabled";

		public static readonly StringName RefreshAllPointVisuals = "RefreshAllPointVisuals";

		public static readonly StringName PlayStartOfActAnimation = "PlayStartOfActAnimation";

		public static readonly StringName InitMapPrompt = "InitMapPrompt";

		public static readonly StringName SetInterruptable = "SetInterruptable";

		public static readonly StringName CanScroll = "CanScroll";

		public static readonly StringName TryCancelStartOfActAnim = "TryCancelStartOfActAnim";

		public static readonly StringName OnVisibilityChanged = "OnVisibilityChanged";

		public static readonly StringName OnCapstoneChanged = "OnCapstoneChanged";

		public static readonly StringName Close = "Close";

		public static readonly StringName Open = "Open";

		public static readonly StringName OnBackButtonPressed = "OnBackButtonPressed";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName OnMapDrawingButtonPressed = "OnMapDrawingButtonPressed";

		public static readonly StringName OnMapErasingButtonPressed = "OnMapErasingButtonPressed";

		public static readonly StringName UpdateDrawingButtonStates = "UpdateDrawingButtonStates";

		public static readonly StringName OnClearMapDrawingButtonPressed = "OnClearMapDrawingButtonPressed";

		public static readonly StringName HighlightPointType = "HighlightPointType";

		public static readonly StringName OnLegendHotkeyPressed = "OnLegendHotkeyPressed";

		public static readonly StringName OnDrawingToolsHotkeyPressed = "OnDrawingToolsHotkeyPressed";

		public static readonly StringName GetNetPositionFromScreenPosition = "GetNetPositionFromScreenPosition";

		public static readonly StringName GetMapPositionFromNetPosition = "GetMapPositionFromNetPosition";

		public static readonly StringName GetScreenPositionFromNetPosition = "GetScreenPositionFromNetPosition";

		public static readonly StringName IsNodeOnScreen = "IsNodeOnScreen";

		public static readonly StringName CleanUp = "CleanUp";

		public static readonly StringName UpdateHotkeyDisplay = "UpdateHotkeyDisplay";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsOpen = "IsOpen";

		public static readonly StringName IsTravelEnabled = "IsTravelEnabled";

		public static readonly StringName IsDebugTravelEnabled = "IsDebugTravelEnabled";

		public static readonly StringName MapLegendX = "MapLegendX";

		public static readonly StringName IsTraveling = "IsTraveling";

		public static readonly StringName Drawings = "Drawings";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _mapContainer = "_mapContainer";

		public static readonly StringName _pathsContainer = "_pathsContainer";

		public static readonly StringName _points = "_points";

		public static readonly StringName _bossPointNode = "_bossPointNode";

		public static readonly StringName _secondBossPointNode = "_secondBossPointNode";

		public static readonly StringName _startingPointNode = "_startingPointNode";

		public static readonly StringName _mapBgContainer = "_mapBgContainer";

		public static readonly StringName _marker = "_marker";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _drawingToolsHotkeyIcon = "_drawingToolsHotkeyIcon";

		public static readonly StringName _drawingTools = "_drawingTools";

		public static readonly StringName _mapDrawingButton = "_mapDrawingButton";

		public static readonly StringName _mapErasingButton = "_mapErasingButton";

		public static readonly StringName _mapClearButton = "_mapClearButton";

		public static readonly StringName _mapLegend = "_mapLegend";

		public static readonly StringName _legendItems = "_legendItems";

		public static readonly StringName _legendHotkeyIcon = "_legendHotkeyIcon";

		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _startDragPos = "_startDragPos";

		public static readonly StringName _targetDragPos = "_targetDragPos";

		public static readonly StringName _isDragging = "_isDragging";

		public static readonly StringName _hasPlayedAnimation = "_hasPlayedAnimation";

		public static readonly StringName _controllerScrollAmount = "_controllerScrollAmount";

		public static readonly StringName _distX = "_distX";

		public static readonly StringName _distY = "_distY";

		public static readonly StringName _actAnimTween = "_actAnimTween";

		public static readonly StringName _mapScrollAnimTimer = "_mapScrollAnimTimer";

		public static readonly StringName _mapAnimStartDelay = "_mapAnimStartDelay";

		public static readonly StringName _mapAnimDuration = "_mapAnimDuration";

		public static readonly StringName _canInterruptAnim = "_canInterruptAnim";

		public static readonly StringName _isInputDisabled = "_isInputDisabled";

		public static readonly StringName _promptTween = "_promptTween";

		public static readonly StringName _drawingInput = "_drawingInput";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName Opened = "Opened";

		public static readonly StringName Closed = "Closed";
	}

	private ActMap _map = NullActMap.Instance;

	private Control _mapContainer;

	private Control _pathsContainer;

	private Control _points;

	private NBossMapPoint? _bossPointNode;

	private NBossMapPoint? _secondBossPointNode;

	private NMapPoint? _startingPointNode;

	private NMapBg _mapBgContainer;

	private NMapMarker _marker;

	private NBackButton _backButton;

	private TextureRect _drawingToolsHotkeyIcon;

	private Control _drawingTools;

	private NMapDrawButton _mapDrawingButton;

	private NMapEraseButton _mapErasingButton;

	private NMapClearButton _mapClearButton;

	private Control _mapLegend;

	private Control _legendItems;

	private TextureRect _legendHotkeyIcon;

	private Control _backstop;

	private Tween? _tween;

	private Vector2 _startDragPos;

	private Vector2 _targetDragPos;

	private bool _isDragging;

	private bool _hasPlayedAnimation;

	private readonly Dictionary<MapCoord, NMapPoint> _mapPointDictionary = new Dictionary<MapCoord, NMapPoint>();

	private readonly Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>> _paths = new Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>>();

	private float _controllerScrollAmount = 400f;

	private const float _scrollLimitTop = 1800f;

	private const float _scrollLimitBottom = -600f;

	private const float _totalHeight = 2325f;

	private const float _totalWidth = 1050f;

	private float _distX;

	private float _distY;

	private const float _pointJitterX = 21f;

	private const float _pointJitterY = 25f;

	private const float _tickDist = 22f;

	private const float _pathPosJitter = 3f;

	private const float _pathAngleJitter = 0.1f;

	private static readonly Vector2 _tickTraveledScale = Vector2.One * 1.2f;

	private Tween? _actAnimTween;

	private float _mapScrollAnimTimer;

	private const string _mapTickScenePath = "res://scenes/ui/map_dot.tscn";

	private readonly double _mapAnimStartDelay = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.5 : 1.0);

	private readonly double _mapAnimDuration = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 1.5 : 3.0);

	private bool _canInterruptAnim;

	private bool _isInputDisabled;

	private RunState _runState;

	private Tween? _promptTween;

	private NMapDrawingInput? _drawingInput;

	private OpenedEventHandler backing_Opened;

	private ClosedEventHandler backing_Closed;

	public static NMapScreen? Instance => NRun.Instance?.GlobalUi.MapScreen;

	public bool IsOpen { get; private set; }

	public bool IsTravelEnabled { get; private set; }

	public bool IsDebugTravelEnabled { get; private set; }

	private float MapLegendX => base.Size.X * 0.8f;

	public bool IsTraveling { get; set; }

	public Dictionary<Player, MapCoord?> PlayerVoteDictionary { get; } = new Dictionary<Player, MapCoord?>();

	public NMapDrawings Drawings { get; private set; }

	public static IEnumerable<string> AssetPaths => NMapDrawings.AssetPaths.Append("res://scenes/ui/map_dot.tscn");

	public Control DefaultFocusedControl
	{
		get
		{
			NMapPoint nMapPoint = _mapPointDictionary.Values.FirstOrDefault((NMapPoint n) => n.IsEnabled);
			if (nMapPoint != null)
			{
				return nMapPoint;
			}
			return this;
		}
	}

	public event Action<MapPointType>? PointTypeHighlighted;

	public event OpenedEventHandler Opened
	{
		add
		{
			backing_Opened = (OpenedEventHandler)Delegate.Combine(backing_Opened, value);
		}
		remove
		{
			backing_Opened = (OpenedEventHandler)Delegate.Remove(backing_Opened, value);
		}
	}

	public event ClosedEventHandler Closed
	{
		add
		{
			backing_Closed = (ClosedEventHandler)Delegate.Combine(backing_Closed, value);
		}
		remove
		{
			backing_Closed = (ClosedEventHandler)Delegate.Remove(backing_Closed, value);
		}
	}

	public override void _Ready()
	{
		GetNode<MegaLabel>("MapLegend/Header").SetTextAutoSize(new LocString("map", "LEGEND_HEADER").GetFormattedText());
		_mapContainer = GetNode<Control>("TheMap");
		_mapBgContainer = GetNode<NMapBg>("%MapBg");
		_pathsContainer = GetNode<Control>("TheMap/Paths");
		_points = GetNode<Control>("TheMap/Points");
		_marker = GetNode<NMapMarker>("TheMap/MapMarker");
		Drawings = GetNode<NMapDrawings>("TheMap/Drawings");
		_backButton = GetNode<NBackButton>("Back");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackButtonPressed));
		_backButton.Disable();
		_mapLegend = GetNode<Control>("MapLegend");
		_legendItems = GetNode<Control>("MapLegend/LegendItems");
		_legendHotkeyIcon = GetNode<TextureRect>("MapLegend/LegendHotkeyIcon");
		_drawingToolsHotkeyIcon = GetNode<TextureRect>("DrawingToolsHotkey");
		_backstop = GetNode<Control>("%Backstop");
		_drawingTools = GetNode<Control>("%DrawingTools");
		_mapDrawingButton = GetNode<NMapDrawButton>("%DrawButton");
		_mapDrawingButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnMapDrawingButtonPressed));
		_mapErasingButton = GetNode<NMapEraseButton>("%EraseButton");
		_mapErasingButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnMapErasingButtonPressed));
		_mapClearButton = GetNode<NMapClearButton>("%ClearButton");
		_mapClearButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnClearMapDrawingButtonPressed));
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteChanged += OnPlayerVoteChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteCancelled += OnPlayerVoteCancelled;
		base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(OnVisibilityChanged));
		Callable.From(() => NCapstoneContainer.Instance.Connect(NCapstoneContainer.SignalName.Changed, Callable.From(OnCapstoneChanged))).CallDeferred();
		List<NMapLegendItem> list = _legendItems.GetChildren().OfType<NMapLegendItem>().ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num].FocusNeighborTop = ((num > 0) ? list[num - 1].GetPath() : list[num].GetPath());
			list[num].FocusNeighborBottom = ((num < list.Count - 1) ? list[num + 1].GetPath() : list[num].GetPath());
			list[num].FocusNeighborRight = list[num].GetPath();
		}
		NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(UpdateHotkeyDisplay));
		NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(UpdateHotkeyDisplay));
		NInputManager.Instance.Connect(NInputManager.SignalName.InputRebound, Callable.From(UpdateHotkeyDisplay));
		UpdateHotkeyDisplay();
	}

	public override void _ExitTree()
	{
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteChanged -= OnPlayerVoteChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteCancelled -= OnPlayerVoteCancelled;
	}

	public void Initialize(RunState runState)
	{
		_runState = runState;
		Drawings.Initialize(RunManager.Instance.NetService, _runState, RunManager.Instance.InputSynchronizer);
		_marker.Initialize(LocalContext.GetMe(_runState));
		_mapBgContainer.Initialize(_runState);
	}

	public void SetMap(ActMap map, uint seed, bool clearDrawings)
	{
		_map = map;
		_mapPointDictionary.Clear();
		_paths.Clear();
		RemoveAllMapPointsAndPaths();
		_marker.ResetMapPoint();
		if (clearDrawings)
		{
			Drawings.ClearAllLines();
		}
		_hasPlayedAnimation = false;
		int rowCount = map.GetRowCount();
		int columnCount = map.GetColumnCount();
		float num = ((map.SecondBossMapPoint != null) ? 0.9f : 1f);
		_distY = 2325f / (float)(rowCount - 1) * num;
		_distX = 1050f / (float)columnCount;
		Rng rng = new Rng(seed, $"map_jitter_{_runState.CurrentActIndex}");
		Vector2 vector = new Vector2(-500f, 740f);
		Vector2 vector2 = new Vector2(_distX, 0f - _distY);
		foreach (MapPoint allMapPoint in map.GetAllMapPoints())
		{
			NNormalMapPoint nNormalMapPoint = NNormalMapPoint.Create(allMapPoint, this, _runState);
			nNormalMapPoint.Position = new Vector2(allMapPoint.coord.col, allMapPoint.coord.row) * vector2 + vector;
			float x = rng.NextFloat(-21f, 21f);
			float y = rng.NextFloat(-25f, 25f);
			nNormalMapPoint.Position += new Vector2(x, y);
			_mapPointDictionary.Add(allMapPoint.coord, nNormalMapPoint);
			_points.AddChildSafely(nNormalMapPoint);
			nNormalMapPoint.SetAngle(Rng.Chaotic.NextGaussianFloat(0f, 8f));
		}
		_bossPointNode = NBossMapPoint.Create(map.BossMapPoint, this, _runState);
		_bossPointNode.Position = new Vector2(-200f, -1980f * num);
		_points.AddChildSafely(_bossPointNode);
		_mapPointDictionary[map.BossMapPoint.coord] = _bossPointNode;
		if (map.SecondBossMapPoint != null)
		{
			_bossPointNode.Scale = new Vector2(0.75f, 0.75f);
			_secondBossPointNode = NBossMapPoint.Create(map.SecondBossMapPoint, this, _runState);
			_secondBossPointNode.Position = new Vector2(-200f, -2280f * num);
			_secondBossPointNode.Scale = new Vector2(0.75f, 0.75f);
			_points.AddChildSafely(_secondBossPointNode);
			_mapPointDictionary[map.SecondBossMapPoint.coord] = _secondBossPointNode;
		}
		if (map.StartingMapPoint.PointType == MapPointType.Ancient)
		{
			_startingPointNode = NAncientMapPoint.Create(map.StartingMapPoint, this, _runState);
			_startingPointNode.Position = new Vector2(-80f, (float)map.StartingMapPoint.coord.row * (0f - _distY) + 720f);
		}
		else
		{
			_startingPointNode = NNormalMapPoint.Create(map.StartingMapPoint, this, _runState);
			_startingPointNode.Position = new Vector2(-80f, (float)map.StartingMapPoint.coord.row * (0f - _distY) + 800f);
		}
		_points.AddChildSafely(_startingPointNode);
		_mapPointDictionary[map.StartingMapPoint.coord] = _startingPointNode;
		foreach (MapPoint allMapPoint2 in map.GetAllMapPoints())
		{
			DrawPaths(_mapPointDictionary[allMapPoint2.coord], allMapPoint2);
		}
		DrawPaths(_startingPointNode, map.StartingMapPoint);
		DrawPaths(_bossPointNode, map.BossMapPoint);
		IReadOnlyList<MapCoord> visitedMapCoords = _runState.VisitedMapCoords;
		for (int i = 0; i < visitedMapCoords.Count - 1; i++)
		{
			if (!_paths.TryGetValue((visitedMapCoords[i], visitedMapCoords[i + 1]), out IReadOnlyList<TextureRect> value))
			{
				continue;
			}
			foreach (TextureRect item in value)
			{
				item.Modulate = _runState.Act.MapTraveledColor;
				item.Scale = _tickTraveledScale;
			}
		}
		InitMapVotes();
		RefreshAllMapPointVotes();
		for (int j = 0; j < map.GetRowCount(); j++)
		{
			IEnumerable<MapPoint> pointsInRow = map.GetPointsInRow(j);
			List<NMapPoint> list = pointsInRow.Select((MapPoint p) => _mapPointDictionary[p.coord]).ToList();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				list[num2].FocusNeighborLeft = ((num2 > 0) ? list[num2 - 1].GetPath() : list[num2].GetPath());
				list[num2].FocusNeighborRight = ((num2 < list.Count - 1) ? list[num2 + 1].GetPath() : list[num2].GetPath());
				list[num2].FocusNeighborTop = list[num2].GetPath();
				list[num2].FocusNeighborBottom = list[num2].GetPath();
			}
		}
		_startingPointNode.FocusNeighborLeft = _startingPointNode.GetPath();
		_startingPointNode.FocusNeighborRight = _startingPointNode.GetPath();
		_startingPointNode.FocusNeighborTop = _startingPointNode.GetPath();
		_startingPointNode.FocusNeighborBottom = _startingPointNode.GetPath();
		_bossPointNode.FocusNeighborLeft = _bossPointNode.GetPath();
		_bossPointNode.FocusNeighborRight = _bossPointNode.GetPath();
		_bossPointNode.FocusNeighborBottom = _bossPointNode.GetPath();
		if (_secondBossPointNode != null)
		{
			_bossPointNode.FocusNeighborTop = _secondBossPointNode.GetPath();
			_secondBossPointNode.FocusNeighborBottom = _bossPointNode.GetPath();
			_secondBossPointNode.FocusNeighborLeft = _secondBossPointNode.GetPath();
			_secondBossPointNode.FocusNeighborRight = _secondBossPointNode.GetPath();
			_secondBossPointNode.FocusNeighborTop = _secondBossPointNode.GetPath();
		}
		else
		{
			_bossPointNode.FocusNeighborTop = _bossPointNode.GetPath();
		}
		if (IsVisible())
		{
			RecalculateTravelability();
			RefreshAllPointVisuals();
		}
	}

	private void DrawPaths(NMapPoint mapPointNode, MapPoint mapPoint)
	{
		foreach (MapPoint child in mapPoint.Children)
		{
			if (!_mapPointDictionary.TryGetValue(child.coord, out NMapPoint value))
			{
				throw new InvalidOperationException($"Map point child with coord {child.coord} is not in the map point dictionary!");
			}
			Vector2 lineEndpoint = GetLineEndpoint(mapPointNode);
			Vector2 lineEndpoint2 = GetLineEndpoint(value);
			IReadOnlyList<TextureRect> value2 = CreatePath(lineEndpoint, lineEndpoint2);
			_paths.Add((mapPoint.coord, child.coord), value2);
		}
	}

	private Vector2 GetLineEndpoint(NMapPoint point)
	{
		if (point is NNormalMapPoint)
		{
			return point.Position;
		}
		return point.Position + point.Size * 0.5f;
	}

	private void RecalculateTravelability()
	{
		if (_runState.VisitedMapCoords.Any())
		{
			foreach (NMapPoint value in _mapPointDictionary.Values)
			{
				value.State = MapPointState.Untravelable;
			}
			foreach (MapCoord visitedMapCoord in _runState.VisitedMapCoords)
			{
				_mapPointDictionary[visitedMapCoord].State = MapPointState.Traveled;
			}
			IReadOnlyList<MapCoord> visitedMapCoords = _runState.VisitedMapCoords;
			MapCoord mapCoord = visitedMapCoords[visitedMapCoords.Count - 1];
			if (_secondBossPointNode != null && mapCoord == _bossPointNode.Point.coord)
			{
				_secondBossPointNode.State = MapPointState.Travelable;
				return;
			}
			if (mapCoord.row != _map.GetRowCount() - 1)
			{
				IEnumerable<MapPoint> enumerable = (_runState.Modifiers.OfType<Flight>().Any() ? _map.GetPointsInRow(mapCoord.row + 1) : _mapPointDictionary[mapCoord].Point.Children);
				{
					foreach (MapPoint item in enumerable)
					{
						_mapPointDictionary[item.coord].State = MapPointState.Travelable;
					}
					return;
				}
			}
			_bossPointNode.State = MapPointState.Travelable;
		}
		else
		{
			_startingPointNode.State = MapPointState.Travelable;
		}
	}

	private void InitMapVotes()
	{
		foreach (Player player in _runState.Players)
		{
			MapCoord? mapCoord = RunManager.Instance.MapSelectionSynchronizer.GetVote(player)?.coord;
			if (mapCoord.HasValue)
			{
				OnPlayerVoteChangedInternal(player, null, mapCoord.Value);
			}
		}
	}

	public void OnMapPointSelectedLocally(NMapPoint point)
	{
		Player me = LocalContext.GetMe(_runState);
		if (!PlayerVoteDictionary.TryGetValue(me, out var value) || value != point.Point.coord)
		{
			OnPlayerVoteChangedInternal(me, RunManager.Instance.MapSelectionSynchronizer.GetVote(me)?.coord, point.Point.coord);
			RunLocation source = new RunLocation(_runState.CurrentMapCoord, _runState.CurrentActIndex);
			MapVote value2 = new MapVote
			{
				coord = point.Point.coord,
				mapGenerationCount = RunManager.Instance.MapSelectionSynchronizer.MapGenerationCount
			};
			VoteForMapCoordAction action = new VoteForMapCoordAction(LocalContext.GetMe(_runState), source, value2);
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
		}
		else if (_runState.Players.Count > 1)
		{
			RunManager.Instance.FlavorSynchronizer.SendMapPing(point.Point.coord);
		}
	}

	private void OnPlayerVoteChanged(Player player, MapVote? oldLocation, MapVote? newLocation)
	{
		Log.Info($"Player vote changed for {player.NetId}: {oldLocation}->{newLocation}");
		if (!LocalContext.IsMe(player))
		{
			OnPlayerVoteChangedInternal(player, oldLocation?.coord, newLocation?.coord);
		}
	}

	private void OnPlayerVoteCancelled(Player player)
	{
		Log.Info($"Player vote cancelled for {player.NetId}");
		OnPlayerVoteChangedInternal(player, PlayerVoteDictionary[player], null);
	}

	private void OnPlayerVoteChangedInternal(Player player, MapCoord? oldCoord, MapCoord? newCoord)
	{
		if (_runState.Players.Count > 1)
		{
			PlayerVoteDictionary[player] = newCoord;
			if (oldCoord.HasValue)
			{
				NMapPoint nMapPoint = _mapPointDictionary[oldCoord.Value];
				nMapPoint.VoteContainer.RefreshPlayerVotes();
			}
			else if (_runState.CurrentLocation.coord.HasValue)
			{
				NMapPoint nMapPoint2 = _mapPointDictionary[_runState.CurrentLocation.coord.Value];
				nMapPoint2.VoteContainer.RefreshPlayerVotes();
			}
			if (newCoord.HasValue)
			{
				NMapPoint nMapPoint3 = _mapPointDictionary[newCoord.Value];
				nMapPoint3.VoteContainer.RefreshPlayerVotes();
			}
			else if (_runState.CurrentLocation.coord.HasValue)
			{
				NMapPoint nMapPoint4 = _mapPointDictionary[_runState.CurrentLocation.coord.Value];
				nMapPoint4.VoteContainer.RefreshPlayerVotes();
			}
		}
	}

	public void InitMarker(MapCoord coord)
	{
		NMapPoint mapPoint = _mapPointDictionary[coord];
		_marker.SetMapPoint(mapPoint);
	}

	public async Task TravelToMapCoord(MapCoord coord)
	{
		IsTraveling = true;
		RecalculateTravelability();
		if (NCapstoneContainer.Instance.CurrentCapstoneScreen is NDeckViewScreen)
		{
			NCapstoneContainer.Instance.Close();
		}
		_marker.HideMapPoint();
		IsTravelEnabled = false;
		MapSplitVoteAnimation mapSplitVoteAnimation = new MapSplitVoteAnimation(this, _runState, _mapPointDictionary);
		await mapSplitVoteAnimation.TryPlay(coord);
		NMapPoint node = _mapPointDictionary[coord];
		node.OnSelected();
		float scaleMultiplier = 1f;
		if (node is NAncientMapPoint)
		{
			scaleMultiplier = 1.5f;
		}
		else if (node is NBossMapPoint)
		{
			scaleMultiplier = 2f;
		}
		NMapNodeSelectVfx nMapNodeSelectVfx = NMapNodeSelectVfx.Create(scaleMultiplier);
		SfxCmd.Play("event:/sfx/ui/map/map_select");
		node.AddChildSafely(nMapNodeSelectVfx);
		nMapNodeSelectVfx.Position += node.PivotOffset;
		IReadOnlyList<MapCoord> visitedMapCoords = _runState.VisitedMapCoords;
		SfxCmd.Play("event:/sfx/ui/wipe_map");
		Task fadeOutTask = TaskHelper.RunSafely(NGame.Instance.Transition.RoomFadeOut());
		if (visitedMapCoords.Any())
		{
			if (_paths.TryGetValue((visitedMapCoords[visitedMapCoords.Count - 1], node.Point.coord), out IReadOnlyList<TextureRect> value))
			{
				float waitPerTick = SaveManager.Instance.PrefsSave.FastMode switch
				{
					FastModeType.Fast => 0.3f, 
					FastModeType.Normal => 0.8f, 
					_ => 0f, 
				} / (float)value.Count;
				foreach (TextureRect tick in value)
				{
					await Cmd.Wait(waitPerTick);
					tick.Modulate = StsColors.pathDotTraveled;
					Tween tween = CreateTween();
					tween.TweenProperty(tick, "scale", _tickTraveledScale, 0.4).From(Vector2.One * 1.7f).SetEase(Tween.EaseType.Out)
						.SetTrans(Tween.TransitionType.Cubic);
				}
			}
		}
		_marker.SetMapPoint(node);
		await fadeOutTask;
		await RunManager.Instance.EnterMapCoord(coord);
		RefreshAllPointVisuals();
		PlayerVoteDictionary.Clear();
		RefreshAllMapPointVotes();
	}

	public void RefreshAllMapPointVotes()
	{
		foreach (NMapPoint value in _mapPointDictionary.Values)
		{
			value.VoteContainer.RefreshPlayerVotes();
		}
	}

	private void RemoveAllMapPointsAndPaths()
	{
		_points.FreeChildren();
		_pathsContainer.FreeChildren();
		_bossPointNode?.QueueFreeSafely();
		_secondBossPointNode?.QueueFreeSafely();
		_startingPointNode?.QueueFreeSafely();
	}

	private IReadOnlyList<TextureRect> CreatePath(Vector2 start, Vector2 end)
	{
		List<TextureRect> list = new List<TextureRect>();
		Vector2 vector = (end - start).Normalized();
		float num = vector.Angle() + (float)Math.PI / 2f;
		float num2 = start.DistanceTo(end);
		int num3 = (int)(num2 / 22f) + 1;
		for (int i = 1; i < num3; i++)
		{
			float num4 = (float)i * 22f;
			TextureRect textureRect = PreloadManager.Cache.GetScene("res://scenes/ui/map_dot.tscn").Instantiate<TextureRect>(PackedScene.GenEditState.Disabled);
			textureRect.Position = start + vector * num4;
			textureRect.Position -= new Vector2(base.Size.X * 0.5f - 20f, base.Size.Y * 0.5f - 20f);
			textureRect.Position += new Vector2(Rng.Chaotic.NextFloat(-3f, 3f), Rng.Chaotic.NextFloat(-3f, 3f));
			textureRect.FlipH = Rng.Chaotic.NextBool();
			textureRect.Rotation = num + Rng.Chaotic.NextGaussianFloat(0f, 0.1f);
			textureRect.Modulate = _runState.Act.MapUntraveledColor;
			_pathsContainer.AddChildSafely(textureRect);
			list.Add(textureRect);
		}
		return list;
	}

	public override void _Process(double delta)
	{
		if (IsVisibleInTree() && (_actAnimTween == null || !_actAnimTween.IsRunning()))
		{
			UpdateScrollPosition(delta);
		}
	}

	private void UpdateScrollPosition(double delta)
	{
		if (_mapContainer.Position != _targetDragPos)
		{
			_mapContainer.Position = _mapContainer.Position.Lerp(_targetDragPos, (float)delta * 15f);
			if (_mapContainer.Position.DistanceTo(_targetDragPos) < 0.5f)
			{
				_mapContainer.Position = _targetDragPos;
			}
		}
		if (!_isDragging)
		{
			if (_targetDragPos.Y < -600f)
			{
				_targetDragPos = _targetDragPos.Lerp(new Vector2(0f, -600f), (float)delta * 12f);
			}
			else if (_targetDragPos.Y > 1800f)
			{
				_targetDragPos = _targetDragPos.Lerp(new Vector2(0f, 1800f), (float)delta * 12f);
			}
		}
		NGame.Instance.RemoteCursorContainer.ForceUpdateAllCursors();
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (IsVisibleInTree())
		{
			ProcessMouseEvent(inputEvent);
			ProcessScrollEvent(inputEvent);
		}
	}

	private void ProcessMouseEvent(InputEvent inputEvent)
	{
		ProcessMouseDrawingEvent(inputEvent);
		if (_drawingInput != null)
		{
			return;
		}
		if (_isDragging && inputEvent is InputEventMouseMotion inputEventMouseMotion)
		{
			_targetDragPos += new Vector2(0f, inputEventMouseMotion.Relative.Y);
		}
		else if (inputEvent is InputEventMouseButton inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				if (inputEventMouseButton.Pressed && CanScroll())
				{
					_isDragging = true;
					_startDragPos = _mapContainer.Position;
					_targetDragPos = _startDragPos;
					TryCancelStartOfActAnim();
				}
				else
				{
					_isDragging = false;
				}
			}
			else if (!inputEventMouseButton.Pressed)
			{
				_isDragging = false;
			}
		}
		if (inputEvent is InputEventMouseMotion inputEventMouseMotion2 && Drawings.IsLocalDrawing())
		{
			Drawings.UpdateCurrentLinePositionLocal(Drawings.GetGlobalTransform().Inverse() * inputEventMouseMotion2.GlobalPosition);
		}
	}

	private void ProcessMouseDrawingEvent(InputEvent inputEvent)
	{
		if (!_isInputDisabled && (_actAnimTween == null || !_actAnimTween.IsRunning()) && _drawingInput == null && inputEvent is InputEventMouseButton { Pressed: not false } inputEventMouseButton)
		{
			if (inputEventMouseButton.ButtonIndex == MouseButton.Right)
			{
				_drawingInput = NMapDrawingInput.Create(Drawings, DrawingMode.Drawing, stopOnMouseRelease: true);
			}
			else if (inputEventMouseButton.ButtonIndex == MouseButton.Middle)
			{
				_drawingInput = NMapDrawingInput.Create(Drawings, DrawingMode.Erasing, stopOnMouseRelease: true);
			}
			_drawingInput?.Connect(NMapDrawingInput.SignalName.Finished, Callable.From(delegate
			{
				_drawingInput = null;
				UpdateDrawingButtonStates();
			}));
			this.AddChildSafely(_drawingInput);
		}
	}

	private void ProcessScrollEvent(InputEvent inputEvent)
	{
		if (CanScroll())
		{
			_targetDragPos += new Vector2(0f, ScrollHelper.GetDragForScrollEvent(inputEvent));
			if ((inputEvent is InputEventMouseButton || inputEvent is InputEventPanGesture) ? true : false)
			{
				TryCancelStartOfActAnim();
			}
		}
	}

	private void ProcessControllerEvent(InputEvent inputEvent)
	{
		if (inputEvent.IsActionPressed(MegaInput.up) && CanScroll())
		{
			_targetDragPos += new Vector2(0f, _controllerScrollAmount);
			TryCancelStartOfActAnim();
		}
		else if (inputEvent.IsActionPressed(MegaInput.down) && CanScroll())
		{
			_targetDragPos += new Vector2(0f, 0f - _controllerScrollAmount);
			TryCancelStartOfActAnim();
		}
		else if (inputEvent.IsActionPressed(MegaInput.right) || inputEvent.IsActionPressed(MegaInput.left) || inputEvent.IsActionPressed(MegaInput.select))
		{
			if (_runState.ActFloor == 0)
			{
				_targetDragPos = new Vector2(0f, -600f);
				return;
			}
			int num = _runState.CurrentMapCoord?.row ?? 0;
			_targetDragPos = new Vector2(0f, -600f + (float)num * _distY);
		}
	}

	public void SetTravelEnabled(bool enabled)
	{
		IsTravelEnabled = enabled && Hook.ShouldProceedToNextMapPoint(_runState);
		RefreshAllPointVisuals();
	}

	public void SetDebugTravelEnabled(bool enabled)
	{
		IsDebugTravelEnabled = enabled;
		RefreshAllPointVisuals();
	}

	public void RefreshAllPointVisuals()
	{
		foreach (NMapPoint value in _mapPointDictionary.Values)
		{
			value.RefreshVisualsInstantly();
		}
		_mapPointDictionary.Values.FirstOrDefault((NMapPoint n) => n.IsEnabled)?.TryGrabFocus();
	}

	private void PlayStartOfActAnimation()
	{
		if (_hasPlayedAnimation)
		{
			Log.Warn("Tried to play start of act animation twice! Ignoring second try");
			return;
		}
		_hasPlayedAnimation = true;
		NActBanner child = NActBanner.Create(_runState.Act, _runState.CurrentActIndex);
		NRun.Instance?.GlobalUi.MapScreen.AddChildSafely(child);
		TaskHelper.RunSafely(StartOfActAnim());
	}

	private async Task StartOfActAnim()
	{
		_mapContainer.Position = new Vector2(0f, 1800f);
		_actAnimTween?.Kill();
		_actAnimTween = CreateTween().SetParallel();
		_actAnimTween.TweenInterval(_mapAnimStartDelay);
		_actAnimTween.Chain();
		Vector2 targetDragPos = new Vector2(0f, -600f);
		_actAnimTween.TweenProperty(_mapContainer, "position:y", -600f, _mapAnimDuration).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Expo);
		_actAnimTween.TweenCallback(Callable.From(SetInterruptable)).SetDelay(_mapAnimDuration * 0.25);
		_targetDragPos = targetDragPos;
		await ToSignal(_actAnimTween, Tween.SignalName.Finished);
		_actAnimTween = null;
		InitMapPrompt();
	}

	private void InitMapPrompt()
	{
		if (!TestMode.IsOn && !SaveManager.Instance.SeenFtue("map_select_ftue"))
		{
			TaskHelper.RunSafely(MapFtueCheck());
		}
	}

	private async Task MapFtueCheck()
	{
		await Task.Delay(100);
		NMapSelectFtue nMapSelectFtue = NMapSelectFtue.Create(_startingPointNode);
		NModalContainer.Instance.Add(nMapSelectFtue);
		SaveManager.Instance.MarkFtueAsComplete("map_select_ftue");
		await nMapSelectFtue.WaitForPlayerToConfirm();
	}

	private void SetInterruptable()
	{
		_canInterruptAnim = true;
	}

	private bool CanScroll()
	{
		if (_actAnimTween == null || _canInterruptAnim)
		{
			return !_isInputDisabled;
		}
		return false;
	}

	private void TryCancelStartOfActAnim()
	{
		if (_actAnimTween != null && _canInterruptAnim)
		{
			_actAnimTween?.Kill();
			_actAnimTween = null;
			_canInterruptAnim = false;
			_isDragging = false;
			_targetDragPos = new Vector2(0f, -600f);
			TaskHelper.RunSafely(DisableInputVeryBriefly());
		}
	}

	private async Task DisableInputVeryBriefly()
	{
		_isInputDisabled = true;
		_drawingInput?.StopDrawing();
		await Task.Delay(200);
		_isInputDisabled = false;
		InitMapPrompt();
	}

	private void OnVisibilityChanged()
	{
		if (base.Visible)
		{
			RunManager.Instance.InputSynchronizer.StartOverridingCursorPositioning(this);
			return;
		}
		_isDragging = false;
		RunManager.Instance.InputSynchronizer.StopOverridingCursorPositioning();
		_backButton.Disable();
		Drawings.StopLineLocal();
		Drawings.SetDrawingModeLocal(DrawingMode.None);
		_drawingInput?.StopDrawing();
		UpdateDrawingButtonStates();
	}

	private void OnCapstoneChanged()
	{
		_backstop.Visible = !(NCapstoneContainer.Instance?.InUse ?? false);
		if (base.Visible)
		{
			if (!_backstop.Visible)
			{
				NRun.Instance.GlobalUi.TopBar.Map.StopOscillation();
			}
			else
			{
				NRun.Instance.GlobalUi.TopBar.Map.StartOscillation();
			}
		}
	}

	public void Close(bool animateOut = true)
	{
		if (IsOpen)
		{
			IsOpen = false;
			base.FocusMode = FocusModeEnum.None;
			NRun.Instance.GlobalUi.TopBar.Map.StopOscillation();
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.accept, OnLegendHotkeyPressed);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(MegaInput.viewExhaustPileAndTabRight, OnDrawingToolsHotkeyPressed);
			if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
			{
				CombatManager.Instance.Unpause();
			}
			_backButton.Disable();
			ActiveScreenContext.Instance.Update();
			EmitSignalClosed();
			if (animateOut)
			{
				TaskHelper.RunSafely(AnimClose());
				SfxCmd.Play("event:/sfx/ui/map/map_close");
			}
			else
			{
				base.Visible = false;
				base.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	private async Task AnimClose()
	{
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_backstop, "modulate:a", 0f, 0.15);
		_tween.TweenProperty(_points, "modulate:a", 0f, 0.15);
		_tween.TweenProperty(_mapContainer, "modulate", StsColors.transparentBlack, 0.25).SetDelay(0.1);
		_tween.TweenProperty(_mapContainer, "position:y", _mapContainer.Position.Y + 200f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_mapLegend, "modulate:a", 0f, 0.15);
		_tween.TweenProperty(_mapLegend, "position:x", MapLegendX + 120f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_drawingTools, "modulate:a", 0f, 0.15);
		await ToSignal(_tween, Tween.SignalName.Finished);
		base.Visible = false;
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	public NMapScreen Open(bool isOpenedFromTopBar = false)
	{
		if (IsOpen)
		{
			return this;
		}
		IsOpen = true;
		base.Visible = true;
		_backButton.MoveToHidePosition();
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.accept, OnLegendHotkeyPressed);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(MegaInput.viewExhaustPileAndTabRight, OnDrawingToolsHotkeyPressed);
		if (_runState.ActFloor > 0)
		{
			_backButton.Enable();
		}
		base.ProcessMode = ProcessModeEnum.Inherit;
		NRun.Instance.GlobalUi.TopBar.Map.StartOscillation();
		if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
		{
			CombatManager.Instance.Pause();
		}
		Color modulate;
		if (((_runState.CurrentActIndex == 0 && _runState.ExtraFields.StartedWithNeow) ? (_runState.ActFloor == 1) : (_runState.ActFloor == 0)) && !_hasPlayedAnimation)
		{
			if (!isOpenedFromTopBar && (SaveManager.Instance.PrefsSave.FastMode < FastModeType.Fast || !SaveManager.Instance.SeenFtue("map_select_ftue")))
			{
				PlayStartOfActAnimation();
			}
			else
			{
				_hasPlayedAnimation = true;
				Control mapContainer = _mapContainer;
				Vector2 position = _mapContainer.Position;
				position.Y = -600f;
				mapContainer.Position = position;
				_targetDragPos = new Vector2(0f, -600f);
				NActBanner child = NActBanner.Create(_runState.Act, _runState.CurrentActIndex);
				NRun.Instance.GlobalUi.MapScreen.AddChildSafely(child);
			}
		}
		else
		{
			int num = _runState.CurrentMapCoord?.row ?? 0;
			_targetDragPos = new Vector2(0f, -600f + (float)num * _distY);
			_mapContainer.Position = new Vector2(0f, -600f + (float)num * _distY);
			Control points = _points;
			modulate = _points.Modulate;
			modulate.A = 0f;
			points.Modulate = modulate;
			Control backstop = _backstop;
			modulate = _backstop.Modulate;
			modulate.A = 0f;
			backstop.Modulate = modulate;
			_mapLegend.Modulate = StsColors.transparentBlack;
			_drawingTools.Modulate = StsColors.transparentBlack;
		}
		Control mapLegend = _mapLegend;
		modulate = _mapLegend.Modulate;
		modulate.A = 0f;
		mapLegend.Modulate = modulate;
		Control drawingTools = _drawingTools;
		modulate = _drawingTools.Modulate;
		modulate.A = 0f;
		drawingTools.Modulate = modulate;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_backstop, "modulate:a", 0.85f, 0.25);
		_tween.TweenProperty(_mapContainer, "modulate", Colors.White, 0.25).From(StsColors.transparentBlack);
		_tween.TweenProperty(_mapLegend, "modulate", Colors.White, 0.25).SetDelay(0.1);
		_tween.TweenProperty(_mapLegend, "position:x", MapLegendX, 0.25).From(MapLegendX + 120f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Back)
			.SetDelay(0.1);
		_tween.TweenProperty(_drawingTools, "modulate", Colors.White, 0.25).SetDelay(0.2);
		_tween.TweenProperty(_points, "modulate:a", 1f, 0.25).SetDelay(0.1);
		RecalculateTravelability();
		if (_runState.VisitedMapCoords.Count != 0)
		{
			IReadOnlyList<MapCoord> visitedMapCoords = _runState.VisitedMapCoords;
			MapCoord key = visitedMapCoords[visitedMapCoords.Count - 1];
			if (_bossPointNode.Point.coord.row != key.row && _startingPointNode.Point.coord.row != key.row)
			{
				NMapPoint mapPoint = _mapPointDictionary[key];
				_marker.SetMapPoint(mapPoint);
			}
		}
		SfxCmd.Play("event:/sfx/ui/map/map_open");
		ActiveScreenContext.Instance.Update();
		EmitSignalOpened();
		NMapPoint nMapPoint = _mapPointDictionary.Values.FirstOrDefault((NMapPoint n) => n.IsEnabled);
		if (nMapPoint == null)
		{
			base.FocusMode = FocusModeEnum.All;
		}
		return this;
	}

	private void OnBackButtonPressed(NButton _)
	{
		Close();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if ((GetViewport().GuiGetFocusOwner() is NMapPoint || HasFocus()) && ActiveScreenContext.Instance.IsCurrent(this))
		{
			if (inputEvent.IsActionReleased(DebugHotkey.unlockCharacters))
			{
				_mapLegend.Visible = !_mapLegend.Visible;
				NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_mapLegend.Visible ? "Show Legend" : "Hide Legend"));
			}
			if (IsVisibleInTree())
			{
				ProcessControllerEvent(inputEvent);
			}
		}
	}

	private void OnMapDrawingButtonPressed(NButton _)
	{
		NMapDrawingInput drawingInput = _drawingInput;
		if (drawingInput != null && drawingInput.DrawingMode == DrawingMode.Drawing)
		{
			_drawingInput?.StopDrawing();
		}
		else
		{
			_drawingInput?.StopDrawing();
			_drawingInput = NMapDrawingInput.Create(Drawings, DrawingMode.Drawing);
			_drawingInput.Connect(NMapDrawingInput.SignalName.Finished, Callable.From(delegate
			{
				_drawingInput = null;
				UpdateDrawingButtonStates();
			}));
			this.AddChildSafely(_drawingInput);
		}
		UpdateDrawingButtonStates();
	}

	private void OnMapErasingButtonPressed(NButton _)
	{
		NMapDrawingInput drawingInput = _drawingInput;
		if (drawingInput != null && drawingInput.DrawingMode == DrawingMode.Erasing)
		{
			_drawingInput?.StopDrawing();
		}
		else
		{
			_drawingInput?.StopDrawing();
			_drawingInput = NMapDrawingInput.Create(Drawings, DrawingMode.Erasing);
			_drawingInput.Connect(NMapDrawingInput.SignalName.Finished, Callable.From(delegate
			{
				_drawingInput = null;
				UpdateDrawingButtonStates();
			}));
			this.AddChildSafely(_drawingInput);
		}
		UpdateDrawingButtonStates();
	}

	private void UpdateDrawingButtonStates()
	{
		_mapDrawingButton.SetIsDrawing(Drawings.GetLocalDrawingMode() == DrawingMode.Drawing);
		_mapErasingButton.SetIsErasing(Drawings.GetLocalDrawingMode() == DrawingMode.Erasing);
	}

	private void OnClearMapDrawingButtonPressed(NButton _)
	{
		Drawings.ClearDrawnLinesLocal();
		SfxCmd.Play("event:/sfx/ui/map/map_erase");
		UpdateDrawingButtonStates();
	}

	public void HighlightPointType(MapPointType pointType)
	{
		this.PointTypeHighlighted?.Invoke(pointType);
	}

	public void PingMapCoord(MapCoord coord, Player player)
	{
		if (!_mapPointDictionary.TryGetValue(coord, out NMapPoint value))
		{
			Log.Error($"Someone tried to ping map coord {coord} that doesn't exist!");
			return;
		}
		NMapPingVfx nMapPingVfx = NMapPingVfx.Create();
		nMapPingVfx.Modulate = player.Character.MapDrawingColor;
		value.AddChildSafely(nMapPingVfx);
		value.MoveChild(nMapPingVfx, 0);
		nMapPingVfx.Position = Vector2.Zero;
		nMapPingVfx.Size *= value.Size.X * (1f / 64f);
		nMapPingVfx.PivotOffset = nMapPingVfx.Size * 0.5f;
		NRun.Instance.GlobalUi.MultiplayerPlayerContainer.FlashPlayerReady(player);
		NDebugAudioManager.Instance.Play("map_ping.mp3", 1f, PitchVariance.Medium);
	}

	private void OnLegendHotkeyPressed()
	{
		List<NMapLegendItem> list = _legendItems.GetChildren().OfType<NMapLegendItem>().ToList();
		if (list.Any((NMapLegendItem c) => GetViewport().GuiGetFocusOwner() == c))
		{
			_mapPointDictionary.Values.FirstOrDefault((NMapPoint n) => n.IsEnabled)?.TryGrabFocus();
			return;
		}
		NMapPoint nMapPoint = _mapPointDictionary.Values.LastOrDefault((NMapPoint n) => n.IsEnabled);
		if (nMapPoint != null)
		{
			foreach (NMapLegendItem item in list)
			{
				if (nMapPoint != null)
				{
					item.FocusNeighborLeft = nMapPoint.GetPath();
				}
				else
				{
					item.FocusNeighborLeft = GetPath();
				}
			}
		}
		list[0].TryGrabFocus();
	}

	private void OnDrawingToolsHotkeyPressed()
	{
		NMapDrawingInput drawingInput = _drawingInput;
		if (drawingInput != null && drawingInput.DrawingMode == DrawingMode.Erasing)
		{
			_mapErasingButton.TryGrabFocus();
		}
		else
		{
			_mapDrawingButton.TryGrabFocus();
		}
	}

	public Vector2 GetNetPositionFromScreenPosition(Vector2 screenPosition)
	{
		Vector2 vector = _mapBgContainer.GetGlobalTransformWithCanvas().Inverse() * screenPosition;
		Vector2 vector2 = _mapBgContainer.Size * 0.5f;
		Vector2 vector3 = new Vector2(960f, vector2.Y);
		return (vector - vector2) / vector3;
	}

	private Vector2 GetMapPositionFromNetPosition(Vector2 netPosition)
	{
		Vector2 vector = _mapBgContainer.Size * 0.5f;
		Vector2 vector2 = new Vector2(960f, vector.Y);
		return netPosition * vector2 + vector;
	}

	public Vector2 GetScreenPositionFromNetPosition(Vector2 netPosition)
	{
		Vector2 mapPositionFromNetPosition = GetMapPositionFromNetPosition(netPosition);
		return _mapBgContainer.GetGlobalTransformWithCanvas() * mapPositionFromNetPosition;
	}

	public bool IsNodeOnScreen(NMapPoint mapPoint)
	{
		float y = mapPoint.GlobalPosition.Y;
		if (y > 0f)
		{
			return y < base.Size.Y;
		}
		return false;
	}

	public void CleanUp()
	{
		if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
		{
			CombatManager.Instance.Unpause();
		}
	}

	private void UpdateHotkeyDisplay()
	{
		_legendHotkeyIcon.Visible = NControllerManager.Instance.IsUsingController;
		_legendHotkeyIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.accept);
		_drawingToolsHotkeyIcon.Visible = NControllerManager.Instance.IsUsingController;
		_drawingToolsHotkeyIcon.Texture = NInputManager.Instance.GetHotkeyIcon(MegaInput.viewExhaustPileAndTabRight);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(42);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetLineEndpoint, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "point", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RecalculateTravelability, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitMapVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMapPointSelectedLocally, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "point", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshAllMapPointVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveAllMapPointsAndPaths, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateScrollPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessMouseEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessMouseDrawingEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessScrollEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessControllerEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTravelEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetDebugTravelEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "enabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshAllPointVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayStartOfActAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitMapPrompt, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetInterruptable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CanScroll, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryCancelStartOfActAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnVisibilityChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCapstoneChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Close, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "animateOut", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isOpenedFromTopBar", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnBackButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMapDrawingButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMapErasingButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateDrawingButtonStates, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnClearMapDrawingButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HighlightPointType, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnLegendHotkeyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDrawingToolsHotkeyPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetNetPositionFromScreenPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "screenPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetMapPositionFromNetPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "netPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetScreenPositionFromNetPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "netPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsNodeOnScreen, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "mapPoint", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CleanUp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHotkeyDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.GetLineEndpoint && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetLineEndpoint(VariantUtils.ConvertTo<NMapPoint>(in args[0])));
			return true;
		}
		if (method == MethodName.RecalculateTravelability && args.Count == 0)
		{
			RecalculateTravelability();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitMapVotes && args.Count == 0)
		{
			InitMapVotes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMapPointSelectedLocally && args.Count == 1)
		{
			OnMapPointSelectedLocally(VariantUtils.ConvertTo<NMapPoint>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshAllMapPointVotes && args.Count == 0)
		{
			RefreshAllMapPointVotes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveAllMapPointsAndPaths && args.Count == 0)
		{
			RemoveAllMapPointsAndPaths();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateScrollPosition && args.Count == 1)
		{
			UpdateScrollPosition(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessMouseEvent && args.Count == 1)
		{
			ProcessMouseEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessMouseDrawingEvent && args.Count == 1)
		{
			ProcessMouseDrawingEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessScrollEvent && args.Count == 1)
		{
			ProcessScrollEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessControllerEvent && args.Count == 1)
		{
			ProcessControllerEvent(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTravelEnabled && args.Count == 1)
		{
			SetTravelEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDebugTravelEnabled && args.Count == 1)
		{
			SetDebugTravelEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshAllPointVisuals && args.Count == 0)
		{
			RefreshAllPointVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayStartOfActAnimation && args.Count == 0)
		{
			PlayStartOfActAnimation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitMapPrompt && args.Count == 0)
		{
			InitMapPrompt();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetInterruptable && args.Count == 0)
		{
			SetInterruptable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CanScroll && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(CanScroll());
			return true;
		}
		if (method == MethodName.TryCancelStartOfActAnim && args.Count == 0)
		{
			TryCancelStartOfActAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnVisibilityChanged && args.Count == 0)
		{
			OnVisibilityChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCapstoneChanged && args.Count == 0)
		{
			OnCapstoneChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Close && args.Count == 1)
		{
			Close(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Open && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMapScreen>(Open(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		if (method == MethodName.OnBackButtonPressed && args.Count == 1)
		{
			OnBackButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMapDrawingButtonPressed && args.Count == 1)
		{
			OnMapDrawingButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMapErasingButtonPressed && args.Count == 1)
		{
			OnMapErasingButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateDrawingButtonStates && args.Count == 0)
		{
			UpdateDrawingButtonStates();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnClearMapDrawingButtonPressed && args.Count == 1)
		{
			OnClearMapDrawingButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HighlightPointType && args.Count == 1)
		{
			HighlightPointType(VariantUtils.ConvertTo<MapPointType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnLegendHotkeyPressed && args.Count == 0)
		{
			OnLegendHotkeyPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDrawingToolsHotkeyPressed && args.Count == 0)
		{
			OnDrawingToolsHotkeyPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetNetPositionFromScreenPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetNetPositionFromScreenPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.GetMapPositionFromNetPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetMapPositionFromNetPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.GetScreenPositionFromNetPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetScreenPositionFromNetPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.IsNodeOnScreen && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsNodeOnScreen(VariantUtils.ConvertTo<NMapPoint>(in args[0])));
			return true;
		}
		if (method == MethodName.CleanUp && args.Count == 0)
		{
			CleanUp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHotkeyDisplay && args.Count == 0)
		{
			UpdateHotkeyDisplay();
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
		if (method == MethodName.GetLineEndpoint)
		{
			return true;
		}
		if (method == MethodName.RecalculateTravelability)
		{
			return true;
		}
		if (method == MethodName.InitMapVotes)
		{
			return true;
		}
		if (method == MethodName.OnMapPointSelectedLocally)
		{
			return true;
		}
		if (method == MethodName.RefreshAllMapPointVotes)
		{
			return true;
		}
		if (method == MethodName.RemoveAllMapPointsAndPaths)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.UpdateScrollPosition)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.ProcessMouseEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessMouseDrawingEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessScrollEvent)
		{
			return true;
		}
		if (method == MethodName.ProcessControllerEvent)
		{
			return true;
		}
		if (method == MethodName.SetTravelEnabled)
		{
			return true;
		}
		if (method == MethodName.SetDebugTravelEnabled)
		{
			return true;
		}
		if (method == MethodName.RefreshAllPointVisuals)
		{
			return true;
		}
		if (method == MethodName.PlayStartOfActAnimation)
		{
			return true;
		}
		if (method == MethodName.InitMapPrompt)
		{
			return true;
		}
		if (method == MethodName.SetInterruptable)
		{
			return true;
		}
		if (method == MethodName.CanScroll)
		{
			return true;
		}
		if (method == MethodName.TryCancelStartOfActAnim)
		{
			return true;
		}
		if (method == MethodName.OnVisibilityChanged)
		{
			return true;
		}
		if (method == MethodName.OnCapstoneChanged)
		{
			return true;
		}
		if (method == MethodName.Close)
		{
			return true;
		}
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.OnBackButtonPressed)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.OnMapDrawingButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnMapErasingButtonPressed)
		{
			return true;
		}
		if (method == MethodName.UpdateDrawingButtonStates)
		{
			return true;
		}
		if (method == MethodName.OnClearMapDrawingButtonPressed)
		{
			return true;
		}
		if (method == MethodName.HighlightPointType)
		{
			return true;
		}
		if (method == MethodName.OnLegendHotkeyPressed)
		{
			return true;
		}
		if (method == MethodName.OnDrawingToolsHotkeyPressed)
		{
			return true;
		}
		if (method == MethodName.GetNetPositionFromScreenPosition)
		{
			return true;
		}
		if (method == MethodName.GetMapPositionFromNetPosition)
		{
			return true;
		}
		if (method == MethodName.GetScreenPositionFromNetPosition)
		{
			return true;
		}
		if (method == MethodName.IsNodeOnScreen)
		{
			return true;
		}
		if (method == MethodName.CleanUp)
		{
			return true;
		}
		if (method == MethodName.UpdateHotkeyDisplay)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsOpen)
		{
			IsOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsTravelEnabled)
		{
			IsTravelEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsDebugTravelEnabled)
		{
			IsDebugTravelEnabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.IsTraveling)
		{
			IsTraveling = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.Drawings)
		{
			Drawings = VariantUtils.ConvertTo<NMapDrawings>(in value);
			return true;
		}
		if (name == PropertyName._mapContainer)
		{
			_mapContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._pathsContainer)
		{
			_pathsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._points)
		{
			_points = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bossPointNode)
		{
			_bossPointNode = VariantUtils.ConvertTo<NBossMapPoint>(in value);
			return true;
		}
		if (name == PropertyName._secondBossPointNode)
		{
			_secondBossPointNode = VariantUtils.ConvertTo<NBossMapPoint>(in value);
			return true;
		}
		if (name == PropertyName._startingPointNode)
		{
			_startingPointNode = VariantUtils.ConvertTo<NMapPoint>(in value);
			return true;
		}
		if (name == PropertyName._mapBgContainer)
		{
			_mapBgContainer = VariantUtils.ConvertTo<NMapBg>(in value);
			return true;
		}
		if (name == PropertyName._marker)
		{
			_marker = VariantUtils.ConvertTo<NMapMarker>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._drawingToolsHotkeyIcon)
		{
			_drawingToolsHotkeyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._drawingTools)
		{
			_drawingTools = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._mapDrawingButton)
		{
			_mapDrawingButton = VariantUtils.ConvertTo<NMapDrawButton>(in value);
			return true;
		}
		if (name == PropertyName._mapErasingButton)
		{
			_mapErasingButton = VariantUtils.ConvertTo<NMapEraseButton>(in value);
			return true;
		}
		if (name == PropertyName._mapClearButton)
		{
			_mapClearButton = VariantUtils.ConvertTo<NMapClearButton>(in value);
			return true;
		}
		if (name == PropertyName._mapLegend)
		{
			_mapLegend = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._legendItems)
		{
			_legendItems = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._legendHotkeyIcon)
		{
			_legendHotkeyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			_startDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			_targetDragPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			_isDragging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hasPlayedAnimation)
		{
			_hasPlayedAnimation = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._controllerScrollAmount)
		{
			_controllerScrollAmount = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._distX)
		{
			_distX = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._distY)
		{
			_distY = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._actAnimTween)
		{
			_actAnimTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._mapScrollAnimTimer)
		{
			_mapScrollAnimTimer = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._canInterruptAnim)
		{
			_canInterruptAnim = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isInputDisabled)
		{
			_isInputDisabled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._promptTween)
		{
			_promptTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._drawingInput)
		{
			_drawingInput = VariantUtils.ConvertTo<NMapDrawingInput>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.IsOpen)
		{
			from = IsOpen;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsTravelEnabled)
		{
			from = IsTravelEnabled;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsDebugTravelEnabled)
		{
			from = IsDebugTravelEnabled;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MapLegendX)
		{
			value = VariantUtils.CreateFrom<float>(MapLegendX);
			return true;
		}
		if (name == PropertyName.IsTraveling)
		{
			from = IsTraveling;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Drawings)
		{
			value = VariantUtils.CreateFrom<NMapDrawings>(Drawings);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._mapContainer)
		{
			value = VariantUtils.CreateFrom(in _mapContainer);
			return true;
		}
		if (name == PropertyName._pathsContainer)
		{
			value = VariantUtils.CreateFrom(in _pathsContainer);
			return true;
		}
		if (name == PropertyName._points)
		{
			value = VariantUtils.CreateFrom(in _points);
			return true;
		}
		if (name == PropertyName._bossPointNode)
		{
			value = VariantUtils.CreateFrom(in _bossPointNode);
			return true;
		}
		if (name == PropertyName._secondBossPointNode)
		{
			value = VariantUtils.CreateFrom(in _secondBossPointNode);
			return true;
		}
		if (name == PropertyName._startingPointNode)
		{
			value = VariantUtils.CreateFrom(in _startingPointNode);
			return true;
		}
		if (name == PropertyName._mapBgContainer)
		{
			value = VariantUtils.CreateFrom(in _mapBgContainer);
			return true;
		}
		if (name == PropertyName._marker)
		{
			value = VariantUtils.CreateFrom(in _marker);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._drawingToolsHotkeyIcon)
		{
			value = VariantUtils.CreateFrom(in _drawingToolsHotkeyIcon);
			return true;
		}
		if (name == PropertyName._drawingTools)
		{
			value = VariantUtils.CreateFrom(in _drawingTools);
			return true;
		}
		if (name == PropertyName._mapDrawingButton)
		{
			value = VariantUtils.CreateFrom(in _mapDrawingButton);
			return true;
		}
		if (name == PropertyName._mapErasingButton)
		{
			value = VariantUtils.CreateFrom(in _mapErasingButton);
			return true;
		}
		if (name == PropertyName._mapClearButton)
		{
			value = VariantUtils.CreateFrom(in _mapClearButton);
			return true;
		}
		if (name == PropertyName._mapLegend)
		{
			value = VariantUtils.CreateFrom(in _mapLegend);
			return true;
		}
		if (name == PropertyName._legendItems)
		{
			value = VariantUtils.CreateFrom(in _legendItems);
			return true;
		}
		if (name == PropertyName._legendHotkeyIcon)
		{
			value = VariantUtils.CreateFrom(in _legendHotkeyIcon);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._startDragPos)
		{
			value = VariantUtils.CreateFrom(in _startDragPos);
			return true;
		}
		if (name == PropertyName._targetDragPos)
		{
			value = VariantUtils.CreateFrom(in _targetDragPos);
			return true;
		}
		if (name == PropertyName._isDragging)
		{
			value = VariantUtils.CreateFrom(in _isDragging);
			return true;
		}
		if (name == PropertyName._hasPlayedAnimation)
		{
			value = VariantUtils.CreateFrom(in _hasPlayedAnimation);
			return true;
		}
		if (name == PropertyName._controllerScrollAmount)
		{
			value = VariantUtils.CreateFrom(in _controllerScrollAmount);
			return true;
		}
		if (name == PropertyName._distX)
		{
			value = VariantUtils.CreateFrom(in _distX);
			return true;
		}
		if (name == PropertyName._distY)
		{
			value = VariantUtils.CreateFrom(in _distY);
			return true;
		}
		if (name == PropertyName._actAnimTween)
		{
			value = VariantUtils.CreateFrom(in _actAnimTween);
			return true;
		}
		if (name == PropertyName._mapScrollAnimTimer)
		{
			value = VariantUtils.CreateFrom(in _mapScrollAnimTimer);
			return true;
		}
		if (name == PropertyName._mapAnimStartDelay)
		{
			value = VariantUtils.CreateFrom(in _mapAnimStartDelay);
			return true;
		}
		if (name == PropertyName._mapAnimDuration)
		{
			value = VariantUtils.CreateFrom(in _mapAnimDuration);
			return true;
		}
		if (name == PropertyName._canInterruptAnim)
		{
			value = VariantUtils.CreateFrom(in _canInterruptAnim);
			return true;
		}
		if (name == PropertyName._isInputDisabled)
		{
			value = VariantUtils.CreateFrom(in _isInputDisabled);
			return true;
		}
		if (name == PropertyName._promptTween)
		{
			value = VariantUtils.CreateFrom(in _promptTween);
			return true;
		}
		if (name == PropertyName._drawingInput)
		{
			value = VariantUtils.CreateFrom(in _drawingInput);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsTravelEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsDebugTravelEnabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._pathsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._points, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bossPointNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._secondBossPointNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._startingPointNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapBgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._marker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawingToolsHotkeyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawingTools, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapDrawingButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapErasingButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapClearButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mapLegend, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._legendItems, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._legendHotkeyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._startDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetDragPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDragging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hasPlayedAnimation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.MapLegendX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsTraveling, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._controllerScrollAmount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._distX, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._distY, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actAnimTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._mapScrollAnimTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._mapAnimStartDelay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._mapAnimDuration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._canInterruptAnim, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInputDisabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._promptTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Drawings, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawingInput, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsOpen, Variant.From<bool>(IsOpen));
		info.AddProperty(PropertyName.IsTravelEnabled, Variant.From<bool>(IsTravelEnabled));
		info.AddProperty(PropertyName.IsDebugTravelEnabled, Variant.From<bool>(IsDebugTravelEnabled));
		info.AddProperty(PropertyName.IsTraveling, Variant.From<bool>(IsTraveling));
		info.AddProperty(PropertyName.Drawings, Variant.From<NMapDrawings>(Drawings));
		info.AddProperty(PropertyName._mapContainer, Variant.From(in _mapContainer));
		info.AddProperty(PropertyName._pathsContainer, Variant.From(in _pathsContainer));
		info.AddProperty(PropertyName._points, Variant.From(in _points));
		info.AddProperty(PropertyName._bossPointNode, Variant.From(in _bossPointNode));
		info.AddProperty(PropertyName._secondBossPointNode, Variant.From(in _secondBossPointNode));
		info.AddProperty(PropertyName._startingPointNode, Variant.From(in _startingPointNode));
		info.AddProperty(PropertyName._mapBgContainer, Variant.From(in _mapBgContainer));
		info.AddProperty(PropertyName._marker, Variant.From(in _marker));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._drawingToolsHotkeyIcon, Variant.From(in _drawingToolsHotkeyIcon));
		info.AddProperty(PropertyName._drawingTools, Variant.From(in _drawingTools));
		info.AddProperty(PropertyName._mapDrawingButton, Variant.From(in _mapDrawingButton));
		info.AddProperty(PropertyName._mapErasingButton, Variant.From(in _mapErasingButton));
		info.AddProperty(PropertyName._mapClearButton, Variant.From(in _mapClearButton));
		info.AddProperty(PropertyName._mapLegend, Variant.From(in _mapLegend));
		info.AddProperty(PropertyName._legendItems, Variant.From(in _legendItems));
		info.AddProperty(PropertyName._legendHotkeyIcon, Variant.From(in _legendHotkeyIcon));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._startDragPos, Variant.From(in _startDragPos));
		info.AddProperty(PropertyName._targetDragPos, Variant.From(in _targetDragPos));
		info.AddProperty(PropertyName._isDragging, Variant.From(in _isDragging));
		info.AddProperty(PropertyName._hasPlayedAnimation, Variant.From(in _hasPlayedAnimation));
		info.AddProperty(PropertyName._controllerScrollAmount, Variant.From(in _controllerScrollAmount));
		info.AddProperty(PropertyName._distX, Variant.From(in _distX));
		info.AddProperty(PropertyName._distY, Variant.From(in _distY));
		info.AddProperty(PropertyName._actAnimTween, Variant.From(in _actAnimTween));
		info.AddProperty(PropertyName._mapScrollAnimTimer, Variant.From(in _mapScrollAnimTimer));
		info.AddProperty(PropertyName._canInterruptAnim, Variant.From(in _canInterruptAnim));
		info.AddProperty(PropertyName._isInputDisabled, Variant.From(in _isInputDisabled));
		info.AddProperty(PropertyName._promptTween, Variant.From(in _promptTween));
		info.AddProperty(PropertyName._drawingInput, Variant.From(in _drawingInput));
		info.AddSignalEventDelegate(SignalName.Opened, backing_Opened);
		info.AddSignalEventDelegate(SignalName.Closed, backing_Closed);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsOpen, out var value))
		{
			IsOpen = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsTravelEnabled, out var value2))
		{
			IsTravelEnabled = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsDebugTravelEnabled, out var value3))
		{
			IsDebugTravelEnabled = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.IsTraveling, out var value4))
		{
			IsTraveling = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.Drawings, out var value5))
		{
			Drawings = value5.As<NMapDrawings>();
		}
		if (info.TryGetProperty(PropertyName._mapContainer, out var value6))
		{
			_mapContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._pathsContainer, out var value7))
		{
			_pathsContainer = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._points, out var value8))
		{
			_points = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bossPointNode, out var value9))
		{
			_bossPointNode = value9.As<NBossMapPoint>();
		}
		if (info.TryGetProperty(PropertyName._secondBossPointNode, out var value10))
		{
			_secondBossPointNode = value10.As<NBossMapPoint>();
		}
		if (info.TryGetProperty(PropertyName._startingPointNode, out var value11))
		{
			_startingPointNode = value11.As<NMapPoint>();
		}
		if (info.TryGetProperty(PropertyName._mapBgContainer, out var value12))
		{
			_mapBgContainer = value12.As<NMapBg>();
		}
		if (info.TryGetProperty(PropertyName._marker, out var value13))
		{
			_marker = value13.As<NMapMarker>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value14))
		{
			_backButton = value14.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._drawingToolsHotkeyIcon, out var value15))
		{
			_drawingToolsHotkeyIcon = value15.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._drawingTools, out var value16))
		{
			_drawingTools = value16.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._mapDrawingButton, out var value17))
		{
			_mapDrawingButton = value17.As<NMapDrawButton>();
		}
		if (info.TryGetProperty(PropertyName._mapErasingButton, out var value18))
		{
			_mapErasingButton = value18.As<NMapEraseButton>();
		}
		if (info.TryGetProperty(PropertyName._mapClearButton, out var value19))
		{
			_mapClearButton = value19.As<NMapClearButton>();
		}
		if (info.TryGetProperty(PropertyName._mapLegend, out var value20))
		{
			_mapLegend = value20.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._legendItems, out var value21))
		{
			_legendItems = value21.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._legendHotkeyIcon, out var value22))
		{
			_legendHotkeyIcon = value22.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value23))
		{
			_backstop = value23.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value24))
		{
			_tween = value24.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._startDragPos, out var value25))
		{
			_startDragPos = value25.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetDragPos, out var value26))
		{
			_targetDragPos = value26.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isDragging, out var value27))
		{
			_isDragging = value27.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hasPlayedAnimation, out var value28))
		{
			_hasPlayedAnimation = value28.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._controllerScrollAmount, out var value29))
		{
			_controllerScrollAmount = value29.As<float>();
		}
		if (info.TryGetProperty(PropertyName._distX, out var value30))
		{
			_distX = value30.As<float>();
		}
		if (info.TryGetProperty(PropertyName._distY, out var value31))
		{
			_distY = value31.As<float>();
		}
		if (info.TryGetProperty(PropertyName._actAnimTween, out var value32))
		{
			_actAnimTween = value32.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._mapScrollAnimTimer, out var value33))
		{
			_mapScrollAnimTimer = value33.As<float>();
		}
		if (info.TryGetProperty(PropertyName._canInterruptAnim, out var value34))
		{
			_canInterruptAnim = value34.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isInputDisabled, out var value35))
		{
			_isInputDisabled = value35.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._promptTween, out var value36))
		{
			_promptTween = value36.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._drawingInput, out var value37))
		{
			_drawingInput = value37.As<NMapDrawingInput>();
		}
		if (info.TryGetSignalEventDelegate<OpenedEventHandler>(SignalName.Opened, out var value38))
		{
			backing_Opened = value38;
		}
		if (info.TryGetSignalEventDelegate<ClosedEventHandler>(SignalName.Closed, out var value39))
		{
			backing_Closed = value39;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Opened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.Closed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalOpened()
	{
		EmitSignal(SignalName.Opened);
	}

	protected void EmitSignalClosed()
	{
		EmitSignal(SignalName.Closed);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Opened && args.Count == 0)
		{
			backing_Opened?.Invoke();
		}
		else if (signal == SignalName.Closed && args.Count == 0)
		{
			backing_Closed?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Opened)
		{
			return true;
		}
		if (signal == SignalName.Closed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
