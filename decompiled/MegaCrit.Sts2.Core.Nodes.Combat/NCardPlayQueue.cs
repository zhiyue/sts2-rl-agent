using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Actions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

/// <summary>
/// Takes control of card nodes that are about to be played and arranges them in the order of play.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Combat/NCardPlayQueue.cs")]
public class NCardPlayQueue : Control
{
	private class QueueItem
	{
		public required NCard card;

		public required GameAction action;

		public Tween? currentTween;
	}

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'RemoveCardFromQueueForCancellation' method.
		/// </summary>
		public static readonly StringName RemoveCardFromQueueForCancellation = "RemoveCardFromQueueForCancellation";

		/// <summary>
		/// Cached name for the 'RemoveCardFromQueue' method.
		/// </summary>
		public static readonly StringName RemoveCardFromQueue = "RemoveCardFromQueue";

		/// <summary>
		/// Cached name for the 'TweenAllToQueuePosition' method.
		/// </summary>
		public static readonly StringName TweenAllToQueuePosition = "TweenAllToQueuePosition";

		/// <summary>
		/// Cached name for the 'AnimOut' method.
		/// </summary>
		public static readonly StringName AnimOut = "AnimOut";

		/// <summary>
		/// Cached name for the 'GetScaleForQueueIndex' method.
		/// </summary>
		public static readonly StringName GetScaleForQueueIndex = "GetScaleForQueueIndex";

		/// <summary>
		/// Cached name for the 'GetPositionForQueueIndex' method.
		/// </summary>
		public static readonly StringName GetPositionForQueueIndex = "GetPositionForQueueIndex";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private List<QueueItem> _playQueue = new List<QueueItem>();

	public static NCardPlayQueue? Instance => NCombatRoom.Instance?.Ui.PlayQueue;

	public override void _Ready()
	{
		RunManager.Instance.ActionQueueSet.ActionEnqueued += OnActionEnqueued;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.ActionQueueSet.ActionEnqueued -= OnActionEnqueued;
		_playQueue.Clear();
	}

	/// <summary>
	/// Called after a card is played by the local player.
	/// If the card is not yet in the play pile, it tweens the card into its queue position.
	/// </summary>
	/// <param name="action">The play card action that caused the card to be played.</param>
	/// <param name="holder">The card holder that was played.</param>
	/// <param name="card">The card that was played.</param>
	public void OnLocalCardPlayed(PlayCardAction action, NCardHolder? holder, CardModel card)
	{
		NCard nCard = holder?.CardNode ?? NCard.Create(card);
		CardModel? model = nCard.Model;
		if (model != null && model.Pile?.Type == PileType.Hand)
		{
			QueueItem item = new QueueItem
			{
				card = nCard,
				action = action
			};
			if (nCard.IsInsideTree())
			{
				nCard.Reparent(this);
			}
			else
			{
				this.AddChildSafely(nCard);
			}
			this.MoveChildSafely(nCard, 0);
			if (holder != null && holder.IsValid())
			{
				NPlayerHand.Instance.RemoveCardHolder(holder);
			}
			_playQueue.Add(item);
			TweenCardToQueuePosition(item, _playQueue.Count - 1);
		}
	}

