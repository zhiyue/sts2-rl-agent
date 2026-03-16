using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NTargetManager.cs")]
public class NTargetManager : Node2D
{
	[Signal]
	public delegate void CreatureHoveredEventHandler(NCreature creature);

	[Signal]
	public delegate void CreatureUnhoveredEventHandler(NCreature creature);

	[Signal]
	public delegate void NodeHoveredEventHandler(Node node);

	[Signal]
	public delegate void NodeUnhoveredEventHandler(Node node);

	[Signal]
	public delegate void TargetingBeganEventHandler();

	[Signal]
	public delegate void TargetingEndedEventHandler();

	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Input = "_Input";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName CancelTargeting = "CancelTargeting";

		public static readonly StringName FinishTargeting = "FinishTargeting";

		public static readonly StringName AllowedToTargetNode = "AllowedToTargetNode";

		public static readonly StringName OnNodeHovered = "OnNodeHovered";

		public static readonly StringName OnNodeUnhovered = "OnNodeUnhovered";

		public static readonly StringName OnCreatureHovered = "OnCreatureHovered";

		public static readonly StringName OnCreatureUnhovered = "OnCreatureUnhovered";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName IsInSelection = "IsInSelection";

		public static readonly StringName HoveredNode = "HoveredNode";

		public static readonly StringName LastTargetingFinishedFrame = "LastTargetingFinishedFrame";

		public static readonly StringName _targetingArrow = "_targetingArrow";

		public static readonly StringName _targetMode = "_targetMode";

