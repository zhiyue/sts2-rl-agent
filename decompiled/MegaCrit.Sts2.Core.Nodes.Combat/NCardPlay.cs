using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCardPlay.cs")]
public abstract class NCardPlay : Node
{
	[Signal]
	public delegate void FinishedEventHandler(bool success);

	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Start = "Start";

		public static readonly StringName CancelPlayCard = "CancelPlayCard";

		public static readonly StringName OnCancelPlayCard = "OnCancelPlayCard";

		public static readonly StringName Cleanup = "Cleanup";

		public static readonly StringName OnCreatureHover = "OnCreatureHover";

		public static readonly StringName OnCreatureUnhover = "OnCreatureUnhover";

		public static readonly StringName CenterCard = "CenterCard";

		public static readonly StringName HideTargetingVisuals = "HideTargetingVisuals";

		public static readonly StringName ShowMultiCreatureTargetingVisuals = "ShowMultiCreatureTargetingVisuals";

		public static readonly StringName AutoDisableCannotPlayCardFtueCheck = "AutoDisableCannotPlayCardFtueCheck";

		public static readonly StringName TryShowEvokingOrbs = "TryShowEvokingOrbs";

		public static readonly StringName HideEvokingOrbs = "HideEvokingOrbs";

		public static readonly StringName ClearTarget = "ClearTarget";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName Holder = "Holder";

		public static readonly StringName CardNode = "CardNode";

		public static readonly StringName CardOwnerNode = "CardOwnerNode";

		public static readonly StringName _viewport = "_viewport";

