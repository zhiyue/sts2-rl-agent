using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Relics;

/// <summary>
/// UI for holding relics. Is located under the top bar.
/// Positioning is handled by the HFlowContainer.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Relics/NRelicInventory.cs")]
public class NRelicInventory : FlowContainer
{
	[Signal]
	public delegate void RelicsChangedEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : FlowContainer.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'ConnectPlayerEvents' method.
		/// </summary>
		public static readonly StringName ConnectPlayerEvents = "ConnectPlayerEvents";

		/// <summary>
		/// Cached name for the 'DisconnectPlayerEvents' method.
		/// </summary>
		public static readonly StringName DisconnectPlayerEvents = "DisconnectPlayerEvents";

		/// <summary>
		/// Cached name for the 'OnRelicUnfocused' method.
		/// </summary>
		public static readonly StringName OnRelicUnfocused = "OnRelicUnfocused";

		/// <summary>
		/// Cached name for the 'AnimShow' method.
		/// </summary>
		public static readonly StringName AnimShow = "AnimShow";

		/// <summary>
		/// Cached name for the 'AnimHide' method.
		/// </summary>
		public static readonly StringName AnimHide = "AnimHide";

		/// <summary>
		/// Cached name for the 'ShowImmediately' method.
		/// </summary>
		public static readonly StringName ShowImmediately = "ShowImmediately";

		/// <summary>
		/// Cached name for the 'HideImmediately' method.
		/// </summary>
		public static readonly StringName HideImmediately = "HideImmediately";

		/// <summary>
		/// Cached name for the 'GetDefaultPosition' method.
		/// </summary>
		public static readonly StringName GetDefaultPosition = "GetDefaultPosition";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'DebugHideTopBar' method.
		/// </summary>
		public static readonly StringName DebugHideTopBar = "DebugHideTopBar";