		public static readonly StringName _validTargetsType = "_validTargetsType";
	}

	public new class SignalName : Node2D.SignalName
	{
		public static readonly StringName CreatureHovered = "CreatureHovered";

		public static readonly StringName CreatureUnhovered = "CreatureUnhovered";

		public static readonly StringName NodeHovered = "NodeHovered";

		public static readonly StringName NodeUnhovered = "NodeUnhovered";

		public static readonly StringName TargetingBegan = "TargetingBegan";

		public static readonly StringName TargetingEnded = "TargetingEnded";
	}

	private NTargetingArrow _targetingArrow;

	private TaskCompletionSource<Node?>? _completionSource;

	private Func<bool>? _exitEarlyCondition;

	private Func<Node, bool>? _nodeFilter;

	private TargetMode _targetMode;

	private TargetType _validTargetsType;

	private CreatureHoveredEventHandler backing_CreatureHovered;

	private CreatureUnhoveredEventHandler backing_CreatureUnhovered;

	private NodeHoveredEventHandler backing_NodeHovered;

	private NodeUnhoveredEventHandler backing_NodeUnhovered;

	private TargetingBeganEventHandler backing_TargetingBegan;

	private TargetingEndedEventHandler backing_TargetingEnded;

	public static NTargetManager Instance => NRun.Instance.GlobalUi.TargetManager;

	public bool IsInSelection => _targetMode != TargetMode.None;

	private Node? HoveredNode { get; set; }

	public long LastTargetingFinishedFrame { get; set; }

	public event CreatureHoveredEventHandler CreatureHovered
	{
		add
		{
			backing_CreatureHovered = (CreatureHoveredEventHandler)Delegate.Combine(backing_CreatureHovered, value);
		}
		remove
		{
			backing_CreatureHovered = (CreatureHoveredEventHandler)Delegate.Remove(backing_CreatureHovered, value);
		}
	}

	public event CreatureUnhoveredEventHandler CreatureUnhovered
	{
		add
		{
			backing_CreatureUnhovered = (CreatureUnhoveredEventHandler)Delegate.Combine(backing_CreatureUnhovered, value);
		}
		remove
		{
			backing_CreatureUnhovered = (CreatureUnhoveredEventHandler)Delegate.Remove(backing_CreatureUnhovered, value);
		}
	}

	public event NodeHoveredEventHandler NodeHovered
	{
		add
		{
			backing_NodeHovered = (NodeHoveredEventHandler)Delegate.Combine(backing_NodeHovered, value);
		}
		remove
		{
			backing_NodeHovered = (NodeHoveredEventHandler)Delegate.Remove(backing_NodeHovered, value);
		}
	}

	public event NodeUnhoveredEventHandler NodeUnhovered
	{
		add
		{
			backing_NodeUnhovered = (NodeUnhoveredEventHandler)Delegate.Combine(backing_NodeUnhovered, value);
		}
		remove
		{
			backing_NodeUnhovered = (NodeUnhoveredEventHandler)Delegate.Remove(backing_NodeUnhovered, value);
		}
	}

	public event TargetingBeganEventHandler TargetingBegan
	{
		add
		{
			backing_TargetingBegan = (TargetingBeganEventHandler)Delegate.Combine(backing_TargetingBegan, value);
		}
		remove
		{
			backing_TargetingBegan = (TargetingBeganEventHandler)Delegate.Remove(backing_TargetingBegan, value);
		}
	}

	public event TargetingEndedEventHandler TargetingEnded
	{
		add
		{
			backing_TargetingEnded = (TargetingEndedEventHandler)Delegate.Combine(backing_TargetingEnded, value);
		}
		remove
		{
			backing_TargetingEnded = (TargetingEndedEventHandler)Delegate.Remove(backing_TargetingEnded, value);
		}
	}

	public override void _Ready()
	{
		_targetingArrow = GetNode<NTargetingArrow>("TargetingArrow");
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(CancelTargeting));
			NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(CancelTargeting));
		}
		CombatManager.Instance.CombatEnded += OnCombatEnded;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(CancelTargeting));
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(CancelTargeting));
		}
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsInSelection)
		{
			return;
		}
		bool flag = false;
		bool cancel = false;
		if (inputEvent is InputEventMouseButton { ButtonIndex: var buttonIndex } inputEventMouseButton)
		{
			switch (buttonIndex)
			{
			case MouseButton.Left:
				if (!inputEventMouseButton.IsReleased())
				{
					break;
				}
				switch (_targetMode)
				{
				case TargetMode.ReleaseMouseToTarget:
					if (HoveredNode != null)
					{
						flag = true;
					}
					else
					{
						_targetMode = TargetMode.ClickMouseToTarget;
					}
					break;
				case TargetMode.ClickMouseToTarget:
					flag = true;
					break;
				}
				break;
			case MouseButton.Right:
				flag = inputEventMouseButton.IsPressed();
				cancel = true;
				break;
			}
		}
		else if (inputEvent.IsActionPressed(MegaInput.select) && HoveredNode != null)
		{
			flag = true;
			cancel = false;
			GetViewport().SetInputAsHandled();
		}
		else if (inputEvent.IsActionPressed(MegaInput.cancel) || inputEvent.IsActionPressed(MegaInput.topPanel))
		{
			flag = true;
			cancel = true;
			GetViewport().SetInputAsHandled();
		}
		if (_exitEarlyCondition != null && _exitEarlyCondition())
		{
			flag = true;
			cancel = true;
		}
		if (flag)
		{
			FinishTargeting(cancel);
		}
	}

	public override void _Process(double delta)
	{
		if (_exitEarlyCondition != null && _exitEarlyCondition())
		{
			FinishTargeting(cancel: true);
		}
		if (HoveredNode is NCreature nCreature)
		{
			Creature entity = nCreature.Entity;
			if (entity != null && !entity.IsHittable && _targetMode == TargetMode.Controller)
			{
				FinishTargeting(cancel: true);
			}
		}
	}

	private void OnCombatEnded(CombatRoom _)
	{
		if (_exitEarlyCondition != null)
		{
			FinishTargeting(cancel: true);
		}
	}

	public void CancelTargeting()
	{
		if (_targetMode != TargetMode.None)
		{
			FinishTargeting(cancel: true);
		}
	}

	private void FinishTargeting(bool cancel)
	{
		NHoverTipSet.shouldBlockHoverTips = false;
		_exitEarlyCondition = null;
		_completionSource.SetResult(cancel ? null : HoveredNode);
		LastTargetingFinishedFrame = GetTree().GetFrame();
		EmitSignal(SignalName.TargetingEnded);
		_targetMode = TargetMode.None;
		_targetingArrow.StopDrawing();
		if (HoveredNode is NCreature nCreature)
		{
			nCreature.HideMultiselectReticle();
		}
		else if (HoveredNode is NRestSiteCharacter nRestSiteCharacter)
		{
			nRestSiteCharacter.Deselect();
		}
		HoveredNode = null;
		RunManager.Instance.InputSynchronizer.SyncLocalIsTargeting(isTargeting: false);
	}

	public async Task<Node?> SelectionFinished()
	{
		return await _completionSource.Task;
	}

	public void StartTargeting(TargetType validTargetsType, Vector2 startPosition, TargetMode startingMode, Func<bool>? exitEarlyCondition, Func<Node, bool>? nodeFilter)
	{
		if (!validTargetsType.IsSingleTarget())
		{
			throw new InvalidOperationException($"Tried to begin targeting with invalid ActionTarget {validTargetsType}!");
		}
		_validTargetsType = validTargetsType;
		_targetingArrow.StartDrawingFrom(startPosition, startingMode == TargetMode.Controller);
		_completionSource = new TaskCompletionSource<Node>();
		_exitEarlyCondition = exitEarlyCondition;
		_nodeFilter = nodeFilter;
		_targetMode = startingMode;
		NHoverTipSet.shouldBlockHoverTips = true;
		EmitSignal(SignalName.TargetingBegan);
		RunManager.Instance.InputSynchronizer.SyncLocalIsTargeting(isTargeting: true);
		foreach (NCreature item in NCombatRoom.Instance?.CreatureNodes ?? Array.Empty<NCreature>())
		{
			item.OnTargetingStarted();
		}
	}

	public void StartTargeting(TargetType validTargetsType, Control control, TargetMode startingMode, Func<bool>? exitEarlyCondition, Func<Node, bool>? nodeFilter)
	{
		if (!validTargetsType.IsSingleTarget())
		{
			throw new InvalidOperationException($"Tried to begin targeting with invalid ActionTarget {validTargetsType}!");
		}
		_validTargetsType = validTargetsType;
		_targetingArrow.StartDrawingFrom(control, startingMode == TargetMode.Controller);
		_completionSource = new TaskCompletionSource<Node>();
		_exitEarlyCondition = exitEarlyCondition;
		_nodeFilter = nodeFilter;
		_targetMode = startingMode;
		NHoverTipSet.shouldBlockHoverTips = true;
		EmitSignal(SignalName.TargetingBegan);
		RunManager.Instance.InputSynchronizer.SyncLocalIsTargeting(isTargeting: true);
		foreach (NCreature item in NCombatRoom.Instance?.CreatureNodes ?? Array.Empty<NCreature>())
		{
			item.OnTargetingStarted();
		}
	}

	public bool AllowedToTargetNode(Node node)
	{
		if (_nodeFilter != null && !_nodeFilter(node))
		{
			return false;
		}
		if (node is NCreature nCreature)
		{
			return AllowedToTargetCreature(nCreature.Entity);
		}
		if (node is NMultiplayerPlayerState nMultiplayerPlayerState)
		{
			return AllowedToTargetCreature(nMultiplayerPlayerState.Player.Creature);
		}
		return true;
	}

	private bool AllowedToTargetCreature(Creature creature)
	{
		switch (_validTargetsType)
		{
		case TargetType.AnyEnemy:
			if (creature.Side != CombatSide.Enemy)
			{
				return false;
			}
			break;
		case TargetType.AnyPlayer:
			if (!creature.IsPlayer || creature.IsDead)
			{
				return false;
			}
			break;
		case TargetType.AnyAlly:
			if (!creature.IsPlayer || creature.IsDead)
			{
				return false;
			}
			if (LocalContext.IsMe(creature.Player))
			{
				return false;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("_validTargetsType", _validTargetsType, null);
		}
		return true;
	}

	public void OnNodeHovered(Node node)
	{
		if (!IsInSelection || !AllowedToTargetNode(node))
		{
			return;
		}
		if (node is NCreature creature)
		{
			OnCreatureHovered(creature);
			return;
		}
		HoveredNode = node;
		_targetingArrow.SetHighlightingOn(isEnemy: false);
		if (_targetMode == TargetMode.Controller)
		{
			if (!(node is NMultiplayerPlayerState nMultiplayerPlayerState))
			{
				if (!(node is Control control))
				{
					if (node is Node2D node2D)
					{
						_targetingArrow.UpdateDrawingTo(node2D.GlobalPosition);
					}
				}
				else
				{
					_targetingArrow.UpdateDrawingTo(control.GlobalPosition + control.PivotOffset);
				}
			}
			else
			{
				_targetingArrow.UpdateDrawingTo(nMultiplayerPlayerState.GlobalPosition + Vector2.Right * nMultiplayerPlayerState.Hitbox.Size.X + Vector2.Down * nMultiplayerPlayerState.Hitbox.Size.Y / 2f);
			}
		}
		EmitSignal(SignalName.NodeHovered, node);
	}

	public void OnNodeUnhovered(Node node)
	{
		if (IsInSelection && AllowedToTargetNode(node))
		{
			if (node is NCreature creature)
			{
				OnCreatureUnhovered(creature);
				return;
			}
			HoveredNode = null;
			_targetingArrow.SetHighlightingOff();
			EmitSignal(SignalName.NodeUnhovered, node);
		}
	}

	private void OnCreatureHovered(NCreature creature)
	{
		if (Hook.ShouldAllowTargeting(creature.Entity.CombatState, creature.Entity, out AbstractModel preventer))
		{
			HoveredNode = creature;
			_targetingArrow.SetHighlightingOn(creature.Entity.IsEnemy);
			creature.ShowSingleSelectReticle();
			EmitSignal(SignalName.CreatureHovered, creature);
			if (_targetMode == TargetMode.Controller)
			{
				_targetingArrow.UpdateDrawingTo(creature.VfxSpawnPosition);
			}
		}
		else
		{
			TaskHelper.RunSafely(preventer.AfterTargetingBlockedVfx(creature.Entity));
		}
	}

	private void OnCreatureUnhovered(NCreature creature)
	{
		EmitSignal(SignalName.CreatureUnhovered, creature);
		if (HoveredNode == creature)
		{
			HoveredNode = null;
		}
		_targetingArrow.SetHighlightingOff();
		if (_targetMode != TargetMode.None)
		{
			creature.HideSingleSelectReticle();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CancelTargeting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FinishTargeting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "cancel", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AllowedToTargetNode, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnNodeHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnNodeUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCreatureHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCreatureUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelTargeting && args.Count == 0)
		{
			CancelTargeting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FinishTargeting && args.Count == 1)
		{
			FinishTargeting(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AllowedToTargetNode && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(AllowedToTargetNode(VariantUtils.ConvertTo<Node>(in args[0])));
			return true;
		}
		if (method == MethodName.OnNodeHovered && args.Count == 1)
		{
			OnNodeHovered(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnNodeUnhovered && args.Count == 1)
		{
			OnNodeUnhovered(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureHovered && args.Count == 1)
		{
			OnCreatureHovered(VariantUtils.ConvertTo<NCreature>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureUnhovered && args.Count == 1)
		{
			OnCreatureUnhovered(VariantUtils.ConvertTo<NCreature>(in args[0]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.CancelTargeting)
		{
			return true;
		}
		if (method == MethodName.FinishTargeting)
		{
			return true;
		}
		if (method == MethodName.AllowedToTargetNode)
		{
			return true;
		}
		if (method == MethodName.OnNodeHovered)
		{
			return true;
		}
		if (method == MethodName.OnNodeUnhovered)
		{
			return true;
		}
		if (method == MethodName.OnCreatureHovered)
		{
			return true;
		}
		if (method == MethodName.OnCreatureUnhovered)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.HoveredNode)
		{
			HoveredNode = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName.LastTargetingFinishedFrame)
		{
			LastTargetingFinishedFrame = VariantUtils.ConvertTo<long>(in value);
			return true;
		}
		if (name == PropertyName._targetingArrow)
		{
			_targetingArrow = VariantUtils.ConvertTo<NTargetingArrow>(in value);
			return true;
		}
		if (name == PropertyName._targetMode)
		{
			_targetMode = VariantUtils.ConvertTo<TargetMode>(in value);
			return true;
		}
		if (name == PropertyName._validTargetsType)
		{
			_validTargetsType = VariantUtils.ConvertTo<TargetType>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsInSelection)
		{
			value = VariantUtils.CreateFrom<bool>(IsInSelection);
			return true;
		}
		if (name == PropertyName.HoveredNode)
		{
			value = VariantUtils.CreateFrom<Node>(HoveredNode);
			return true;
		}
		if (name == PropertyName.LastTargetingFinishedFrame)
		{
			value = VariantUtils.CreateFrom<long>(LastTargetingFinishedFrame);
			return true;
		}
		if (name == PropertyName._targetingArrow)
		{
			value = VariantUtils.CreateFrom(in _targetingArrow);
			return true;
		}
		if (name == PropertyName._targetMode)
		{
			value = VariantUtils.CreateFrom(in _targetMode);
			return true;
		}
		if (name == PropertyName._validTargetsType)
		{
			value = VariantUtils.CreateFrom(in _validTargetsType);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetingArrow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsInSelection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._targetMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._validTargetsType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.HoveredNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.LastTargetingFinishedFrame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.HoveredNode, Variant.From<Node>(HoveredNode));
		info.AddProperty(PropertyName.LastTargetingFinishedFrame, Variant.From<long>(LastTargetingFinishedFrame));
		info.AddProperty(PropertyName._targetingArrow, Variant.From(in _targetingArrow));
		info.AddProperty(PropertyName._targetMode, Variant.From(in _targetMode));
		info.AddProperty(PropertyName._validTargetsType, Variant.From(in _validTargetsType));
		info.AddSignalEventDelegate(SignalName.CreatureHovered, backing_CreatureHovered);
		info.AddSignalEventDelegate(SignalName.CreatureUnhovered, backing_CreatureUnhovered);
		info.AddSignalEventDelegate(SignalName.NodeHovered, backing_NodeHovered);
		info.AddSignalEventDelegate(SignalName.NodeUnhovered, backing_NodeUnhovered);
		info.AddSignalEventDelegate(SignalName.TargetingBegan, backing_TargetingBegan);
		info.AddSignalEventDelegate(SignalName.TargetingEnded, backing_TargetingEnded);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.HoveredNode, out var value))
		{
			HoveredNode = value.As<Node>();
		}
		if (info.TryGetProperty(PropertyName.LastTargetingFinishedFrame, out var value2))
		{
			LastTargetingFinishedFrame = value2.As<long>();
		}
		if (info.TryGetProperty(PropertyName._targetingArrow, out var value3))
		{
			_targetingArrow = value3.As<NTargetingArrow>();
		}
		if (info.TryGetProperty(PropertyName._targetMode, out var value4))
		{
			_targetMode = value4.As<TargetMode>();
		}
		if (info.TryGetProperty(PropertyName._validTargetsType, out var value5))
		{
			_validTargetsType = value5.As<TargetType>();
		}
		if (info.TryGetSignalEventDelegate<CreatureHoveredEventHandler>(SignalName.CreatureHovered, out var value6))
		{
			backing_CreatureHovered = value6;
		}
		if (info.TryGetSignalEventDelegate<CreatureUnhoveredEventHandler>(SignalName.CreatureUnhovered, out var value7))
		{
			backing_CreatureUnhovered = value7;
		}
		if (info.TryGetSignalEventDelegate<NodeHoveredEventHandler>(SignalName.NodeHovered, out var value8))
		{
			backing_NodeHovered = value8;
		}
		if (info.TryGetSignalEventDelegate<NodeUnhoveredEventHandler>(SignalName.NodeUnhovered, out var value9))
		{
			backing_NodeUnhovered = value9;
		}
		if (info.TryGetSignalEventDelegate<TargetingBeganEventHandler>(SignalName.TargetingBegan, out var value10))
		{
			backing_TargetingBegan = value10;
		}
		if (info.TryGetSignalEventDelegate<TargetingEndedEventHandler>(SignalName.TargetingEnded, out var value11))
		{
			backing_TargetingEnded = value11;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(SignalName.CreatureHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.CreatureUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.NodeHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.NodeUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.TargetingBegan, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.TargetingEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalCreatureHovered(NCreature creature)
	{
		EmitSignal(SignalName.CreatureHovered, creature);
	}

	protected void EmitSignalCreatureUnhovered(NCreature creature)
	{
		EmitSignal(SignalName.CreatureUnhovered, creature);
	}

	protected void EmitSignalNodeHovered(Node node)
	{
		EmitSignal(SignalName.NodeHovered, node);
	}

	protected void EmitSignalNodeUnhovered(Node node)
	{
		EmitSignal(SignalName.NodeUnhovered, node);
	}

	protected void EmitSignalTargetingBegan()
	{
		EmitSignal(SignalName.TargetingBegan);
	}

	protected void EmitSignalTargetingEnded()
	{
		EmitSignal(SignalName.TargetingEnded);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.CreatureHovered && args.Count == 1)
		{
			backing_CreatureHovered?.Invoke(VariantUtils.ConvertTo<NCreature>(in args[0]));
		}
		else if (signal == SignalName.CreatureUnhovered && args.Count == 1)
		{
			backing_CreatureUnhovered?.Invoke(VariantUtils.ConvertTo<NCreature>(in args[0]));
		}
		else if (signal == SignalName.NodeHovered && args.Count == 1)
		{
			backing_NodeHovered?.Invoke(VariantUtils.ConvertTo<Node>(in args[0]));
		}
		else if (signal == SignalName.NodeUnhovered && args.Count == 1)
		{
			backing_NodeUnhovered?.Invoke(VariantUtils.ConvertTo<Node>(in args[0]));
		}
		else if (signal == SignalName.TargetingBegan && args.Count == 0)
		{
			backing_TargetingBegan?.Invoke();
		}
		else if (signal == SignalName.TargetingEnded && args.Count == 0)
		{
			backing_TargetingEnded?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.CreatureHovered)
		{
			return true;
		}
		if (signal == SignalName.CreatureUnhovered)
		{
			return true;
		}
		if (signal == SignalName.NodeHovered)
		{
			return true;
		}
		if (signal == SignalName.NodeUnhovered)
		{
			return true;
		}
		if (signal == SignalName.TargetingBegan)
		{
			return true;
		}
		if (signal == SignalName.TargetingEnded)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
