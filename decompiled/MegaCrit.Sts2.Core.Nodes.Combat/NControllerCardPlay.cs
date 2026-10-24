using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NControllerCardPlay.cs")]
public class NControllerCardPlay : NCardPlay
{
	[Signal]
	public delegate void ConfirmedEventHandler();

	[Signal]
	public delegate void CanceledEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NCardPlay.MethodName
	{
		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'Start' method.
		/// </summary>
		public new static readonly StringName Start = "Start";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'DisconnectTargetingSignals' method.
		/// </summary>
		public static readonly StringName DisconnectTargetingSignals = "DisconnectTargetingSignals";

		/// <summary>
		/// Cached name for the 'MultiCreatureTargeting' method.
		/// </summary>
		public static readonly StringName MultiCreatureTargeting = "MultiCreatureTargeting";

		/// <summary>
		/// Cached name for the 'OnCancelPlayCard' method.
		/// </summary>
		public new static readonly StringName OnCancelPlayCard = "OnCancelPlayCard";

		/// <summary>
		/// Cached name for the 'Cleanup' method.
		/// </summary>
		public new static readonly StringName Cleanup = "Cleanup";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NCardPlay.PropertyName
	{
		/// <summary>
		/// Cached name for the '_onCreatureHoverCallable' field.
		/// </summary>
		public static readonly StringName _onCreatureHoverCallable = "_onCreatureHoverCallable";

		/// <summary>
		/// Cached name for the '_onCreatureUnhoverCallable' field.
		/// </summary>
		public static readonly StringName _onCreatureUnhoverCallable = "_onCreatureUnhoverCallable";

		/// <summary>
		/// Cached name for the '_signalsConnected' field.
		/// </summary>
		public static readonly StringName _signalsConnected = "_signalsConnected";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NCardPlay.SignalName
	{
		/// <summary>
		/// Cached name for the 'Confirmed' signal.
		/// </summary>
		public static readonly StringName Confirmed = "Confirmed";

		/// <summary>
		/// Cached name for the 'Canceled' signal.
		/// </summary>
		public static readonly StringName Canceled = "Canceled";
	}

	private Callable _onCreatureHoverCallable;

	private Callable _onCreatureUnhoverCallable;

	private bool _signalsConnected;

	private ConfirmedEventHandler backing_Confirmed;

	private CanceledEventHandler backing_Canceled;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Combat.NControllerCardPlay.ConfirmedEventHandler" />
	public event ConfirmedEventHandler Confirmed
	{
		add
		{
			backing_Confirmed = (ConfirmedEventHandler)Delegate.Combine(backing_Confirmed, value);
		}
		remove
		{
			backing_Confirmed = (ConfirmedEventHandler)Delegate.Remove(backing_Confirmed, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Combat.NControllerCardPlay.CanceledEventHandler" />
	public event CanceledEventHandler Canceled
	{
		add
		{
			backing_Canceled = (CanceledEventHandler)Delegate.Combine(backing_Canceled, value);
		}
		remove
		{
			backing_Canceled = (CanceledEventHandler)Delegate.Remove(backing_Canceled, value);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!(inputEvent is InputEventAction inputEventAction))
		{
			return;
		}
		if (inputEventAction.IsActionPressed(MegaInput.select))
		{
			EmitSignal(SignalName.Confirmed);
			GetViewport()?.SetInputAsHandled();
		}
		if (inputEventAction.IsActionPressed(MegaInput.cancel) || inputEventAction.IsActionPressed(MegaInput.topPanel))
		{
			EmitSignal(SignalName.Canceled);
			if (inputEvent.IsActionPressed(MegaInput.cancel))
			{
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public static NControllerCardPlay Create(NHandCardHolder holder)
	{
		NControllerCardPlay nControllerCardPlay = new NControllerCardPlay();
		nControllerCardPlay.Holder = holder;
		nControllerCardPlay.Player = holder.CardModel.Owner;
		return nControllerCardPlay;
	}

	public override void Start()
	{
		if (base.Card == null || base.CardNode == null)
		{
			return;
		}
		NDebugAudioManager.Instance?.Play("card_select.mp3");
		NHoverTipSet.Remove(base.Holder);
		_onCreatureHoverCallable = Callable.From<NCreature>(base.OnCreatureHover);
		_onCreatureUnhoverCallable = Callable.From<NCreature>(base.OnCreatureUnhover);
		if (!base.Card.CanPlay(out UnplayableReason reason, out AbstractModel preventer))
		{
			CannotPlayThisCardFtueCheck(base.Card);
			CancelPlayCard();
			LocString playerDialogueLine = reason.GetPlayerDialogueLine(preventer);
			if (playerDialogueLine != null)
			{
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(NThoughtBubbleVfx.Create(playerDialogueLine.GetFormattedText(), base.Card.Owner.Creature, 1.0));
			}
			return;
		}
		TryShowEvokingOrbs();
		base.CardNode.CardHighlight.AnimFlash();
		CenterCard();
		TargetType targetType = base.Card.TargetType;
		if ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false)
		{
			TaskHelper.RunSafely(SingleCreatureTargeting(base.Card.TargetType));
		}
		else
		{
			MultiCreatureTargeting();
		}
	}

	private async Task SingleCreatureTargeting(TargetType targetType)
	{
		Creature owner = base.Card.Owner.Creature;
		List<Creature> list = new List<Creature>();
		switch (targetType)
		{
		case TargetType.AnyEnemy:
			list = (from c in owner.CombatState.GetOpponentsOf(owner)
				where c.IsHittable
				select c).ToList();
			break;
		case TargetType.AnyAlly:
			list = base.Card.CombatState.PlayerCreatures.Where((Creature c) => c.IsHittable && c != owner).ToList();
			break;
		}
		if (list.Count == 0)
		{
			CancelPlayCard();
			return;
		}
		List<NCreature> list2 = list.Select((Creature c) => NCombatRoom.Instance.GetCreatureNode(c)).OfType<NCreature>().ToList();
		if (list2.Count == 0)
		{
			CancelPlayCard();
			return;
		}
		NTargetManager instance = NTargetManager.Instance;
		instance.Connect(NTargetManager.SignalName.CreatureHovered, _onCreatureHoverCallable);
		instance.Connect(NTargetManager.SignalName.CreatureUnhovered, _onCreatureUnhoverCallable);
		_signalsConnected = true;
		try
		{
			instance.StartTargeting(targetType, base.CardNode, TargetMode.Controller, () => !GodotObject.IsInstanceValid(this) || !NControllerManager.Instance.IsUsingController, null);
			NCombatRoom.Instance.RestrictControllerNavigation(list2.Select((NCreature n) => n.Hitbox));
			NCreature nCreature = list2.First();
			if (NCombatRoom.Instance.LastTargetedCreature != null && NCombatRoom.Instance.LastTargetedCreature.IsHittable)
			{
				NCreature nCreature2 = list2.FirstOrDefault((NCreature c) => c.Entity == NCombatRoom.Instance.LastTargetedCreature);
				if (nCreature2 != null)
				{
					nCreature = nCreature2;
				}
			}
			nCreature.Hitbox.TryGrabFocus();
			NCreature nCreature3 = (NCreature)(await instance.SelectionFinished());
			NCombatRoom.Instance.EnableControllerNavigation();
			if (GodotObject.IsInstanceValid(this))
			{
				if (nCreature3 != null)
				{
					TryPlayCard(nCreature3.Entity);
				}
				else
				{
					CancelPlayCard();
				}
			}
		}
		finally
		{
			DisconnectTargetingSignals();
		}
	}

	public override void _ExitTree()
	{
		DisconnectTargetingSignals();
	}

	private void DisconnectTargetingSignals()
	{
		if (_signalsConnected)
		{
			_signalsConnected = false;
			if (NRun.Instance != null)
			{
				NTargetManager instance = NTargetManager.Instance;
				instance.Disconnect(NTargetManager.SignalName.CreatureHovered, _onCreatureHoverCallable);
				instance.Disconnect(NTargetManager.SignalName.CreatureUnhovered, _onCreatureUnhoverCallable);
			}
		}
	}

	private void MultiCreatureTargeting()
	{
		NCombatRoom.Instance.RestrictControllerNavigation(Array.Empty<Control>());
		ShowMultiCreatureTargetingVisuals();
		Connect(SignalName.Confirmed, Callable.From(delegate
		{
			NCombatRoom.Instance.EnableControllerNavigation();
			TryPlayCard(null);
		}));
		Connect(SignalName.Canceled, Callable.From(delegate
		{
			NCombatRoom.Instance.EnableControllerNavigation();
			CancelPlayCard();
		}));
	}

	protected override void OnCancelPlayCard()
	{
		base.Holder.TryGrabFocus();
	}

	protected override void Cleanup(bool isFinished)
	{
		base.Cleanup(isFinished);
		NCombatRoom.Instance.Ui.Hand.DefaultFocusedControl.TryGrabFocus();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectTargetingSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MultiCreatureTargeting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isFinished", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NControllerCardPlay>(Create(VariantUtils.ConvertTo<NHandCardHolder>(in args[0])));
			return true;
		}
		if (method == MethodName.Start && args.Count == 0)
		{
			Start();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectTargetingSignals && args.Count == 0)
		{
			DisconnectTargetingSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MultiCreatureTargeting && args.Count == 0)
		{
			MultiCreatureTargeting();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCancelPlayCard && args.Count == 0)
		{
			OnCancelPlayCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cleanup && args.Count == 1)
		{
			Cleanup(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NControllerCardPlay>(Create(VariantUtils.ConvertTo<NHandCardHolder>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.Start)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.DisconnectTargetingSignals)
		{
			return true;
		}
		if (method == MethodName.MultiCreatureTargeting)
		{
			return true;
		}
		if (method == MethodName.OnCancelPlayCard)
		{
			return true;
		}
		if (method == MethodName.Cleanup)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._onCreatureHoverCallable)
		{
			_onCreatureHoverCallable = VariantUtils.ConvertTo<Callable>(in value);
			return true;
		}
		if (name == PropertyName._onCreatureUnhoverCallable)
		{
			_onCreatureUnhoverCallable = VariantUtils.ConvertTo<Callable>(in value);
			return true;
		}
		if (name == PropertyName._signalsConnected)
		{
			_signalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._onCreatureHoverCallable)
		{
			value = VariantUtils.CreateFrom(in _onCreatureHoverCallable);
			return true;
		}
		if (name == PropertyName._onCreatureUnhoverCallable)
		{
			value = VariantUtils.CreateFrom(in _onCreatureUnhoverCallable);
			return true;
		}
		if (name == PropertyName._signalsConnected)
		{
			value = VariantUtils.CreateFrom(in _signalsConnected);
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
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName._onCreatureHoverCallable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName._onCreatureUnhoverCallable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._signalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._onCreatureHoverCallable, Variant.From(in _onCreatureHoverCallable));
		info.AddProperty(PropertyName._onCreatureUnhoverCallable, Variant.From(in _onCreatureUnhoverCallable));
		info.AddProperty(PropertyName._signalsConnected, Variant.From(in _signalsConnected));
		info.AddSignalEventDelegate(SignalName.Confirmed, backing_Confirmed);
		info.AddSignalEventDelegate(SignalName.Canceled, backing_Canceled);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._onCreatureHoverCallable, out var value))
		{
			_onCreatureHoverCallable = value.As<Callable>();
		}
		if (info.TryGetProperty(PropertyName._onCreatureUnhoverCallable, out var value2))
		{
			_onCreatureUnhoverCallable = value2.As<Callable>();
		}
		if (info.TryGetProperty(PropertyName._signalsConnected, out var value3))
		{
			_signalsConnected = value3.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<ConfirmedEventHandler>(SignalName.Confirmed, out var value4))
		{
			backing_Confirmed = value4;
		}
		if (info.TryGetSignalEventDelegate<CanceledEventHandler>(SignalName.Canceled, out var value5))
		{
			backing_Canceled = value5;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Confirmed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(SignalName.Canceled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalConfirmed()
	{
		EmitSignal(SignalName.Confirmed);
	}

	protected void EmitSignalCanceled()
	{
		EmitSignal(SignalName.Canceled);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Confirmed && args.Count == 0)
		{
			backing_Confirmed?.Invoke();
		}
		else if (signal == SignalName.Canceled && args.Count == 0)
		{
			backing_Canceled?.Invoke();
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
		if (signal == SignalName.Confirmed)
		{
			return true;
		}
		if (signal == SignalName.Canceled)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