	/// <summary>
	/// Called when any action is enqueued onto the action queue.
	/// Handles cases in which a card is played by a remote player and we want to display the card in the queue.
	/// </summary>
	/// <param name="action">The action that was enqueued.</param>
	private void OnActionEnqueued(GameAction action)
	{
		if (!(action is PlayCardAction { NetCombatCard: var netCombatCard } playCardAction))
		{
			return;
		}
		CardModel cardModel = netCombatCard.ToCardModelOrNull();
		if (cardModel == null)
		{
			try
			{
				cardModel = ModelDb.GetById<CardModel>(playCardAction.CardModelId);
			}
			catch (ModelNotFoundException)
			{
				cardModel = ModelDb.Card<DeprecatedCard>();
			}
		}
		if (LocalContext.IsMe(playCardAction.Player))
		{
			NCardHolder cardHolder = NPlayerHand.Instance.GetCardHolder(cardModel);
			OnLocalCardPlayed(playCardAction, cardHolder, cardModel);
			return;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(playCardAction.Player.Creature);
		NMultiplayerPlayerIntentHandler playerIntentHandler = creatureNode.PlayerIntentHandler;
		NCard nCard = NCard.Create(cardModel);
		Vector2 globalPosition = playerIntentHandler.CardIntent.GlobalPosition + playerIntentHandler.CardIntent.Size * 0.5f;
		nCard.GlobalPosition = globalPosition;
		nCard.Scale = Vector2.One * 0.25f;
		this.AddChildSafely(nCard);
		this.MoveChildSafely(nCard, 0);
		QueueItem item = new QueueItem
		{
			card = nCard,
			action = playCardAction
		};
		_playQueue.Add(item);
		UpdateCardVisuals(item);
		TweenCardToQueuePosition(item, _playQueue.Count - 1);
	}

	/// <summary>
	/// After remote player choice is complete, this re-adds the card back into the queue if necessary.
	/// Note that this is not called for local player choice - that just sits in the center of the screen.
	/// </summary>
	/// <param name="card">The card that finished player choice.</param>
	/// <param name="action">The action that ran the player choice.</param>
	public void ReAddCardAfterPlayerChoice(NCard card, GameAction action)
	{
		if (action.State == GameActionState.Executing)
		{
			card.Reparent(NCombatRoom.Instance.Ui.PlayContainer);
			card.AnimCardToPlayPile();
			return;
		}
		QueueItem item = new QueueItem
		{
			card = card,
			action = action
		};
		card.Reparent(this);
		this.MoveChildSafely(card, 0);
		_playQueue.Add(item);
		TweenCardToQueuePosition(item, _playQueue.Count - 1);
		action.BeforeResumedAfterPlayerChoice += BeforeRemoteCardPlayResumedAfterPlayerChoice;
	}

	/// <summary>
	/// This is a bit of a hack. When a card resumes execution after player choice, there's nothing to tell the card
	/// that it needs to animate back to the play pile. We do it here, even though we're not really responsible for
	/// the play pile.
	/// </summary>
	/// <param name="action">Action that finished player choice.</param>
	private void BeforeRemoteCardPlayResumedAfterPlayerChoice(GameAction action)
	{
		action.BeforeResumedAfterPlayerChoice -= BeforeRemoteCardPlayResumedAfterPlayerChoice;
		int num = _playQueue.FindIndex((QueueItem i) => i.action == action);
		if (num >= 0)
		{
			QueueItem queueItem = _playQueue[num];
			RemoveCardFromQueue(num);
			queueItem.card.Reparent(NCombatRoom.Instance.Ui.PlayContainer);
			queueItem.card.AnimCardToPlayPile();
		}
	}

	/// <summary>
	/// Called when a card play is canceled.
	/// When a card play is canceled (e.g. because it's now targeting something invalid), this removes the card from the
	/// queue. If the card was a local card, then it is returned to the hand.
	/// </summary>
	/// <param name="action">The play card action that was canceled.</param>
	public void RemoveCardFromQueueForCancellation(PlayCardAction action)
	{
		int num = _playQueue.FindIndex((QueueItem i) => i.action == action);
		if (num >= 0)
		{
			RemoveCardFromQueueForCancellation(num);
		}
	}

	/// <summary>
	/// Called when a card play is canceled.
	/// When a card play is canceled (e.g. because it's now targeting something invalid), this removes the card from the
	/// queue. If the card was a local card, then it is returned to the hand.
	/// </summary>
	/// <param name="card">The card node that was cancelled.</param>
	/// <param name="forceReturnToHand">In rare circumstances, we want to force the card to return to the hand even
	/// though it's not in the hand pile. If true is passed, the card holder will be added to the player hand instead of
	/// fading away (e.g. as if autoplayed). Only use this if you know what you're doing.</param>
	public void RemoveCardFromQueueForCancellation(NCard card, bool forceReturnToHand = false)
	{
		int num = _playQueue.FindIndex((QueueItem i) => i.card == card);
		if (num >= 0)
		{
			RemoveCardFromQueueForCancellation(num, forceReturnToHand);
		}
	}

	private void RemoveCardFromQueueForCancellation(int index, bool forceReturnToHand = false)
	{
		QueueItem queueItem = _playQueue[index];
		RemoveCardFromQueue(index);
		if (queueItem.action.OwnerId == LocalContext.NetId)
		{
			CardModel? model = queueItem.card.Model;
			if ((model != null && model.Pile?.Type == PileType.Hand) || forceReturnToHand)
			{
				NPlayerHand.Instance.Add(queueItem.card);
			}
			else
			{
				TweenCardForCancellation(queueItem);
			}
		}
		else
		{
			TweenCardForCancellation(queueItem);
		}
	}

	/// <summary>
	/// Called just before a card begins execution.
	/// For the play pile to properly take control of cards that are in the queue, the queue must reference the exact
	/// card that is getting played. In most instances, this is true when the action is enqueued. However, if the card was
	/// dynamically created in combat but not yet created on our peer at enqueue time, then it was created with a
	/// placeholder card. By this time, it should be available.
	/// </summary>
	/// <param name="playCardAction">The action that is about to be executed.</param>
	public void UpdateCardBeforeExecution(PlayCardAction playCardAction)
	{
		int num = _playQueue.FindIndex((QueueItem i) => i.action == playCardAction);
		if (num < 0)
		{
			return;
		}
		QueueItem queueItem = _playQueue[num];
		queueItem.card.Model = playCardAction.NetCombatCard.ToCardModel();
		UpdateCardVisuals(queueItem);
		if (LocalContext.IsMe(queueItem.card.Model.Owner))
		{
			NCardHolder nCardHolder = NPlayerHand.Instance?.GetCardHolder(queueItem.card.Model);
			if (nCardHolder != null)
			{
				NPlayerHand.Instance?.Remove(queueItem.card.Model);
			}
		}
	}

	/// <summary>
	/// Called when a card begins execution.
	/// The play pile (specifically CardPileCmd.Add) takes control of the NCard when it begins executing. This method
	/// relinquishes control of the NCard, but does nothing with the node afterward - it is left where it is, and we
	/// expect CardPileCmd.Add to tween it to the play pile position.
	/// </summary>
	/// <param name="card">The card that was canceled.</param>
	public void RemoveCardFromQueueForExecution(CardModel card)
	{
		int num = _playQueue.FindIndex((QueueItem i) => i.card.Model == card);
		if (num < 0)
		{
			throw new InvalidOperationException();
		}
		RemoveCardFromQueue(num);
	}

	/// <summary>
	/// Updates a card's visuals (i.e. updates target, updates text).
	/// </summary>
	private void UpdateCardVisuals(QueueItem item)
	{
		if (item.action is PlayCardAction playCardAction)
		{
			item.card.SetPreviewTarget(playCardAction.Target);
		}
		item.card.UpdateVisuals(item.card.Model.Pile?.Type ?? PileType.None, CardPreviewMode.Normal);
	}

	/// <summary>
	/// Removes a specific node from the queue and tweens other cards to their new position.
	/// The node is not freed.
	/// </summary>
	private void RemoveCardFromQueue(NCard card)
	{
		int num = _playQueue.FindIndex((QueueItem i) => i.card == card);
		if (num >= 0)
		{
			RemoveCardFromQueue(num);
		}
	}

	/// <summary>
	/// Removes a specific card index from the queue and tweens other cards to their new position.
	/// The node is not freed.
	/// </summary>
	private void RemoveCardFromQueue(int index)
	{
		QueueItem queueItem = _playQueue[index];
		queueItem.currentTween?.Kill();
		_playQueue.RemoveAt(index);
		TweenAllToQueuePosition();
	}

	/// <summary>
	/// Tweens all cards to their position/scale in the queue.
	/// </summary>
	private void TweenAllToQueuePosition()
	{
		for (int i = 0; i < _playQueue.Count; i++)
		{
			TweenCardToQueuePosition(_playQueue[i], i);
		}
	}

	/// <summary>
	/// Returns the NCard representing the card if it is in the queue.
	/// Remember that:
	///  - Cards that are being dragged are in the NPlayerHand
	///  - Cards that are being executed are in NCombatUi.PlayContainer
	/// This only returns cards that are enqueued, but not yet executing.
	/// </summary>
	/// <param name="card">The card whose node we will return.</param>
	/// <returns>The node for the card, if it is in the play queue.</returns>
	public NCard? GetCardNode(CardModel card)
	{
		return _playQueue.FirstOrDefault((QueueItem i) => i.card.Model == card)?.card;
	}

	/// <summary>
	/// Dequeue and hide all cards that are in the queue.
	/// This is called when combat ends so that cards don't just hang out.
	/// </summary>
	public void AnimOut()
	{
		foreach (QueueItem item in _playQueue)
		{
			item.currentTween?.Kill();
			if (item.action.OwnerId == LocalContext.NetId)
			{
				CardModel? model = item.card.Model;
				if (model != null && model.Pile?.Type == PileType.Hand)
				{
					NPlayerHand.Instance.Add(item.card);
					continue;
				}
			}
			TweenCardForCancellation(item);
		}
		_playQueue.Clear();
	}

	private void TweenCardForCancellation(QueueItem item)
	{
		item.currentTween?.Kill();
		item.currentTween = CreateTween().SetParallel();
		item.currentTween.TweenProperty(item.card, "position:y", 30f, 0.5).AsRelative().SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		item.currentTween.TweenProperty(item.card, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		item.currentTween.Chain().TweenCallback(Callable.From(item.card.QueueFreeSafely));
		item.currentTween.Play();
	}

	private void TweenCardToQueuePosition(QueueItem item, int queueIndex)
	{
		item.currentTween?.Kill();
		item.currentTween = CreateTween().SetParallel();
		item.currentTween.TweenProperty(item.card, "position", GetPositionForQueueIndex(item.card, queueIndex), 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		item.currentTween.TweenProperty(item.card, "scale", GetScaleForQueueIndex(queueIndex), 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		item.currentTween.TweenProperty(item.card, "modulate:a", 1f, 0.3499999940395355).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		item.currentTween.Play();
	}

	private Vector2 GetScaleForQueueIndex(int index)
	{
		index++;
		float num = 1f - (float)index / (float)(index + 1);
		return num * Vector2.One * 0.8f;
	}

	private Vector2 GetPositionForQueueIndex(NCard card, int index)
	{
		index++;
		float num = (float)index / (float)(index + 2);
		return PileType.Play.GetTargetPosition(card) + Vector2.Left * 300f * num;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveCardFromQueueForCancellation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "forceReturnToHand", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveCardFromQueue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TweenAllToQueuePosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetScaleForQueueIndex, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetPositionForQueueIndex, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveCardFromQueueForCancellation && args.Count == 2)
		{
			RemoveCardFromQueueForCancellation(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveCardFromQueue && args.Count == 1)
		{
			RemoveCardFromQueue(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenAllToQueuePosition && args.Count == 0)
		{
			TweenAllToQueuePosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetScaleForQueueIndex && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetScaleForQueueIndex(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.GetPositionForQueueIndex && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetPositionForQueueIndex(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.RemoveCardFromQueueForCancellation)
		{
			return true;
		}
		if (method == MethodName.RemoveCardFromQueue)
		{
			return true;
		}
		if (method == MethodName.TweenAllToQueuePosition)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.GetScaleForQueueIndex)
		{
			return true;
		}
		if (method == MethodName.GetPositionForQueueIndex)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