		public static readonly StringName _isTryingToPlayCard = "_isTryingToPlayCard";
	}

	public new class SignalName : Node.SignalName
	{
		public static readonly StringName Finished = "Finished";
	}

	private static int _totalCardsPlayedForFtue;

	private const int _numCardPlayedUntilDisableFtue = 8;

	protected Viewport _viewport;

	private bool _isTryingToPlayCard;

	private FinishedEventHandler backing_Finished;

	public NHandCardHolder Holder { get; protected set; }

	protected NCard? CardNode => Holder.CardNode;

	protected CardModel? Card => CardNode?.Model;

	protected NCreature? CardOwnerNode => NCombatRoom.Instance.GetCreatureNode(Card?.Owner.Creature);

	public Player Player { get; protected set; }

	public event FinishedEventHandler Finished
	{
		add
		{
			backing_Finished = (FinishedEventHandler)Delegate.Combine(backing_Finished, value);
		}
		remove
		{
			backing_Finished = (FinishedEventHandler)Delegate.Remove(backing_Finished, value);
		}
	}

	public override void _Ready()
	{
		_viewport = GetViewport();
	}

	public abstract void Start();

	protected void TryPlayCard(Creature? target)
	{
		if (Card == null)
		{
			return;
		}
		if (Card.TargetType == TargetType.AnyEnemy && target == null)
		{
			CancelPlayCard();
			return;
		}
		if (Card.TargetType == TargetType.AnyAlly && target == null)
		{
			CancelPlayCard();
			return;
		}
		if (!Holder.CardModel.CanPlayTargeting(target))
		{
			CannotPlayThisCardFtueCheck(Holder.CardModel);
			CancelPlayCard();
			return;
		}
		CardModel card = Card;
		_isTryingToPlayCard = true;
		TargetType targetType = card.TargetType;
		bool flag = ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false);
		bool flag2 = ((!flag) ? card.TryManualPlay(null) : card.TryManualPlay(target));
		_isTryingToPlayCard = false;
		if (flag2)
		{
			AutoDisableCannotPlayCardFtueCheck();
			targetType = card.TargetType;
			flag = ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false);
			if (flag && Holder.IsInsideTree())
			{
				Vector2 size = GetViewport().GetVisibleRect().Size;
				Holder.SetTargetPosition(new Vector2(size.X / 2f, size.Y - Holder.Size.Y));
			}
			Cleanup();
			EmitSignal(SignalName.Finished, true);
			NCombatRoom.Instance?.Ui.Hand.TryGrabFocus();
		}
		else
		{
			CancelPlayCard();
		}
	}

	public void CancelPlayCard()
	{
		if (!_isTryingToPlayCard)
		{
			ClearTarget();
			Cleanup();
			EmitSignal(SignalName.Finished, false);
			OnCancelPlayCard();
		}
	}

	protected virtual void OnCancelPlayCard()
	{
	}

	protected virtual void Cleanup()
	{
		HideTargetingVisuals();
		HideEvokingOrbs();
		this.QueueFreeSafely();
	}

	protected void OnCreatureHover(NCreature creature)
	{
		CardNode?.SetPreviewTarget(creature.Entity);
	}

	protected void OnCreatureUnhover(NCreature _)
	{
		ClearTarget();
	}

	protected void CenterCard()
	{
		Vector2 size = _viewport.GetVisibleRect().Size;
		Holder.SetTargetPosition(new Vector2(size.X / 2f, size.Y - Holder.Hitbox.Size.Y * 0.75f / 2f));
		Holder.SetTargetScale(Vector2.One * 0.75f);
	}

	protected void CannotPlayThisCardFtueCheck(CardModel card)
	{
		if (!SaveManager.Instance.SeenFtue("cannot_play_card_ftue") && !card.CanPlay(out UnplayableReason reason, out AbstractModel _) && reason == UnplayableReason.EnergyCostTooHigh)
		{
			NModalContainer.Instance.Add(NCannotPlayCardFtue.Create());
			SaveManager.Instance.MarkFtueAsComplete("cannot_play_card_ftue");
		}
	}

	protected void HideTargetingVisuals()
	{
		foreach (NCreature creatureNode in NCombatRoom.Instance.CreatureNodes)
		{
			creatureNode.HideMultiselectReticle();
		}
		CardNode?.SetPreviewTarget(null);
		CardNode?.UpdateVisuals((Card?.Pile?.Type).GetValueOrDefault(), CardPreviewMode.Normal);
	}

	protected void ShowMultiCreatureTargetingVisuals()
	{
		if (CardNode == null || Card?.CombatState == null)
		{
			return;
		}
		TargetType targetType = Card.TargetType;
		if ((uint)(targetType - 3) <= 1u)
		{
			IReadOnlyList<Creature> hittableEnemies = Card.CombatState.HittableEnemies;
			if (hittableEnemies.Count == 1)
			{
				CardNode.SetPreviewTarget(hittableEnemies[0]);
			}
			CardNode.UpdateVisuals((CardNode.Model?.Pile?.Type).GetValueOrDefault(), CardPreviewMode.MultiCreatureTargeting);
			foreach (Creature item in hittableEnemies)
			{
				NCombatRoom.Instance.GetCreatureNode(item)?.ShowMultiselectReticle();
			}
		}
		if (Card.TargetType == TargetType.AllAllies)
		{
			IEnumerable<Creature> enumerable = Card.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive);
			{
				foreach (Creature item2 in enumerable)
				{
					NCombatRoom.Instance.GetCreatureNode(item2)?.ShowMultiselectReticle();
				}
				return;
			}
		}
		if (Card.TargetType == TargetType.Self)
		{
			NCombatRoom.Instance.GetCreatureNode(Card.Owner.Creature)?.ShowMultiselectReticle();
		}
		else if (Card.TargetType == TargetType.Osty)
		{
			NCombatRoom.Instance.GetCreatureNode(Card.Owner.Osty)?.ShowMultiselectReticle();
		}
	}

	private void AutoDisableCannotPlayCardFtueCheck()
	{
		_totalCardsPlayedForFtue++;
		if (_totalCardsPlayedForFtue == 8 && !SaveManager.Instance.SeenFtue("cannot_play_card_ftue"))
		{
			Log.Info("Cannot play cards FTUE was disabled, the player never saw it!!");
			SaveManager.Instance.MarkFtueAsComplete("cannot_play_card_ftue");
		}
	}

	protected void TryShowEvokingOrbs()
	{
		if (Card != null)
		{
			CardOwnerNode?.OrbManager?.UpdateVisuals(Card.OrbEvokeType);
		}
	}

	private void HideEvokingOrbs()
	{
		CardOwnerNode?.OrbManager?.UpdateVisuals(OrbEvokeType.None);
	}

	private void ClearTarget()
	{
		CardNode?.SetPreviewTarget(null);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCreatureHover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCreatureUnhover, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.CenterCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideTargetingVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowMultiCreatureTargetingVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AutoDisableCannotPlayCardFtueCheck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TryShowEvokingOrbs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideEvokingOrbs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Start && args.Count == 0)
		{
			Start();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelPlayCard && args.Count == 0)
		{
			CancelPlayCard();
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
		if (method == MethodName.OnCreatureHover && args.Count == 1)
		{
			OnCreatureHover(VariantUtils.ConvertTo<NCreature>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureUnhover && args.Count == 1)
		{
			OnCreatureUnhover(VariantUtils.ConvertTo<NCreature>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CenterCard && args.Count == 0)
		{
			CenterCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideTargetingVisuals && args.Count == 0)
		{
			HideTargetingVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowMultiCreatureTargetingVisuals && args.Count == 0)
		{
			ShowMultiCreatureTargetingVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AutoDisableCannotPlayCardFtueCheck && args.Count == 0)
		{
			AutoDisableCannotPlayCardFtueCheck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TryShowEvokingOrbs && args.Count == 0)
		{
			TryShowEvokingOrbs();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideEvokingOrbs && args.Count == 0)
		{
			HideEvokingOrbs();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearTarget && args.Count == 0)
		{
			ClearTarget();
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
		if (method == MethodName.Start)
		{
			return true;
		}
		if (method == MethodName.CancelPlayCard)
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
		if (method == MethodName.OnCreatureHover)
		{
			return true;
		}
		if (method == MethodName.OnCreatureUnhover)
		{
			return true;
		}
		if (method == MethodName.CenterCard)
		{
			return true;
		}
		if (method == MethodName.HideTargetingVisuals)
		{
			return true;
		}
		if (method == MethodName.ShowMultiCreatureTargetingVisuals)
		{
			return true;
		}
		if (method == MethodName.AutoDisableCannotPlayCardFtueCheck)
		{
			return true;
		}
		if (method == MethodName.TryShowEvokingOrbs)
		{
			return true;
		}
		if (method == MethodName.HideEvokingOrbs)
		{
			return true;
		}
		if (method == MethodName.ClearTarget)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Holder)
		{
			Holder = VariantUtils.ConvertTo<NHandCardHolder>(in value);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._isTryingToPlayCard)
		{
			_isTryingToPlayCard = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Holder)
		{
			value = VariantUtils.CreateFrom<NHandCardHolder>(Holder);
			return true;
		}
		if (name == PropertyName.CardNode)
		{
			value = VariantUtils.CreateFrom<NCard>(CardNode);
			return true;
		}
		if (name == PropertyName.CardOwnerNode)
		{
			value = VariantUtils.CreateFrom<NCreature>(CardOwnerNode);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._isTryingToPlayCard)
		{
			value = VariantUtils.CreateFrom(in _isTryingToPlayCard);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Holder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardOwnerNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isTryingToPlayCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Holder, Variant.From<NHandCardHolder>(Holder));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._isTryingToPlayCard, Variant.From(in _isTryingToPlayCard));
		info.AddSignalEventDelegate(SignalName.Finished, backing_Finished);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Holder, out var value))
		{
			Holder = value.As<NHandCardHolder>();
		}
		if (info.TryGetProperty(PropertyName._viewport, out var value2))
		{
			_viewport = value2.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._isTryingToPlayCard, out var value3))
		{
			_isTryingToPlayCard = value3.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<FinishedEventHandler>(SignalName.Finished, out var value4))
		{
			backing_Finished = value4;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Finished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "success", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalFinished(bool success)
	{
		EmitSignal(SignalName.Finished, success);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Finished && args.Count == 1)
		{
			backing_Finished?.Invoke(VariantUtils.ConvertTo<bool>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Finished)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
