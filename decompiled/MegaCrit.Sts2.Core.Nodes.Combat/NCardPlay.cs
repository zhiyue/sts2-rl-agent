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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

/// <summary>
/// An abstract base class in charge of managing the logic for playing a card from your hand.
/// Inherited classes manage card play behavior for different types of inputs.
/// Is created and destroyed on demand.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Combat/NCardPlay.cs")]
public abstract class NCardPlay : Node
{
	[Signal]
	public delegate void FinishedEventHandler(bool success);

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Start' method.
		/// </summary>
		public static readonly StringName Start = "Start";

		/// <summary>
		/// Cached name for the 'CancelPlayCard' method.
		/// </summary>
		public static readonly StringName CancelPlayCard = "CancelPlayCard";

		/// <summary>
		/// Cached name for the 'OnCancelPlayCard' method.
		/// </summary>
		public static readonly StringName OnCancelPlayCard = "OnCancelPlayCard";

		/// <summary>
		/// Cached name for the 'Cleanup' method.
		/// </summary>
		public static readonly StringName Cleanup = "Cleanup";

		/// <summary>
		/// Cached name for the 'OnCreatureHover' method.
		/// </summary>
		public static readonly StringName OnCreatureHover = "OnCreatureHover";

		/// <summary>
		/// Cached name for the 'OnCreatureUnhover' method.
		/// </summary>
		public static readonly StringName OnCreatureUnhover = "OnCreatureUnhover";

		/// <summary>
		/// Cached name for the 'CenterCard' method.
		/// </summary>
		public static readonly StringName CenterCard = "CenterCard";

		/// <summary>
		/// Cached name for the 'HideTargetingVisuals' method.
		/// </summary>
		public static readonly StringName HideTargetingVisuals = "HideTargetingVisuals";

		/// <summary>
		/// Cached name for the 'ShowMultiCreatureTargetingVisuals' method.
		/// </summary>
		public static readonly StringName ShowMultiCreatureTargetingVisuals = "ShowMultiCreatureTargetingVisuals";

		/// <summary>
		/// Cached name for the 'AutoDisableCannotPlayCardFtueCheck' method.
		/// </summary>
		public static readonly StringName AutoDisableCannotPlayCardFtueCheck = "AutoDisableCannotPlayCardFtueCheck";

		/// <summary>
		/// Cached name for the 'TryShowEvokingOrbs' method.
		/// </summary>
		public static readonly StringName TryShowEvokingOrbs = "TryShowEvokingOrbs";

		/// <summary>
		/// Cached name for the 'HideEvokingOrbs' method.
		/// </summary>
		public static readonly StringName HideEvokingOrbs = "HideEvokingOrbs";

		/// <summary>
		/// Cached name for the 'ClearTarget' method.
		/// </summary>
		public static readonly StringName ClearTarget = "ClearTarget";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Holder' property.
		/// </summary>
		public static readonly StringName Holder = "Holder";

		/// <summary>
		/// Cached name for the 'CardNode' property.
		/// </summary>
		public static readonly StringName CardNode = "CardNode";

		/// <summary>
		/// Cached name for the 'CardOwnerNode' property.
		/// </summary>
		public static readonly StringName CardOwnerNode = "CardOwnerNode";

		/// <summary>
		/// Cached name for the '_viewport' field.
		/// </summary>
		public static readonly StringName _viewport = "_viewport";

		/// <summary>
		/// Cached name for the '_isTryingToPlayCard' field.
		/// </summary>
		public static readonly StringName _isTryingToPlayCard = "_isTryingToPlayCard";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
		/// <summary>
		/// Cached name for the 'Finished' signal.
		/// </summary>
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

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Combat.NCardPlay.FinishedEventHandler" />
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
		bool flag;
		if ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false)
		{
			if (NCombatRoom.Instance != null)
			{
				NCombatRoom.Instance.LastTargetedCreature = target;
			}
			flag = card.TryManualPlay(target);
		}
		else
		{
			bool flag2 = NCombatRoom.Instance != null;
			bool flag3 = flag2;
			if (flag3)
			{
				targetType = card.TargetType;
				bool flag4 = ((targetType == TargetType.AllEnemies || targetType == TargetType.AllAllies) ? true : false);
				flag3 = flag4;
			}
			if (flag3)
			{
				NCombatRoom.Instance.LastTargetedCreature = null;
			}
			flag = card.TryManualPlay(null);
		}
		_isTryingToPlayCard = false;
		if (flag)
		{
			AutoDisableCannotPlayCardFtueCheck();
			targetType = card.TargetType;
			bool flag4 = ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false);
			if (flag4 && Holder.IsInsideTree())
			{
				Vector2 size = GetViewport().GetVisibleRect().Size;
				Holder.SetTargetPosition(new Vector2(size.X / 2f, size.Y - Holder.Size.Y));
			}
			Cleanup(isFinished: true);
			ActiveScreenContext.Instance.FocusOnDefaultControl();
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
			Cleanup(isFinished: false);
			OnCancelPlayCard();
		}
	}

	protected virtual void OnCancelPlayCard()
	{
	}

	protected virtual void Cleanup(bool isFinished)
	{
		HideTargetingVisuals();
		HideEvokingOrbs();
		if (GodotObject.IsInstanceValid(this))
		{
			EmitSignal(SignalName.Finished, isFinished);
			this.QueueFreeSafely();
		}
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
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isFinished", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.Cleanup && args.Count == 1)
		{
			Cleanup(VariantUtils.ConvertTo<bool>(in args[0]));
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Holder, Variant.From<NHandCardHolder>(Holder));
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._isTryingToPlayCard, Variant.From(in _isTryingToPlayCard));
		info.AddSignalEventDelegate(SignalName.Finished, backing_Finished);
	}

	/// <inheritdoc />
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

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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
