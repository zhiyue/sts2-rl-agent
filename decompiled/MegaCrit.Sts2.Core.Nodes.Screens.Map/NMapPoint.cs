using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

[ScriptPath("res://src/Core/Nodes/Screens/Map/NMapPoint.cs")]
public abstract class NMapPoint : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ConnectSignals' method.
		/// </summary>
		public new static readonly StringName ConnectSignals = "ConnectSignals";

		/// <summary>
		/// Cached name for the 'IsInputAllowed' method.
		/// </summary>
		public static readonly StringName IsInputAllowed = "IsInputAllowed";

		/// <summary>
		/// Cached name for the 'RefreshVisualsInstantly' method.
		/// </summary>
		public static readonly StringName RefreshVisualsInstantly = "RefreshVisualsInstantly";

		/// <summary>
		/// Cached name for the 'OnSelected' method.
		/// </summary>
		public static readonly StringName OnSelected = "OnSelected";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'RefreshColorInstantly' method.
		/// </summary>
		public static readonly StringName RefreshColorInstantly = "RefreshColorInstantly";

		/// <summary>
		/// Cached name for the 'RefreshState' method.
		/// </summary>
		public static readonly StringName RefreshState = "RefreshState";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'TraveledColor' property.
		/// </summary>
		public static readonly StringName TraveledColor = "TraveledColor";

		/// <summary>
		/// Cached name for the 'UntravelableColor' property.
		/// </summary>
		public static readonly StringName UntravelableColor = "UntravelableColor";

		/// <summary>
		/// Cached name for the 'HoveredColor' property.
		/// </summary>
		public static readonly StringName HoveredColor = "HoveredColor";

		/// <summary>
		/// Cached name for the 'HoverScale' property.
		/// </summary>
		public static readonly StringName HoverScale = "HoverScale";

		/// <summary>
		/// Cached name for the 'DownScale' property.
		/// </summary>
		public static readonly StringName DownScale = "DownScale";

		/// <summary>
		/// Cached name for the 'AllowFocusWhileDisabled' property.
		/// </summary>
		public new static readonly StringName AllowFocusWhileDisabled = "AllowFocusWhileDisabled";

		/// <summary>
		/// Cached name for the 'VoteContainer' property.
		/// </summary>
		public static readonly StringName VoteContainer = "VoteContainer";

		/// <summary>
		/// Cached name for the 'IsTravelable' property.
		/// </summary>
		public static readonly StringName IsTravelable = "IsTravelable";

		/// <summary>
		/// Cached name for the 'State' property.
		/// </summary>
		public static readonly StringName State = "State";

		/// <summary>
		/// Cached name for the 'TargetColor' property.
		/// </summary>
		public static readonly StringName TargetColor = "TargetColor";

		/// <summary>
		/// Cached name for the '_state' field.
		/// </summary>
		public static readonly StringName _state = "_state";

		/// <summary>
		/// Cached name for the '_outlineColor' field.
		/// </summary>
		public static readonly StringName _outlineColor = "_outlineColor";

		/// <summary>
		/// Cached name for the '_controllerSelectionReticle' field.
		/// </summary>
		public static readonly StringName _controllerSelectionReticle = "_controllerSelectionReticle";

		/// <summary>
		/// Cached name for the '_screen' field.
		/// </summary>
		public static readonly StringName _screen = "_screen";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private MapPointState _state = MapPointState.Untravelable;

	protected IRunState _runState;

	protected Color _outlineColor = new Color(1f, 1f, 1f, 0.75f);

	protected const double _pressDownDur = 0.3;

	protected const double _unhoverAnimDur = 0.5;

	protected NSelectionReticle _controllerSelectionReticle;

	protected NMapScreen _screen;

	protected abstract Color TraveledColor { get; }

	protected abstract Color UntravelableColor { get; }

	protected abstract Color HoveredColor { get; }

	protected abstract Vector2 HoverScale { get; }

	protected abstract Vector2 DownScale { get; }

	protected override bool AllowFocusWhileDisabled => true;

	public NMultiplayerVoteContainer VoteContainer { get; set; }

	/// <summary>
	/// Indicates whether a node is an adjacent node above the current node (you're able to travel to
	/// it IF you have completed the room).
	/// If you want logic to trigger based on the button being clickable, use _isEnabled instead.
	/// </summary>
	protected bool IsTravelable
	{
		get
		{
			NMapScreen screen = _screen;
			if (screen != null && screen.IsDebugTravelEnabled && !screen.IsTraveling)
			{
				return true;
			}
			if (_screen.IsTravelEnabled)
			{
				return State == MapPointState.Travelable;
			}
			return false;
		}
	}

	public MapPoint Point { get; protected set; }

	/// <summary>
	/// This map point's current state. Affects how it looks and its hoverability/clickability.
	/// </summary>
	public MapPointState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (value != _state)
			{
				_state = value;
				RefreshVisualsInstantly();
			}
		}
	}

	protected Color TargetColor
	{
		get
		{
			MapPointState state = State;
			if ((uint)(state - 1) <= 1u)
			{
				return TraveledColor;
			}
			return UntravelableColor;
		}
	}

	public override void _Ready()
	{
		if (GetType() != typeof(NMapPoint))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		VoteContainer = GetNode<NMultiplayerVoteContainer>("%MapPointVoteContainer");
		_controllerSelectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		VoteContainer.Initialize(ShouldDisplayPlayerVote, _runState.Players);
	}

	protected bool IsInputAllowed()
	{
		if (!_screen.IsTraveling)
		{
			return _screen.Drawings.GetLocalDrawingMode() == DrawingMode.None;
		}
		return false;
	}

	/// <summary>
	/// Returns true when a player's icon should be displayed to indicate that they've voted for this map point.
	/// </summary>
	private bool ShouldDisplayPlayerVote(Player player)
	{
		if (_screen.PlayerVoteDictionary.TryGetValue(player, out var value) && value.HasValue)
		{
			return value == Point.coord;
		}
		return _runState.MapLocation.coord == Point.coord;
	}

	public void RefreshVisualsInstantly()
	{
		_controllerSelectionReticle.OnDeselect();
		RefreshColorInstantly();
		RefreshState();
	}

	/// <summary>
	/// Override this to add extra behavior when a map node selection.
	/// Usually for visuals.
	/// </summary>
	public virtual void OnSelected()
	{
	}

	/// <summary>
	/// The base class handles the logic for selecting a map point, so we don't allow overriding it. Instead, override
	/// OnSelected to run logic before selecting the map point.
	/// </summary>
	protected sealed override void OnRelease()
	{
		if (IsTravelable && (Point.coord.row != 0 || !TestMode.IsOff || SaveManager.Instance.SeenFtue("map_select_ftue")) && _screen.Drawings.GetLocalDrawingMode() == DrawingMode.None && (_screen.IsNodeOnScreen(this) || !NControllerManager.Instance.IsUsingController))
		{
			_screen.OnMapPointSelectedLocally(this);
		}
	}

	protected virtual void RefreshColorInstantly()
	{
	}

	protected virtual void RefreshState()
	{
		if (IsTravelable)
		{
			Enable();
		}
		else
		{
			Disable();
		}
	}

	protected override void OnFocus()
	{
		if (!IsInputAllowed())
		{
			return;
		}
		if (_isEnabled && NControllerManager.Instance.IsUsingController)
		{
			_controllerSelectionReticle.OnSelect();
		}
		if (_state != MapPointState.Traveled || !(_runState.MapLocation.coord != Point.coord) || NControllerManager.Instance.IsUsingController)
		{
			return;
		}
		MapPointHistoryEntry historyEntryFor = _runState.GetHistoryEntryFor(new MapLocation(Point.coord, _runState.CurrentActIndex));
		if (historyEntryFor != null)
		{
			int num = Point.coord.row + 1;
			for (int i = 0; i < _runState.MapPointHistory.Count - 1; i++)
			{
				num += _runState.MapPointHistory[i].Count;
			}
			NHoverTipSet tip = NHoverTipSet.CreateAndShowMapPointHistory(this, NMapPointHistoryHoverTip.Create(num, LocalContext.NetId.Value, historyEntryFor));
			Callable.From(delegate
			{
				tip.SetAlignment(this, HoverTip.GetHoverTipAlignment(this));
			}).CallDeferred();
		}
	}

	protected override void OnUnfocus()
	{
		_controllerSelectionReticle.OnDeselect();
		NHoverTipSet.Remove(this);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsInputAllowed, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshVisualsInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshColorInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsInputAllowed && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsInputAllowed());
			return true;
		}
		if (method == MethodName.RefreshVisualsInstantly && args.Count == 0)
		{
			RefreshVisualsInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSelected && args.Count == 0)
		{
			OnSelected();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshColorInstantly && args.Count == 0)
		{
			RefreshColorInstantly();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshState && args.Count == 0)
		{
			RefreshState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.IsInputAllowed)
		{
			return true;
		}
		if (method == MethodName.RefreshVisualsInstantly)
		{
			return true;
		}
		if (method == MethodName.OnSelected)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.RefreshColorInstantly)
		{
			return true;
		}
		if (method == MethodName.RefreshState)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VoteContainer)
		{
			VoteContainer = VariantUtils.ConvertTo<NMultiplayerVoteContainer>(in value);
			return true;
		}
		if (name == PropertyName.State)
		{
			State = VariantUtils.ConvertTo<MapPointState>(in value);
			return true;
		}
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<MapPointState>(in value);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			_outlineColor = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._controllerSelectionReticle)
		{
			_controllerSelectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._screen)
		{
			_screen = VariantUtils.ConvertTo<NMapScreen>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Color from;
		if (name == PropertyName.TraveledColor)
		{
			from = TraveledColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.UntravelableColor)
		{
			from = UntravelableColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HoveredColor)
		{
			from = HoveredColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		Vector2 from2;
		if (name == PropertyName.HoverScale)
		{
			from2 = HoverScale;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.DownScale)
		{
			from2 = DownScale;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		bool from3;
		if (name == PropertyName.AllowFocusWhileDisabled)
		{
			from3 = AllowFocusWhileDisabled;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.VoteContainer)
		{
			value = VariantUtils.CreateFrom<NMultiplayerVoteContainer>(VoteContainer);
			return true;
		}
		if (name == PropertyName.IsTravelable)
		{
			from3 = IsTravelable;
			value = VariantUtils.CreateFrom(in from3);
			return true;
		}
		if (name == PropertyName.State)
		{
			value = VariantUtils.CreateFrom<MapPointState>(State);
			return true;
		}
		if (name == PropertyName.TargetColor)
		{
			from = TargetColor;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._outlineColor)
		{
			value = VariantUtils.CreateFrom(in _outlineColor);
			return true;
		}
		if (name == PropertyName._controllerSelectionReticle)
		{
			value = VariantUtils.CreateFrom(in _controllerSelectionReticle);
			return true;
		}
		if (name == PropertyName._screen)
		{
			value = VariantUtils.CreateFrom(in _screen);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.TraveledColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.UntravelableColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.HoveredColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._outlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.DownScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._controllerSelectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.AllowFocusWhileDisabled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VoteContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsTravelable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.State, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.TargetColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.VoteContainer, Variant.From<NMultiplayerVoteContainer>(VoteContainer));
		info.AddProperty(PropertyName.State, Variant.From<MapPointState>(State));
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._outlineColor, Variant.From(in _outlineColor));
		info.AddProperty(PropertyName._controllerSelectionReticle, Variant.From(in _controllerSelectionReticle));
		info.AddProperty(PropertyName._screen, Variant.From(in _screen));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VoteContainer, out var value))
		{
			VoteContainer = value.As<NMultiplayerVoteContainer>();
		}
		if (info.TryGetProperty(PropertyName.State, out var value2))
		{
			State = value2.As<MapPointState>();
		}
		if (info.TryGetProperty(PropertyName._state, out var value3))
		{
			_state = value3.As<MapPointState>();
		}
		if (info.TryGetProperty(PropertyName._outlineColor, out var value4))
		{
			_outlineColor = value4.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._controllerSelectionReticle, out var value5))
		{
			_controllerSelectionReticle = value5.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._screen, out var value6))
		{
			_screen = value6.As<NMapScreen>();
		}
	}
}
