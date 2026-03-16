using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerIntentHandler.cs")]
public class NMultiplayerPlayerIntentHandler : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnHitboxEntered = "OnHitboxEntered";

		public static readonly StringName OnHitboxExited = "OnHitboxExited";

		public static readonly StringName OnHoverChanged = "OnHoverChanged";

		public static readonly StringName RefreshHoverDisplay = "RefreshHoverDisplay";

		public static readonly StringName OnPeerInputStateChanged = "OnPeerInputStateChanged";

		public static readonly StringName OnPeerInputStateRemoved = "OnPeerInputStateRemoved";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName HideThinkyDots = "HideThinkyDots";

		public static readonly StringName RefreshHoverTips = "RefreshHoverTips";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CardIntent = "CardIntent";

		public static readonly StringName _cardIntent = "_cardIntent";

		public static readonly StringName _relicIntent = "_relicIntent";

		public static readonly StringName _potionIntent = "_potionIntent";

		public static readonly StringName _powerIntent = "_powerIntent";

		public static readonly StringName _hitbox = "_hitbox";

		public static readonly StringName _targetingIndicator = "_targetingIndicator";

		public static readonly StringName _cardThinkyDots = "_cardThinkyDots";

		public static readonly StringName _relicThinkyDots = "_relicThinkyDots";

		public static readonly StringName _potionThinkyDots = "_potionThinkyDots";

		public static readonly StringName _powerThinkyDots = "_powerThinkyDots";

		public static readonly StringName _shouldShowHoverTip = "_shouldShowHoverTip";

		public static readonly StringName _hoverTips = "_hoverTips";

		public static readonly StringName _isInPlayerChoice = "_isInPlayerChoice";

		public static readonly StringName _cardInPlayAwaitingPlayerChoice = "_cardInPlayAwaitingPlayerChoice";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "combat/multiplayer_player_intent";

	private NMultiplayerCardIntent _cardIntent;

	private NRelic _relicIntent;

	private NPotion _potionIntent;

	private NPower _powerIntent;

	private Control _hitbox;

	private NRemoteTargetingIndicator _targetingIndicator;

	private MegaRichTextLabel _cardThinkyDots;

	private MegaRichTextLabel _relicThinkyDots;

	private MegaRichTextLabel _potionThinkyDots;

	private MegaRichTextLabel _powerThinkyDots;

	private bool _shouldShowHoverTip;

	private Player _player;

	private AbstractModel? _displayedModel;

	private NHoverTipSet? _hoverTips;

	private bool _isInPlayerChoice;

	private NCard? _cardInPlayAwaitingPlayerChoice;

	private Tween? _tween;

	public NMultiplayerCardIntent CardIntent => _cardIntent;

	public static NMultiplayerPlayerIntentHandler? Create(Player player)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
		{
			return null;
		}
		NMultiplayerPlayerIntentHandler nMultiplayerPlayerIntentHandler = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("combat/multiplayer_player_intent")).Instantiate<NMultiplayerPlayerIntentHandler>(PackedScene.GenEditState.Disabled);
		nMultiplayerPlayerIntentHandler._player = player;
		return nMultiplayerPlayerIntentHandler;
	}

	public override void _Ready()
	{
		_cardIntent = GetNode<NMultiplayerCardIntent>("%CardIntent");
		_relicIntent = GetNode<NRelic>("%RelicIntent");
		_potionIntent = GetNode<NPotion>("%PotionIntent");
		_powerIntent = GetNode<NPower>("%PowerIntent");
		_hitbox = GetNode<Control>("%Hitbox");
		_targetingIndicator = GetNode<NRemoteTargetingIndicator>("%TargetingIndicator");
		_cardThinkyDots = _cardIntent.GetNode<MegaRichTextLabel>("ThinkyDots");
		_relicThinkyDots = _relicIntent.GetNode<MegaRichTextLabel>("ThinkyDots");
		_potionThinkyDots = _potionIntent.GetNode<MegaRichTextLabel>("ThinkyDots");
		_powerThinkyDots = _powerIntent.GetNode<MegaRichTextLabel>("ThinkyDots");
		_targetingIndicator.Initialize(_player);
		_cardIntent.Visible = false;
		_relicIntent.Visible = false;
		_potionIntent.Visible = false;
		_powerIntent.Visible = false;
		HideThinkyDots();
		RunManager.Instance.ActionQueueSet.ActionEnqueued += OnActionEnqueued;
		if (!LocalContext.IsMe(_player))
		{
			_hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnHitboxEntered));
			_hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnHitboxExited));
			_hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnHitboxEntered));
			_hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnHitboxExited));
			RunManager.Instance.HoveredModelTracker.HoverChanged += OnHoverChanged;
			RunManager.Instance.InputSynchronizer.StateChanged += OnPeerInputStateChanged;
			RunManager.Instance.InputSynchronizer.StateRemoved += OnPeerInputStateRemoved;
		}
		base.ProcessMode = ProcessModeEnum.Disabled;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.ActionQueueSet.ActionEnqueued -= OnActionEnqueued;
		if (!LocalContext.IsMe(_player))
		{
			RunManager.Instance.HoveredModelTracker.HoverChanged -= OnHoverChanged;
			RunManager.Instance.InputSynchronizer.StateChanged -= OnPeerInputStateChanged;
			RunManager.Instance.InputSynchronizer.StateRemoved -= OnPeerInputStateRemoved;
		}
	}

	private void OnHitboxEntered()
	{
		_shouldShowHoverTip = true;
		RefreshHoverTips();
	}

	private void OnHitboxExited()
	{
		_shouldShowHoverTip = false;
		RefreshHoverTips();
	}

	private void OnHoverChanged(ulong playerId)
	{
		if (_player.NetId == playerId)
		{
			RefreshHoverDisplay();
		}
	}

	private void RefreshHoverDisplay()
	{
		if (_isInPlayerChoice)
		{
			return;
		}
		_tween?.Kill();
		HideThinkyDots();
		AbstractModel abstractModel = RunManager.Instance.HoveredModelTracker.GetHoveredModel(_player.NetId);
		if (NCombatUi.IsDebugHideMpIntents)
		{
			abstractModel = null;
		}
		if (_displayedModel == abstractModel)
		{
			return;
		}
		base.Modulate = StsColors.halfTransparentWhite;
		_cardIntent.Visible = false;
		_relicIntent.Visible = false;
		_potionIntent.Visible = false;
		_powerIntent.Visible = false;
		_hitbox.Visible = abstractModel != null;
		if (abstractModel != null)
		{
			if (!(abstractModel is CardModel card))
			{
				if (!(abstractModel is PotionModel model))
				{
					if (!(abstractModel is RelicModel model2))
					{
						if (!(abstractModel is PowerModel model3))
						{
							throw new InvalidOperationException($"Player {_player.NetId} hovering unsupported model {abstractModel}");
						}
						_powerIntent.Visible = true;
						_powerIntent.Model = model3;
						_hitbox.Position = _powerIntent.Position;
						_hitbox.Size = _powerIntent.Size;
					}
					else
					{
						_relicIntent.Visible = true;
						_relicIntent.Model = model2;
						_hitbox.Position = _relicIntent.Position;
						_hitbox.Size = _relicIntent.Size;
					}
				}
				else
				{
					_potionIntent.Visible = true;
					_potionIntent.Model = model;
					_hitbox.Position = _potionIntent.Position;
					_hitbox.Size = _potionIntent.Size;
				}
			}
			else
			{
				_cardIntent.Visible = true;
				_cardIntent.Card = card;
				_hitbox.Position = _cardIntent.Position;
				_hitbox.Size = _cardIntent.Size;
			}
		}
		RefreshHoverTips();
		_displayedModel = abstractModel;
	}

	private void OnPeerInputStateChanged(ulong playerId)
	{
		if (playerId == _player.NetId)
		{
			bool isTargeting = RunManager.Instance.InputSynchronizer.GetIsTargeting(_player.NetId);
			if (isTargeting && !_targetingIndicator.Visible)
			{
				_targetingIndicator.StartDrawingFrom(Vector2.Zero);
				base.ProcessMode = ProcessModeEnum.Inherit;
			}
			else if (!isTargeting && _targetingIndicator.Visible)
			{
				_targetingIndicator.StopDrawing();
				base.ProcessMode = ProcessModeEnum.Disabled;
			}
		}
	}

	private void OnPeerInputStateRemoved(ulong playerId)
	{
		if (playerId == _player.NetId && _targetingIndicator.Visible)
		{
			_targetingIndicator.StopDrawing();
			base.ProcessMode = ProcessModeEnum.Disabled;
		}
	}

	public override void _Process(double delta)
	{
		Vector2 cursorPosition = NGame.Instance.RemoteCursorContainer.GetCursorPosition(_player.NetId);
		_targetingIndicator.UpdateDrawingTo(cursorPosition - _targetingIndicator.GlobalPosition);
	}

	private void OnActionEnqueued(GameAction action)
	{
		if (action.OwnerId == _player.NetId)
		{
			action.BeforeExecuted += BeforeActionExecuted;
		}
	}

	private void BeforeActionExecuted(GameAction action)
	{
		action.BeforeExecuted -= BeforeActionExecuted;
		action.BeforePausedForPlayerChoice += BeforeActionPausedForPlayerChoice;
		action.BeforeReadyToResumeAfterPlayerChoice += BeforeActionReadyToResumeAfterPlayerChoice;
		action.AfterFinished += UnsubscribeFromAction;
		action.BeforeCancelled += UnsubscribeFromAction;
	}

	private void UnsubscribeFromAction(GameAction action)
	{
		action.BeforePausedForPlayerChoice -= BeforeActionPausedForPlayerChoice;
		action.BeforeReadyToResumeAfterPlayerChoice -= BeforeActionReadyToResumeAfterPlayerChoice;
		action.AfterFinished -= UnsubscribeFromAction;
		action.BeforeCancelled -= UnsubscribeFromAction;
	}

	private void BeforeActionPausedForPlayerChoice(GameAction action)
	{
		AbstractModel abstractModel = null;
		if (action is PlayCardAction playCardAction)
		{
			abstractModel = playCardAction.PlayerChoiceContext?.LastInvolvedModel;
		}
		else if (action is UsePotionAction usePotionAction)
		{
			abstractModel = usePotionAction.PlayerChoiceContext?.LastInvolvedModel;
		}
		else if (action is GenericHookGameAction genericHookGameAction)
		{
			abstractModel = genericHookGameAction.ChoiceContext?.LastInvolvedModel;
		}
		if (abstractModel == null)
		{
			return;
		}
		_isInPlayerChoice = true;
		_cardIntent.Visible = false;
		_relicIntent.Visible = false;
		_potionIntent.Visible = false;
		_powerIntent.Visible = false;
		_hitbox.Visible = false;
		_cardInPlayAwaitingPlayerChoice = null;
		if (abstractModel is CardModel card)
		{
			NCard nCard = NCard.FindOnTable(card);
			_cardThinkyDots.Visible = true;
			_cardThinkyDots.ProcessMode = ProcessModeEnum.Always;
			if (nCard != null)
			{
				nCard.PlayPileTween?.FastForwardToCompletion();
				Tween tween = nCard.CreateTween();
				tween.Parallel().TweenProperty(nCard, "position", _cardIntent.GlobalPosition + _cardIntent.Size / 2f, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
				tween.Parallel().TweenProperty(nCard, "scale", Vector2.One * 0.25f, (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.2f : 0.3f);
				_cardInPlayAwaitingPlayerChoice = nCard;
				_cardThinkyDots.Reparent(nCard.GetParent());
				_hitbox.Reparent(nCard.GetParent());
			}
			else
			{
				_cardIntent.Card = card;
				_cardIntent.Visible = true;
			}
			_hitbox.Visible = true;
			_hitbox.GlobalPosition = _cardIntent.GlobalPosition;
			_hitbox.Size = _cardIntent.Size;
		}
		else if (abstractModel is RelicModel model)
		{
			_relicIntent.Model = model;
			_relicIntent.Visible = true;
			_relicThinkyDots.Visible = true;
			_relicThinkyDots.ProcessMode = ProcessModeEnum.Always;
			_hitbox.Visible = true;
			_hitbox.Position = _relicIntent.Position;
			_hitbox.Size = _relicIntent.Size;
		}
		else if (abstractModel is PotionModel model2)
		{
			_potionIntent.Model = model2;
			_potionIntent.Visible = true;
			_potionThinkyDots.Visible = true;
			_potionThinkyDots.ProcessMode = ProcessModeEnum.Always;
			_hitbox.Visible = true;
			_hitbox.Position = _potionIntent.Position;
			_hitbox.Size = _potionIntent.Size;
		}
		else if (abstractModel is PowerModel model3)
		{
			_powerIntent.Model = model3;
			_powerIntent.Visible = true;
			_powerThinkyDots.Visible = true;
			_powerThinkyDots.ProcessMode = ProcessModeEnum.Always;
			_hitbox.Visible = true;
			_hitbox.Position = _powerIntent.Position;
			_hitbox.Size = _powerIntent.Size;
		}
		RefreshHoverTips();
		base.Modulate = StsColors.transparentWhite;
		_tween?.Kill();
		_tween = GetTree().CreateTween();
		_tween.TweenProperty(this, "modulate", Colors.White, 0.25);
	}

	private void BeforeActionReadyToResumeAfterPlayerChoice(GameAction action)
	{
		_tween?.Kill();
		_tween = GetTree().CreateTween();
		_tween.TweenProperty(this, "modulate", StsColors.transparentWhite, 0.15000000596046448);
		_tween.TweenCallback(Callable.From(HideThinkyDots));
		_isInPlayerChoice = false;
		if (_cardInPlayAwaitingPlayerChoice != null)
		{
			_cardThinkyDots.Reparent(_cardIntent);
			_hitbox.Reparent(this);
			NCardPlayQueue.Instance.ReAddCardAfterPlayerChoice(_cardInPlayAwaitingPlayerChoice, action);
			_cardInPlayAwaitingPlayerChoice = null;
			RefreshHoverTips();
		}
	}

	private void HideThinkyDots()
	{
		_cardThinkyDots.Visible = false;
		_relicThinkyDots.Visible = false;
		_potionThinkyDots.Visible = false;
		_powerThinkyDots.Visible = false;
		_cardThinkyDots.ProcessMode = ProcessModeEnum.Disabled;
		_relicThinkyDots.ProcessMode = ProcessModeEnum.Disabled;
		_potionThinkyDots.ProcessMode = ProcessModeEnum.Disabled;
		_powerThinkyDots.ProcessMode = ProcessModeEnum.Disabled;
	}

	private void RefreshHoverTips()
	{
		if (LocalContext.IsMe(_player))
		{
			return;
		}
		if (NCombatUi.IsDebugHideTargetingUi)
		{
			_shouldShowHoverTip = false;
		}
		else if (!_hitbox.Visible)
		{
			_shouldShowHoverTip = false;
		}
		NHoverTipSet.Remove(this);
		if (_shouldShowHoverTip)
		{
			List<IHoverTip> list = new List<IHoverTip>();
			if (_cardInPlayAwaitingPlayerChoice != null)
			{
				list.Add(HoverTipFactory.FromCard(_cardInPlayAwaitingPlayerChoice.Model));
				list.AddRange(_cardInPlayAwaitingPlayerChoice.Model.HoverTips);
			}
			else if (_cardIntent.Visible)
			{
				list.Add(HoverTipFactory.FromCard(_cardIntent.Card));
				list.AddRange(_cardIntent.Card.HoverTips);
			}
			else if (_relicIntent.Visible)
			{
				list.AddRange(_relicIntent.Model.HoverTips);
			}
			else if (_potionIntent.Visible)
			{
				list.Add(HoverTipFactory.FromPotion(_potionIntent.Model));
				list.AddRange(_potionIntent.Model.HoverTips);
			}
			if (list.Count > 0)
			{
				NHoverTipSet.CreateAndShow(this, list, HoverTipAlignment.Right);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHitboxEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHitboxExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHoverChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshHoverDisplay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPeerInputStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPeerInputStateRemoved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideThinkyDots, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnHitboxEntered && args.Count == 0)
		{
			OnHitboxEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHitboxExited && args.Count == 0)
		{
			OnHitboxExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHoverChanged && args.Count == 1)
		{
			OnHoverChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshHoverDisplay && args.Count == 0)
		{
			RefreshHoverDisplay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPeerInputStateChanged && args.Count == 1)
		{
			OnPeerInputStateChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPeerInputStateRemoved && args.Count == 1)
		{
			OnPeerInputStateRemoved(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideThinkyDots && args.Count == 0)
		{
			HideThinkyDots();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshHoverTips && args.Count == 0)
		{
			RefreshHoverTips();
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
		if (method == MethodName.OnHitboxEntered)
		{
			return true;
		}
		if (method == MethodName.OnHitboxExited)
		{
			return true;
		}
		if (method == MethodName.OnHoverChanged)
		{
			return true;
		}
		if (method == MethodName.RefreshHoverDisplay)
		{
			return true;
		}
		if (method == MethodName.OnPeerInputStateChanged)
		{
			return true;
		}
		if (method == MethodName.OnPeerInputStateRemoved)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.HideThinkyDots)
		{
			return true;
		}
		if (method == MethodName.RefreshHoverTips)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._cardIntent)
		{
			_cardIntent = VariantUtils.ConvertTo<NMultiplayerCardIntent>(in value);
			return true;
		}
		if (name == PropertyName._relicIntent)
		{
			_relicIntent = VariantUtils.ConvertTo<NRelic>(in value);
			return true;
		}
		if (name == PropertyName._potionIntent)
		{
			_potionIntent = VariantUtils.ConvertTo<NPotion>(in value);
			return true;
		}
		if (name == PropertyName._powerIntent)
		{
			_powerIntent = VariantUtils.ConvertTo<NPower>(in value);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			_hitbox = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._targetingIndicator)
		{
			_targetingIndicator = VariantUtils.ConvertTo<NRemoteTargetingIndicator>(in value);
			return true;
		}
		if (name == PropertyName._cardThinkyDots)
		{
			_cardThinkyDots = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicThinkyDots)
		{
			_relicThinkyDots = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._potionThinkyDots)
		{
			_potionThinkyDots = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._powerThinkyDots)
		{
			_powerThinkyDots = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._shouldShowHoverTip)
		{
			_shouldShowHoverTip = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hoverTips)
		{
			_hoverTips = VariantUtils.ConvertTo<NHoverTipSet>(in value);
			return true;
		}
		if (name == PropertyName._isInPlayerChoice)
		{
			_isInPlayerChoice = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cardInPlayAwaitingPlayerChoice)
		{
			_cardInPlayAwaitingPlayerChoice = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CardIntent)
		{
			value = VariantUtils.CreateFrom<NMultiplayerCardIntent>(CardIntent);
			return true;
		}
		if (name == PropertyName._cardIntent)
		{
			value = VariantUtils.CreateFrom(in _cardIntent);
			return true;
		}
		if (name == PropertyName._relicIntent)
		{
			value = VariantUtils.CreateFrom(in _relicIntent);
			return true;
		}
		if (name == PropertyName._potionIntent)
		{
			value = VariantUtils.CreateFrom(in _potionIntent);
			return true;
		}
		if (name == PropertyName._powerIntent)
		{
			value = VariantUtils.CreateFrom(in _powerIntent);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			value = VariantUtils.CreateFrom(in _hitbox);
			return true;
		}
		if (name == PropertyName._targetingIndicator)
		{
			value = VariantUtils.CreateFrom(in _targetingIndicator);
			return true;
		}
		if (name == PropertyName._cardThinkyDots)
		{
			value = VariantUtils.CreateFrom(in _cardThinkyDots);
			return true;
		}
		if (name == PropertyName._relicThinkyDots)
		{
			value = VariantUtils.CreateFrom(in _relicThinkyDots);
			return true;
		}
		if (name == PropertyName._potionThinkyDots)
		{
			value = VariantUtils.CreateFrom(in _potionThinkyDots);
			return true;
		}
		if (name == PropertyName._powerThinkyDots)
		{
			value = VariantUtils.CreateFrom(in _powerThinkyDots);
			return true;
		}
		if (name == PropertyName._shouldShowHoverTip)
		{
			value = VariantUtils.CreateFrom(in _shouldShowHoverTip);
			return true;
		}
		if (name == PropertyName._hoverTips)
		{
			value = VariantUtils.CreateFrom(in _hoverTips);
			return true;
		}
		if (name == PropertyName._isInPlayerChoice)
		{
			value = VariantUtils.CreateFrom(in _isInPlayerChoice);
			return true;
		}
		if (name == PropertyName._cardInPlayAwaitingPlayerChoice)
		{
			value = VariantUtils.CreateFrom(in _cardInPlayAwaitingPlayerChoice);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardIntent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicIntent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionIntent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerIntent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetingIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardThinkyDots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicThinkyDots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionThinkyDots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerThinkyDots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._shouldShowHoverTip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTips, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isInPlayerChoice, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardInPlayAwaitingPlayerChoice, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardIntent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cardIntent, Variant.From(in _cardIntent));
		info.AddProperty(PropertyName._relicIntent, Variant.From(in _relicIntent));
		info.AddProperty(PropertyName._potionIntent, Variant.From(in _potionIntent));
		info.AddProperty(PropertyName._powerIntent, Variant.From(in _powerIntent));
		info.AddProperty(PropertyName._hitbox, Variant.From(in _hitbox));
		info.AddProperty(PropertyName._targetingIndicator, Variant.From(in _targetingIndicator));
		info.AddProperty(PropertyName._cardThinkyDots, Variant.From(in _cardThinkyDots));
		info.AddProperty(PropertyName._relicThinkyDots, Variant.From(in _relicThinkyDots));
		info.AddProperty(PropertyName._potionThinkyDots, Variant.From(in _potionThinkyDots));
		info.AddProperty(PropertyName._powerThinkyDots, Variant.From(in _powerThinkyDots));
		info.AddProperty(PropertyName._shouldShowHoverTip, Variant.From(in _shouldShowHoverTip));
		info.AddProperty(PropertyName._hoverTips, Variant.From(in _hoverTips));
		info.AddProperty(PropertyName._isInPlayerChoice, Variant.From(in _isInPlayerChoice));
		info.AddProperty(PropertyName._cardInPlayAwaitingPlayerChoice, Variant.From(in _cardInPlayAwaitingPlayerChoice));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cardIntent, out var value))
		{
			_cardIntent = value.As<NMultiplayerCardIntent>();
		}
		if (info.TryGetProperty(PropertyName._relicIntent, out var value2))
		{
			_relicIntent = value2.As<NRelic>();
		}
		if (info.TryGetProperty(PropertyName._potionIntent, out var value3))
		{
			_potionIntent = value3.As<NPotion>();
		}
		if (info.TryGetProperty(PropertyName._powerIntent, out var value4))
		{
			_powerIntent = value4.As<NPower>();
		}
		if (info.TryGetProperty(PropertyName._hitbox, out var value5))
		{
			_hitbox = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._targetingIndicator, out var value6))
		{
			_targetingIndicator = value6.As<NRemoteTargetingIndicator>();
		}
		if (info.TryGetProperty(PropertyName._cardThinkyDots, out var value7))
		{
			_cardThinkyDots = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicThinkyDots, out var value8))
		{
			_relicThinkyDots = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._potionThinkyDots, out var value9))
		{
			_potionThinkyDots = value9.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._powerThinkyDots, out var value10))
		{
			_powerThinkyDots = value10.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._shouldShowHoverTip, out var value11))
		{
			_shouldShowHoverTip = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hoverTips, out var value12))
		{
			_hoverTips = value12.As<NHoverTipSet>();
		}
		if (info.TryGetProperty(PropertyName._isInPlayerChoice, out var value13))
		{
			_isInPlayerChoice = value13.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardInPlayAwaitingPlayerChoice, out var value14))
		{
			_cardInPlayAwaitingPlayerChoice = value14.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value15))
		{
			_tween = value15.As<Tween>();
		}
	}
}
