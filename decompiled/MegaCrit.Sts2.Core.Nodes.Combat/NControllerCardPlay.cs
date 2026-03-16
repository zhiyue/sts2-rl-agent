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

	public new class MethodName : NCardPlay.MethodName
	{
		public new static readonly StringName _Input = "_Input";

		public static readonly StringName Create = "Create";

		public new static readonly StringName Start = "Start";

		public static readonly StringName MultiCreatureTargeting = "MultiCreatureTargeting";

		public new static readonly StringName OnCancelPlayCard = "OnCancelPlayCard";

		public new static readonly StringName Cleanup = "Cleanup";
	}

	public new class PropertyName : NCardPlay.PropertyName
	{
	}

	public new class SignalName : NCardPlay.SignalName
	{
		public static readonly StringName Confirmed = "Confirmed";

		public static readonly StringName Canceled = "Canceled";
	}

	private ConfirmedEventHandler backing_Confirmed;

	private CanceledEventHandler backing_Canceled;

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
		if (inputEvent is InputEventAction inputEventAction)
		{
			if (inputEventAction.IsActionPressed(MegaInput.select))
			{
				EmitSignal(SignalName.Confirmed);
			}
			if (inputEventAction.IsActionPressed(MegaInput.cancel))
			{
				EmitSignal(SignalName.Canceled);
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
		NTargetManager targetManager = NTargetManager.Instance;
		targetManager.Connect(NTargetManager.SignalName.CreatureHovered, Callable.From<NCreature>(base.OnCreatureHover));
		targetManager.Connect(NTargetManager.SignalName.CreatureUnhovered, Callable.From<NCreature>(base.OnCreatureUnhover));
		targetManager.StartTargeting(targetType, base.CardNode, TargetMode.Controller, () => !GodotObject.IsInstanceValid(this) || !NControllerManager.Instance.IsUsingController, null);
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
		NCombatRoom.Instance.RestrictControllerNavigation(list.Select((Creature c) => NCombatRoom.Instance.GetCreatureNode(c).Hitbox));
		NCombatRoom.Instance.GetCreatureNode(list.First()).Hitbox.TryGrabFocus();
		NCreature nCreature = (NCreature)(await targetManager.SelectionFinished());
		if (GodotObject.IsInstanceValid(this))
		{
			targetManager.Disconnect(NTargetManager.SignalName.CreatureHovered, Callable.From<NCreature>(base.OnCreatureHover));
			targetManager.Disconnect(NTargetManager.SignalName.CreatureUnhovered, Callable.From<NCreature>(base.OnCreatureUnhover));
			if (nCreature != null)
			{
				TryPlayCard(nCreature.Entity);
			}
			else
			{
				CancelPlayCard();
			}
		}
	}

	private void MultiCreatureTargeting()
	{
		NCombatRoom.Instance.RestrictControllerNavigation(Array.Empty<Control>());
		ShowMultiCreatureTargetingVisuals();
		Connect(SignalName.Confirmed, Callable.From(delegate
		{
			TryPlayCard(null);
		}));
		Connect(SignalName.Canceled, Callable.From(base.CancelPlayCard));
	}

	protected override void OnCancelPlayCard()
	{
		base.Holder.TryGrabFocus();
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		NCombatRoom.Instance.EnableControllerNavigation();
		NCombatRoom.Instance.Ui.Hand.DefaultFocusedControl.TryGrabFocus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.MultiCreatureTargeting, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

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
		if (method == MethodName.Cleanup && args.Count == 0)
		{
			Cleanup();
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddSignalEventDelegate(SignalName.Confirmed, backing_Confirmed);
		info.AddSignalEventDelegate(SignalName.Canceled, backing_Canceled);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetSignalEventDelegate<ConfirmedEventHandler>(SignalName.Confirmed, out var value))
		{
			backing_Confirmed = value;
		}
		if (info.TryGetSignalEventDelegate<CanceledEventHandler>(SignalName.Canceled, out var value2))
		{
			backing_Canceled = value2;
		}
	}

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