		/// <summary>
		/// Cached name for the 'UpdateNavigation' method.
		/// </summary>
		public static readonly StringName UpdateNavigation = "UpdateNavigation";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : FlowContainer.PropertyName
	{
		/// <summary>
		/// Cached name for the '_originalPos' field.
		/// </summary>
		public static readonly StringName _originalPos = "_originalPos";

		/// <summary>
		/// Cached name for the '_curTween' field.
		/// </summary>
		public static readonly StringName _curTween = "_curTween";

		/// <summary>
		/// Cached name for the '_debugHideTween' field.
		/// </summary>
		public static readonly StringName _debugHideTween = "_debugHideTween";

		/// <summary>
		/// Cached name for the '_isDebugHidden' field.
		/// </summary>
		public static readonly StringName _isDebugHidden = "_isDebugHidden";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : FlowContainer.SignalName
	{
		/// <summary>
		/// Cached name for the 'RelicsChanged' signal.
		/// </summary>
		public static readonly StringName RelicsChanged = "RelicsChanged";
	}

	private Player? _player;

	private readonly List<NRelicInventoryHolder> _relicNodes = new List<NRelicInventoryHolder>();

	private Vector2 _originalPos;

	private Tween? _curTween;

	private Tween? _debugHideTween;

	private bool _isDebugHidden;

	private RelicsChangedEventHandler backing_RelicsChanged;

	public IReadOnlyList<NRelicInventoryHolder> RelicNodes => _relicNodes;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Relics.NRelicInventory.RelicsChangedEventHandler" />
	public event RelicsChangedEventHandler RelicsChanged
	{
		add
		{
			backing_RelicsChanged = (RelicsChangedEventHandler)Delegate.Combine(backing_RelicsChanged, value);
		}
		remove
		{
			backing_RelicsChanged = (RelicsChangedEventHandler)Delegate.Remove(backing_RelicsChanged, value);
		}
	}

	public override void _Ready()
	{
		_originalPos = base.Position;
		Connect(Control.SignalName.FocusEntered, Callable.From(delegate
		{
			_relicNodes[0].TryGrabFocus();
		}));
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		ConnectPlayerEvents();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		DisconnectPlayerEvents();
	}

	public void Initialize(RunState runState)
	{
		DisconnectPlayerEvents();
		_player = LocalContext.GetMe(runState);
		ConnectPlayerEvents();
		foreach (RelicModel relic in _player.Relics)
		{
			Add(relic, startsShown: true);
		}
		UpdateNavigation();
	}

	private void ConnectPlayerEvents()
	{
		if (_player != null)
		{
			_player.RelicObtained += OnRelicObtained;
			_player.RelicRemoved += OnRelicRemoved;
		}
	}

	private void DisconnectPlayerEvents()
	{
		if (_player != null)
		{
			_player.RelicObtained -= OnRelicObtained;
			_player.RelicRemoved -= OnRelicRemoved;
		}
	}

	private void Add(RelicModel relic, bool startsShown, int index = -1)
	{
		NRelicInventoryHolder nRelicInventoryHolder = NRelicInventoryHolder.Create(relic);
		nRelicInventoryHolder.Inventory = this;
		if (index < 0)
		{
			_relicNodes.Add(nRelicInventoryHolder);
		}
		else
		{
			_relicNodes.Insert(index, nRelicInventoryHolder);
		}
		this.AddChildSafely(nRelicInventoryHolder);
		this.MoveChildSafely(nRelicInventoryHolder, index);
		if (!startsShown)
		{
			TextureRect icon = nRelicInventoryHolder.Relic.Icon;
			Color modulate = nRelicInventoryHolder.Relic.Icon.Modulate;
			modulate.A = 0f;
			icon.Modulate = modulate;
			UpdateNavigation();
		}
		nRelicInventoryHolder.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			OnRelicClicked(relic);
		}));
		nRelicInventoryHolder.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(delegate
		{
			OnRelicFocused(relic);
		}));
		nRelicInventoryHolder.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(delegate
		{
			OnRelicUnfocused();
		}));
		EmitSignal(SignalName.RelicsChanged);
	}

	private void Remove(RelicModel relic)
	{
		if (LocalContext.IsMine(relic))
		{
			NRelicInventoryHolder nRelicInventoryHolder = _relicNodes.First((NRelicInventoryHolder n) => n.Relic.Model == relic);
			_relicNodes.Remove(nRelicInventoryHolder);
			this.RemoveChildSafely(nRelicInventoryHolder);
			EmitSignalRelicsChanged();
			UpdateNavigation();
		}
	}

	/// <summary>
	/// Opens the Inspect Relic screen and allows the player to paginate through the rest of the top bar relics.
	/// </summary>
	private void OnRelicClicked(RelicModel model)
	{
		List<RelicModel> list = new List<RelicModel>();
		foreach (NRelicInventoryHolder relicNode in _relicNodes)
		{
			list.Add(relicNode.Relic.Model);
		}
		NGame.Instance.GetInspectRelicScreen().Open(list, model);
	}

	private void OnRelicFocused(RelicModel model)
	{
		RunManager.Instance.HoveredModelTracker.OnLocalRelicHovered(model);
	}

	private static void OnRelicUnfocused()
	{
		RunManager.Instance.HoveredModelTracker.OnLocalRelicUnhovered();
	}

	public void AnimateRelic(RelicModel relic, Vector2? startPosition = null, Vector2? startScale = null)
	{
		if (LocalContext.IsMine(relic))
		{
			NRelicInventoryHolder nRelicInventoryHolder = _relicNodes.First((NRelicInventoryHolder n) => n.Relic.Model == relic);
			TaskHelper.RunSafely(nRelicInventoryHolder.PlayNewlyAcquiredAnimation(startPosition, startScale));
		}
	}

	private void OnRelicObtained(RelicModel relic)
	{
		Add(relic, startsShown: false, _player.Relics.IndexOf(relic));
	}

	private void OnRelicRemoved(RelicModel relic)
	{
		Remove(relic);
	}

	public void AnimShow()
	{
		base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Enabled;
		_curTween?.Kill();
		_curTween = CreateTween();
		_curTween.TweenProperty(this, "global_position:y", _originalPos.Y, 0.25).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	public void AnimHide()
	{
		base.FocusBehaviorRecursive = FocusBehaviorRecursiveEnum.Disabled;
		_curTween?.Kill();
		_curTween = CreateTween();
		_curTween.TweenProperty(this, "global_position:y", _originalPos.Y - 68f * (float)GetLineCount() - 90f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void ShowImmediately()
	{
		_curTween?.Kill();
		Vector2 position = base.Position;
		position.Y = _originalPos.Y;
		base.Position = position;
	}

	public void HideImmediately()
	{
		_curTween?.Kill();
		Vector2 position = base.Position;
		position.Y = _originalPos.Y - 68f * (float)GetLineCount() - 90f;
		base.Position = position;
	}

	/// <summary>
	/// Returns the position the relic inventory is at normally.
	/// this.Position can change if the relic inventory is hidden, so this is used when we want to position elements
	/// relative to the relic inventory.
	/// </summary>
	public Vector2 GetDefaultPosition()
	{
		return _originalPos;
	}

	/// <summary>
	/// Used for trailers
	/// </summary>
	/// <param name="inputEvent"></param>
	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideTopBar))
		{
			DebugHideTopBar();
		}
	}

	/// <summary>
	/// Used for trailers
	/// </summary>
	private void DebugHideTopBar()
	{
		if (_isDebugHidden)
		{
			AnimShow();
		}
		else
		{
			AnimHide();
		}
		_isDebugHidden = !_isDebugHidden;
	}

	private void UpdateNavigation()
	{
		for (int i = 0; i < RelicNodes.Count; i++)
		{
			NRelicInventoryHolder nRelicInventoryHolder = RelicNodes[i];
			NodePath path;
			if (i <= 0)
			{
				IReadOnlyList<NRelicInventoryHolder> relicNodes = RelicNodes;
				path = relicNodes[relicNodes.Count - 1].GetPath();
			}
			else
			{
				path = RelicNodes[i - 1].GetPath();
			}
			nRelicInventoryHolder.FocusNeighborLeft = path;
			nRelicInventoryHolder.FocusNeighborRight = ((i < RelicNodes.Count - 1) ? RelicNodes[i + 1].GetPath() : RelicNodes[0].GetPath());
			Control firstPotionControl = NRun.Instance.GlobalUi.TopBar.PotionContainer.FirstPotionControl;
			nRelicInventoryHolder.FocusNeighborTop = ((firstPotionControl != null && GodotObject.IsInstanceValid(firstPotionControl)) ? firstPotionControl.GetPath() : nRelicInventoryHolder.GetPath());
			NMultiplayerPlayerStateContainer multiplayerPlayerContainer = NRun.Instance.GlobalUi.MultiplayerPlayerContainer;
			if (multiplayerPlayerContainer.GetChildCount() > 0)
			{
				Control control = multiplayerPlayerContainer.FirstPlayerState?.Hitbox;
				nRelicInventoryHolder.FocusNeighborBottom = ((control != null && GodotObject.IsInstanceValid(control)) ? control.GetPath() : nRelicInventoryHolder.GetPath());
			}
			else
			{
				nRelicInventoryHolder.FocusNeighborBottom = NRun.Instance.GlobalUi.TopBar.ActiveScreenProxy.GetPath();
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
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectPlayerEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectPlayerEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelicUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.AnimShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetDefaultPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugHideTopBar, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectPlayerEvents && args.Count == 0)
		{
			ConnectPlayerEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectPlayerEvents && args.Count == 0)
		{
			DisconnectPlayerEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelicUnfocused && args.Count == 0)
		{
			OnRelicUnfocused();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimShow && args.Count == 0)
		{
			AnimShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHide && args.Count == 0)
		{
			AnimHide();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowImmediately && args.Count == 0)
		{
			ShowImmediately();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideImmediately && args.Count == 0)
		{
			HideImmediately();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetDefaultPosition && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetDefaultPosition());
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugHideTopBar && args.Count == 0)
		{
			DebugHideTopBar();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnRelicUnfocused && args.Count == 0)
		{
			OnRelicUnfocused();
			ret = default(godot_variant);
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.ConnectPlayerEvents)
		{
			return true;
		}
		if (method == MethodName.DisconnectPlayerEvents)
		{
			return true;
		}
		if (method == MethodName.OnRelicUnfocused)
		{
			return true;
		}
		if (method == MethodName.AnimShow)
		{
			return true;
		}
		if (method == MethodName.AnimHide)
		{
			return true;
		}
		if (method == MethodName.ShowImmediately)
		{
			return true;
		}
		if (method == MethodName.HideImmediately)
		{
			return true;
		}
		if (method == MethodName.GetDefaultPosition)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.DebugHideTopBar)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._originalPos)
		{
			_originalPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			_curTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._debugHideTween)
		{
			_debugHideTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isDebugHidden)
		{
			_isDebugHidden = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._originalPos)
		{
			value = VariantUtils.CreateFrom(in _originalPos);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			value = VariantUtils.CreateFrom(in _curTween);
			return true;
		}
		if (name == PropertyName._debugHideTween)
		{
			value = VariantUtils.CreateFrom(in _debugHideTween);
			return true;
		}
		if (name == PropertyName._isDebugHidden)
		{
			value = VariantUtils.CreateFrom(in _isDebugHidden);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._curTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._debugHideTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isDebugHidden, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._originalPos, Variant.From(in _originalPos));
		info.AddProperty(PropertyName._curTween, Variant.From(in _curTween));
		info.AddProperty(PropertyName._debugHideTween, Variant.From(in _debugHideTween));
		info.AddProperty(PropertyName._isDebugHidden, Variant.From(in _isDebugHidden));
		info.AddSignalEventDelegate(SignalName.RelicsChanged, backing_RelicsChanged);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._originalPos, out var value))
		{
			_originalPos = value.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._curTween, out var value2))
		{
			_curTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._debugHideTween, out var value3))
		{
			_debugHideTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isDebugHidden, out var value4))
		{
			_isDebugHidden = value4.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<RelicsChangedEventHandler>(SignalName.RelicsChanged, out var value5))
		{
			backing_RelicsChanged = value5;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.RelicsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalRelicsChanged()
	{
		EmitSignal(SignalName.RelicsChanged);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.RelicsChanged && args.Count == 0)
		{
			backing_RelicsChanged?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.RelicsChanged)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
