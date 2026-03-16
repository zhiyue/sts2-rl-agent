using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerState.cs")]
public class NMultiplayerPlayerState : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnCreatureValueChanged = "OnCreatureValueChanged";

		public static readonly StringName RefreshValues = "RefreshValues";

		public static readonly StringName UpdateHealthBarWidth = "UpdateHealthBarWidth";

		public static readonly StringName UpdateSelectionReticleWidth = "UpdateSelectionReticleWidth";

		public static readonly StringName OnEnergyChanged = "OnEnergyChanged";

		public static readonly StringName OnStarsChanged = "OnStarsChanged";

		public static readonly StringName RefreshCombatValues = "RefreshCombatValues";

		public static readonly StringName OnCreatureHovered = "OnCreatureHovered";

		public static readonly StringName OnCreatureUnhovered = "OnCreatureUnhovered";

		public static readonly StringName FlashPlayerReady = "FlashPlayerReady";

		public static readonly StringName UpdateHighlightedState = "UpdateHighlightedState";

		public static readonly StringName BlockChanged = "BlockChanged";

		public static readonly StringName RefreshConnectedState = "RefreshConnectedState";

		public static readonly StringName OnPlayerVotesCleared = "OnPlayerVotesCleared";

		public static readonly StringName OnPlayerEndTurnPing = "OnPlayerEndTurnPing";

		public static readonly StringName FlashEndTurn = "FlashEndTurn";

		public static readonly StringName SetNextTweenTime = "SetNextTweenTime";

		public static readonly StringName OnPlayerScreenChanged = "OnPlayerScreenChanged";

		public static readonly StringName TweenLocationIconAway = "TweenLocationIconAway";

		public static readonly StringName TweenLocationIconIn = "TweenLocationIconIn";

		public static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName OnRelease = "OnRelease";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Hitbox = "Hitbox";

		public static readonly StringName _healthBar = "_healthBar";

		public static readonly StringName _characterIcon = "_characterIcon";

		public static readonly StringName _nameplateLabel = "_nameplateLabel";

		public static readonly StringName _topContainer = "_topContainer";

		public static readonly StringName _turnEndIndicator = "_turnEndIndicator";

		public static readonly StringName _disconnectedIndicator = "_disconnectedIndicator";

		public static readonly StringName _networkProblemIndicator = "_networkProblemIndicator";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _locationIcon = "_locationIcon";

		public static readonly StringName _locationContainer = "_locationContainer";

		public static readonly StringName _energyContainer = "_energyContainer";

		public static readonly StringName _energyImage = "_energyImage";

		public static readonly StringName _energyCount = "_energyCount";

		public static readonly StringName _starContainer = "_starContainer";

		public static readonly StringName _starCount = "_starCount";

		public static readonly StringName _cardContainer = "_cardContainer";

		public static readonly StringName _cardImage = "_cardImage";

		public static readonly StringName _cardCount = "_cardCount";

		public static readonly StringName _locationIconTween = "_locationIconTween";

		public static readonly StringName _isMouseOver = "_isMouseOver";

		public static readonly StringName _isCreatureHovered = "_isCreatureHovered";

		public static readonly StringName _isHighlighted = "_isHighlighted";

		public static readonly StringName _focusedWhileTargeting = "_focusedWhileTargeting";

		public static readonly StringName _nextTweenTime = "_nextTweenTime";

		public static readonly StringName _currentLocationIcon = "_currentLocationIcon";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const ulong _delayBetweenTweensMsec = 500uL;

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/multiplayer_player_state");

	private static readonly string _cardScenePath = SceneHelper.GetScenePath("screens/run_history_screen/deck_history_entry");

	private const string _darkenedEnergyMatPath = "res://materials/ui/energy_orb_dark.tres";

	private const float _refHpBarWidth = 175f;

	private const float _refHpBarMaxHp = 80f;

	private const float _selectionReticlePadding = 6f;

	private NHealthBar _healthBar;

	private TextureRect _characterIcon;

	private MegaLabel _nameplateLabel;

	private HBoxContainer _topContainer;

	private TextureRect _turnEndIndicator;

	private TextureRect _disconnectedIndicator;

	private NMultiplayerNetworkProblemIndicator _networkProblemIndicator;

	private NSelectionReticle _selectionReticle;

	private TextureRect _locationIcon;

	private Control _locationContainer;

	private Control _energyContainer;

	private TextureRect _energyImage;

	private MegaLabel _energyCount;

	private Control _starContainer;

	private MegaLabel _starCount;

	private Control _cardContainer;

	private NTinyCard _cardImage;

	private MegaLabel _cardCount;

	private Tween? _locationIconTween;

	private bool _isMouseOver;

	private bool _isCreatureHovered;

	private bool _isHighlighted;

	private bool _focusedWhileTargeting;

	private ulong _nextTweenTime;

	private Texture2D? _currentLocationIcon;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _scenePath, _cardScenePath });

	public NButton Hitbox { get; private set; }

	public Player Player { get; private set; }

	public static NMultiplayerPlayerState Create(Player player)
	{
		NMultiplayerPlayerState nMultiplayerPlayerState = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMultiplayerPlayerState>(PackedScene.GenEditState.Disabled);
		nMultiplayerPlayerState.Player = player;
		return nMultiplayerPlayerState;
	}

	public override void _Ready()
	{
		_nameplateLabel = GetNode<MegaLabel>("%NameplateLabel");
		_healthBar = GetNode<NHealthBar>("%HealthBar");
		_characterIcon = GetNode<TextureRect>("%CharacterIcon");
		_turnEndIndicator = GetNode<TextureRect>("%TurnEndIndicator");
		_disconnectedIndicator = GetNode<TextureRect>("%DisconnectedIndicator");
		_networkProblemIndicator = GetNode<NMultiplayerNetworkProblemIndicator>("%NetworkProblemIndicator");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		Hitbox = GetNode<NButton>("%Hitbox");
		_locationIcon = GetNode<TextureRect>("%LocationIcon");
		_locationContainer = GetNode<Control>("%LocationContainer");
		_topContainer = GetNode<HBoxContainer>("TopInfoContainer");
		_energyContainer = GetNode<Control>("%EnergyCountContainer");
		_energyImage = _energyContainer.GetNode<TextureRect>("Image");
		_energyCount = _energyContainer.GetNode<MegaLabel>("EnergyCount");
		_starContainer = GetNode<Control>("%StarCountContainer");
		_starCount = _starContainer.GetNode<MegaLabel>("StarCount");
		_cardContainer = GetNode<Control>("%CardCountContainer");
		_cardImage = _cardContainer.GetNode<NTinyCard>("TinyCard");
		_cardCount = _cardContainer.GetNode<MegaLabel>("CardCount");
		_selectionReticle.Visible = true;
		_characterIcon.Texture = Player.Character.IconTexture;
		_nameplateLabel.SetTextAutoSize(PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Player.NetId));
		_healthBar.SetCreature(Player.Creature);
		_networkProblemIndicator.Initialize(Player.NetId);
		_locationContainer.Visible = false;
		_energyContainer.Visible = false;
		_energyImage.Texture = ResourceLoader.Load<Texture2D>(Player.Character.CardPool.EnergyIconPath, null, ResourceLoader.CacheMode.Reuse);
		_starContainer.Visible = false;
		_cardContainer.Visible = false;
		_cardImage.Set(Player.Character.CardPool, CardType.Attack, CardRarity.Common);
		_turnEndIndicator.Visible = false;
		_healthBar.FadeOutHpLabel(0f, 0f);
		Player.Creature.BlockChanged += BlockChanged;
		Player.Creature.CurrentHpChanged += OnCreatureValueChanged;
		Player.Creature.MaxHpChanged += OnCreatureValueChanged;
		Player.Creature.PowerApplied += OnPowerAppliedOrRemoved;
		Player.Creature.PowerIncreased += OnPowerIncreased;
		Player.Creature.PowerDecreased += OnPowerDecreased;
		Player.Creature.PowerRemoved += OnPowerAppliedOrRemoved;
		Player.Creature.Died += OnCreatureChanged;
		Player.RelicObtained += OnRelicObtained;
		Player.RelicRemoved += OnRelicRemoved;
		Player.PotionProcured += OnPotionProcured;
		Player.PotionDiscarded += OnPotionDiscarded;
		Player.Deck.CardAdded += OnCardObtained;
		Player.Deck.CardRemoved += OnCardRemovedFromDeck;
		CombatManager.Instance.PlayerEndedTurn += RefreshPlayerReadyIndicator;
		CombatManager.Instance.PlayerUnendedTurn += RefreshPlayerReadyIndicator;
		CombatManager.Instance.TurnStarted += OnTurnStarted;
		CombatManager.Instance.CombatSetUp += OnCombatSetUp;
		CombatManager.Instance.CombatEnded += OnCombatEnded;
		RunManager.Instance.FlavorSynchronizer.OnEndTurnPingReceived += OnPlayerEndTurnPing;
		RunManager.Instance.InputSynchronizer.ScreenChanged += OnPlayerScreenChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteChanged += OnPlayerVoteChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteCancelled += RefreshPlayerReadyIndicator;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVotesCleared += OnPlayerVotesCleared;
		if (RunManager.Instance.RunLobby != null)
		{
			RunManager.Instance.RunLobby.RemotePlayerDisconnected += RefreshConnectedState;
			RunManager.Instance.RunLobby.LocalPlayerDisconnected += RefreshConnectedState;
			RunManager.Instance.RunLobby.PlayerRejoined += RefreshConnectedState;
		}
		Hitbox.Connect(NClickableControl.SignalName.Focused, Callable.From<NButton>(OnFocus));
		Hitbox.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NButton>(OnUnfocus));
		Hitbox.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnRelease));
		RefreshValues();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Player.Creature.BlockChanged -= BlockChanged;
		Player.Creature.CurrentHpChanged -= OnCreatureValueChanged;
		Player.Creature.MaxHpChanged -= OnCreatureValueChanged;
		Player.Creature.PowerApplied -= OnPowerAppliedOrRemoved;
		Player.Creature.PowerIncreased -= OnPowerIncreased;
		Player.Creature.PowerDecreased -= OnPowerDecreased;
		Player.Creature.PowerRemoved -= OnPowerAppliedOrRemoved;
		Player.Creature.Died -= OnCreatureChanged;
		Player.RelicObtained -= OnRelicObtained;
		Player.RelicRemoved -= OnRelicRemoved;
		Player.PotionProcured -= OnPotionProcured;
		Player.PotionDiscarded -= OnPotionDiscarded;
		Player.Deck.CardAdded -= OnCardObtained;
		Player.Deck.CardRemoved -= OnCardRemovedFromDeck;
		CombatManager.Instance.PlayerEndedTurn -= RefreshPlayerReadyIndicator;
		CombatManager.Instance.PlayerUnendedTurn -= RefreshPlayerReadyIndicator;
		CombatManager.Instance.TurnStarted -= OnTurnStarted;
		CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
		CombatManager.Instance.CombatEnded -= OnCombatEnded;
		RunManager.Instance.FlavorSynchronizer.OnEndTurnPingReceived -= OnPlayerEndTurnPing;
		RunManager.Instance.InputSynchronizer.ScreenChanged -= OnPlayerScreenChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteChanged -= OnPlayerVoteChanged;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVoteCancelled -= RefreshPlayerReadyIndicator;
		RunManager.Instance.MapSelectionSynchronizer.PlayerVotesCleared -= OnPlayerVotesCleared;
		if (RunManager.Instance.RunLobby != null)
		{
			RunManager.Instance.RunLobby.RemotePlayerDisconnected -= RefreshConnectedState;
			RunManager.Instance.RunLobby.LocalPlayerDisconnected -= RefreshConnectedState;
			RunManager.Instance.RunLobby.PlayerRejoined -= RefreshConnectedState;
		}
	}

	private void OnCombatSetUp(CombatState _)
	{
		if (!LocalContext.IsMe(Player))
		{
			_energyContainer.Visible = true;
			Control starContainer = _starContainer;
			int visible;
			if (!(Player.Character is Regent))
			{
				PlayerCombatState? playerCombatState = Player.PlayerCombatState;
				visible = ((playerCombatState != null && playerCombatState.Stars > 0) ? 1 : 0);
			}
			else
			{
				visible = 1;
			}
			starContainer.Visible = (byte)visible != 0;
			_cardContainer.Visible = true;
			Player.PlayerCombatState.EnergyChanged += OnEnergyChanged;
			Player.PlayerCombatState.StarsChanged += OnStarsChanged;
			Player.PlayerCombatState.Hand.CardAdded += OnCardAdded;
			Player.PlayerCombatState.Hand.CardRemoved += OnCardRemoved;
		}
	}

	private void OnCombatEnded(CombatRoom _)
	{
		_turnEndIndicator.Visible = false;
		if (!LocalContext.IsMe(Player))
		{
			_energyContainer.Visible = false;
			_starContainer.Visible = false;
			_cardContainer.Visible = false;
			if (Player.PlayerCombatState != null)
			{
				Player.PlayerCombatState.EnergyChanged -= OnEnergyChanged;
				Player.PlayerCombatState.StarsChanged -= OnStarsChanged;
				Player.PlayerCombatState.Hand.CardAdded -= OnCardAdded;
				Player.PlayerCombatState.Hand.CardRemoved -= OnCardRemoved;
			}
		}
	}

	private void OnCreatureValueChanged(int _, int __)
	{
		RefreshValues();
	}

	private void OnCreatureChanged(Creature _)
	{
		RefreshValues();
	}

	private void OnPowerAppliedOrRemoved(PowerModel _)
	{
		RefreshValues();
	}

	private void OnPowerDecreased(PowerModel _, bool __)
	{
		RefreshValues();
	}

	private void OnPowerIncreased(PowerModel _, int __, bool ___)
	{
		RefreshValues();
	}

	private void RefreshValues()
	{
		UpdateHealthBarWidth();
		_healthBar.RefreshValues();
	}

	private void UpdateHealthBarWidth()
	{
		_healthBar.UpdateWidthRelativeToReferenceValue(80f, 175f);
	}

	private void UpdateSelectionReticleWidth()
	{
		Control control = null;
		foreach (Control item in _topContainer.GetChildren().OfType<Control>())
		{
			if (item.Visible && item.Size.X > 0f)
			{
				control = item;
			}
		}
		float a = control.GlobalPosition.X - base.GlobalPosition.X + control.Size.X + 6f;
		float b = _healthBar.HpBarContainer.GlobalPosition.X - base.GlobalPosition.X + _healthBar.HpBarContainer.Size.X + 6f;
		float x = Mathf.Max(a, b);
		NSelectionReticle selectionReticle = _selectionReticle;
		StringName size = Control.PropertyName.Size;
		Vector2 size2 = _selectionReticle.Size;
		size2.X = x;
		selectionReticle.SetDeferred(size, size2);
		Hitbox.SetDeferred(Control.PropertyName.Size, new Vector2(x, base.Size.Y));
	}

	private void OnEnergyChanged(int _, int __)
	{
		RefreshCombatValues();
	}

	private void OnStarsChanged(int _, int __)
	{
		RefreshCombatValues();
	}

	private void OnCardAdded(CardModel _)
	{
		RefreshCombatValues();
	}

	private void OnCardRemoved(CardModel _)
	{
		RefreshCombatValues();
	}

	private void RefreshCombatValues()
	{
		Control starContainer = _starContainer;
		int visible;
		if (!(Player.Character is Regent))
		{
			PlayerCombatState? playerCombatState = Player.PlayerCombatState;
			visible = ((playerCombatState != null && playerCombatState.Stars > 0) ? 1 : 0);
		}
		else
		{
			visible = 1;
		}
		starContainer.Visible = (byte)visible != 0;
		_energyCount.SetTextAutoSize(Player.PlayerCombatState.Energy.ToString());
		_starCount.SetTextAutoSize(Player.PlayerCombatState.Stars.ToString());
		_cardCount.SetTextAutoSize(Player.PlayerCombatState.Hand.Cards.Count.ToString());
		_energyCount.AddThemeColorOverride(ThemeConstants.Label.fontColor, (Player.PlayerCombatState.Energy == 0) ? StsColors.red : StsColors.cream);
		_energyCount.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, (Player.PlayerCombatState.Energy == 0) ? StsColors.unplayableEnergyCostOutline : Player.Character.EnergyLabelOutlineColor);
		Material material = ((Player.PlayerCombatState.Energy == 0) ? PreloadManager.Cache.GetMaterial("res://materials/ui/energy_orb_dark.tres") : null);
		_energyImage.Material = material;
		_energyImage.Modulate = ((Player.PlayerCombatState.Energy == 0) ? Colors.DarkGray : Colors.White);
	}

	public void OnCreatureHovered()
	{
		_isCreatureHovered = true;
		UpdateHighlightedState();
	}

	public void OnCreatureUnhovered()
	{
		_isCreatureHovered = false;
		UpdateHighlightedState();
	}

	public void FlashPlayerReady()
	{
		FlashEndTurn();
	}

	private void UpdateHighlightedState()
	{
		bool flag = _isMouseOver || _isCreatureHovered;
		if (NTargetManager.Instance.IsInSelection && !NTargetManager.Instance.AllowedToTargetNode(this))
		{
			flag = false;
		}
		if (!NTargetManager.Instance.IsInSelection)
		{
			NPlayerHand? instance = NPlayerHand.Instance;
			if (instance != null && instance.InCardPlay)
			{
				flag = false;
			}
		}
		if (_isHighlighted == flag)
		{
			return;
		}
		_isHighlighted = flag;
		if (_isHighlighted)
		{
			_healthBar.FadeInHpLabel(0.1f);
			UpdateSelectionReticleWidth();
			_selectionReticle.OnSelect();
			if (_networkProblemIndicator.IsShown)
			{
				LocString locString;
				LocString title;
				if (RunManager.Instance.NetService.Type == NetGameType.Client)
				{
					locString = new LocString("static_hover_tips", "NETWORK_PROBLEM_CLIENT.description");
					title = new LocString("static_hover_tips", "NETWORK_PROBLEM_CLIENT.title");
				}
				else
				{
					locString = new LocString("static_hover_tips", "NETWORK_PROBLEM_HOST.description");
					title = new LocString("static_hover_tips", "NETWORK_PROBLEM_HOST.title");
				}
				locString.Add("Player", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, Player.NetId));
				NHoverTipSet.CreateAndShow(this, new HoverTip(title, locString)).GlobalPosition = base.GlobalPosition + Vector2.Down * 80f;
			}
		}
		else
		{
			_healthBar.FadeOutHpLabel(0.5f, 0f);
			_selectionReticle.OnDeselect();
			NHoverTipSet.Remove(this);
		}
	}

	private void BlockChanged(int oldBlock, int blockGain)
	{
		if (oldBlock == 0 && blockGain > 0)
		{
			_healthBar.AnimateInBlock(oldBlock, blockGain);
		}
		_healthBar.RefreshValues();
	}

	private void RefreshConnectedState(ulong _)
	{
		RefreshConnectedState();
	}

	private void RefreshConnectedState()
	{
		bool flag = RunManager.Instance.RunLobby.ConnectedPlayerIds.Contains(Player.NetId);
		_disconnectedIndicator.Visible = !flag;
		_characterIcon.SelfModulate = (flag ? Colors.White : StsColors.gray);
	}

	private void OnRelicObtained(RelicModel relic)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimateRelicObtained(relic));
		}
	}

	private async Task AnimateRelicObtained(RelicModel relic)
	{
		await WaitUntilNextTweenTime();
		NRelic relicImage = NRelic.Create(relic, NRelic.IconSize.Small);
		relicImage.Model = relic;
		await ObtainedAnimation(relicImage);
		relicImage.QueueFreeSafely();
	}

	private void OnRelicRemoved(RelicModel relic)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimateRelicRemoved(relic));
		}
	}

	private async Task AnimateRelicRemoved(RelicModel relic)
	{
		await WaitUntilNextTweenTime();
		NRelic relicImage = NRelic.Create(relic, NRelic.IconSize.Small);
		relicImage.Model = relic;
		await RemovedAnimation(relicImage);
		relicImage.QueueFreeSafely();
	}

	private void OnCardObtained(CardModel card)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimateCardObtained(card));
		}
	}

	private async Task AnimateCardObtained(CardModel card)
	{
		await WaitUntilNextTweenTime();
		NDeckHistoryEntry cardNode = NDeckHistoryEntry.Create(card, 1);
		await ObtainedAnimation(cardNode);
		cardNode.QueueFreeSafely();
	}

	private void OnCardRemovedFromDeck(CardModel card)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimateCardRemovedFromDeck(card));
		}
	}

	private async Task AnimateCardRemovedFromDeck(CardModel card)
	{
		await WaitUntilNextTweenTime();
		NDeckHistoryEntry cardNode = NDeckHistoryEntry.Create(card, 1);
		await RemovedAnimation(cardNode);
		cardNode.QueueFreeSafely();
	}

	private void OnPotionProcured(PotionModel potion)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimatePotionObtained(potion));
		}
	}

	private async Task AnimatePotionObtained(PotionModel potion)
	{
		await WaitUntilNextTweenTime();
		NPotion node = NPotion.Create(potion);
		await ObtainedAnimation(node);
		node.QueueFreeSafely();
	}

	private void OnPotionDiscarded(PotionModel potion)
	{
		if (!LocalContext.IsMe(Player))
		{
			TaskHelper.RunSafely(AnimatePotionDiscarded(potion));
		}
	}

	private async Task AnimatePotionDiscarded(PotionModel potion)
	{
		await WaitUntilNextTweenTime();
		NPotion node = NPotion.Create(potion);
		await RemovedAnimation(node);
		node.QueueFreeSafely();
	}

	private void OnPlayerVoteChanged(Player player, MapVote? _, MapVote? __)
	{
		RefreshPlayerReadyIndicator(player);
	}

	private void OnPlayerVotesCleared()
	{
		RefreshPlayerReadyIndicator(Player);
	}

	private void RefreshPlayerReadyIndicator(Player player, bool _)
	{
		RefreshPlayerReadyIndicator(player);
	}

	private void RefreshPlayerReadyIndicator(Player player)
	{
		if (CombatManager.Instance.IsInProgress)
		{
			_turnEndIndicator.Visible = CombatManager.Instance.IsPlayerReadyToEndTurn(Player);
		}
		else
		{
			_turnEndIndicator.Visible = RunManager.Instance.MapSelectionSynchronizer.GetVote(Player).HasValue;
		}
		if (_turnEndIndicator.Visible && player == Player)
		{
			FlashEndTurn();
		}
	}

	private void OnPlayerEndTurnPing(ulong playerId)
	{
		if (Player.NetId == playerId)
		{
			FlashEndTurn();
		}
	}

	private void FlashEndTurn()
	{
		NUiFlashVfx nUiFlashVfx = NUiFlashVfx.Create(_turnEndIndicator.Texture, _turnEndIndicator.SelfModulate);
		_turnEndIndicator.AddChildSafely(nUiFlashVfx);
		nUiFlashVfx.SetDeferred(Control.PropertyName.Size, _turnEndIndicator.Size);
		nUiFlashVfx.Position = Vector2.Zero;
		TaskHelper.RunSafely(nUiFlashVfx.StartVfx());
	}

	private void OnTurnStarted(CombatState _)
	{
		_turnEndIndicator.Visible = CombatManager.Instance.IsPlayerReadyToEndTurn(Player);
	}

	private void SetNextTweenTime()
	{
		ulong ticksMsec = Time.GetTicksMsec();
		if (_nextTweenTime > ticksMsec)
		{
			_nextTweenTime += 500uL;
		}
		else
		{
			_nextTweenTime = ticksMsec + 500;
		}
	}

	private async Task WaitUntilNextTweenTime()
	{
		ulong nextTweenTime = _nextTweenTime;
		SetNextTweenTime();
		if (nextTweenTime >= Time.GetTicksMsec())
		{
			double timeSec = (double)(nextTweenTime - Time.GetTicksMsec()) / 1000.0;
			await ToSignal(GetTree().CreateTimer(timeSec), SceneTreeTimer.SignalName.Timeout);
		}
	}

	private async Task ObtainedAnimation(Control node)
	{
		this.AddChildSafely(node);
		node.Position = new Vector2(base.Size.X + 40f, 0f);
		node.Scale = Vector2.One * 1.1f;
		Tween tween = node.CreateTween();
		tween.TweenProperty(node, "scale", Vector2.One, 0.30000001192092896).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Bounce);
		tween.TweenProperty(node, "position:x", base.Size.X - node.Size.X, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.SetDelay(0.30000001192092896);
		tween.TweenProperty(node, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.SetDelay(0.30000001192092896);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private async Task RemovedAnimation(Control node)
	{
		this.AddChildSafely(node);
		node.Position = new Vector2(base.Size.X - node.Size.X, 0f);
		Color modulate = node.Modulate;
		modulate.A = 0f;
		node.Modulate = modulate;
		Tween tween = node.CreateTween();
		tween.TweenProperty(node, "position:x", base.Size.X + 40f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(node, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(node, "scale:y", 0f, 0.30000001192092896).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.SetDelay(0.5);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private void OnPlayerScreenChanged(ulong playerId, NetScreenType _)
	{
		if (Player.NetId != playerId || LocalContext.IsMe(Player))
		{
			return;
		}
		Texture2D locationIcon = RunManager.Instance.InputSynchronizer.GetScreenType(playerId).GetLocationIcon();
		if (_currentLocationIcon != locationIcon)
		{
			_currentLocationIcon = locationIcon;
			if (locationIcon == null)
			{
				TweenLocationIconAway();
			}
			else
			{
				TweenLocationIconIn(locationIcon);
			}
		}
	}

	private void TweenLocationIconAway()
	{
		_locationIconTween?.Kill();
		_locationIconTween = _locationIcon.CreateTween();
		_locationIconTween.TweenProperty(_locationIcon, "scale", Vector2.Zero, 0.4).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		_locationIconTween.TweenCallback(Callable.From(() => _locationContainer.Visible = false));
	}

	private void TweenLocationIconIn(Texture2D? texture)
	{
		_locationIconTween?.Kill();
		_locationIconTween = _locationIcon.CreateTween();
		if (!_locationContainer.Visible)
		{
			_locationContainer.Visible = true;
			_locationIcon.Texture = texture;
			_locationIconTween.TweenProperty(_locationIcon, "scale", Vector2.One, 0.4).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			return;
		}
		_locationIconTween.TweenProperty(_locationIcon, "scale", Vector2.One * 0.5f, 0.2).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		_locationIconTween.TweenCallback(Callable.From(() => _locationIcon.Texture = texture));
		_locationIconTween.TweenProperty(_locationIcon, "scale", Vector2.One, 0.3).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
	}

	protected void OnFocus(NButton _)
	{
		_isMouseOver = true;
		UpdateHighlightedState();
		if (NTargetManager.Instance.IsInSelection && NTargetManager.Instance.AllowedToTargetNode(this))
		{
			NTargetManager.Instance.OnNodeHovered(this);
			_focusedWhileTargeting = true;
		}
	}

	protected void OnUnfocus(NButton _)
	{
		_isMouseOver = false;
		UpdateHighlightedState();
		if (_focusedWhileTargeting)
		{
			NTargetManager.Instance.OnNodeUnhovered(this);
		}
		_focusedWhileTargeting = false;
	}

	protected void OnRelease(NButton _)
	{
		if (!NTargetManager.Instance.IsInSelection && NTargetManager.Instance.LastTargetingFinishedFrame != GetTree().GetFrame())
		{
			NMultiplayerPlayerExpandedState screen = NMultiplayerPlayerExpandedState.Create(Player);
			NCapstoneContainer.Instance.Open(screen);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(26);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCreatureValueChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "__", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshValues, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHealthBarWidth, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateSelectionReticleWidth, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnergyChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "__", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnStarsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "__", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshCombatValues, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCreatureHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCreatureUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FlashPlayerReady, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHighlightedState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BlockChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldBlock", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "blockGain", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshConnectedState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshConnectedState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlayerVotesCleared, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlayerEndTurnPing, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FlashEndTurn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetNextTweenTime, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlayerScreenChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.TweenLocationIconAway, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TweenLocationIconIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "texture", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureValueChanged && args.Count == 2)
		{
			OnCreatureValueChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshValues && args.Count == 0)
		{
			RefreshValues();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHealthBarWidth && args.Count == 0)
		{
			UpdateHealthBarWidth();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateSelectionReticleWidth && args.Count == 0)
		{
			UpdateSelectionReticleWidth();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnergyChanged && args.Count == 2)
		{
			OnEnergyChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStarsChanged && args.Count == 2)
		{
			OnStarsChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshCombatValues && args.Count == 0)
		{
			RefreshCombatValues();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureHovered && args.Count == 0)
		{
			OnCreatureHovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCreatureUnhovered && args.Count == 0)
		{
			OnCreatureUnhovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashPlayerReady && args.Count == 0)
		{
			FlashPlayerReady();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHighlightedState && args.Count == 0)
		{
			UpdateHighlightedState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlockChanged && args.Count == 2)
		{
			BlockChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshConnectedState && args.Count == 1)
		{
			RefreshConnectedState(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshConnectedState && args.Count == 0)
		{
			RefreshConnectedState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerVotesCleared && args.Count == 0)
		{
			OnPlayerVotesCleared();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerEndTurnPing && args.Count == 1)
		{
			OnPlayerEndTurnPing(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashEndTurn && args.Count == 0)
		{
			FlashEndTurn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetNextTweenTime && args.Count == 0)
		{
			SetNextTweenTime();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerScreenChanged && args.Count == 2)
		{
			OnPlayerScreenChanged(VariantUtils.ConvertTo<ulong>(in args[0]), VariantUtils.ConvertTo<NetScreenType>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenLocationIconAway && args.Count == 0)
		{
			TweenLocationIconAway();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenLocationIconIn && args.Count == 1)
		{
			TweenLocationIconIn(VariantUtils.ConvertTo<Texture2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 1)
		{
			OnFocus(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 1)
		{
			OnUnfocus(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 1)
		{
			OnRelease(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName.OnCreatureValueChanged)
		{
			return true;
		}
		if (method == MethodName.RefreshValues)
		{
			return true;
		}
		if (method == MethodName.UpdateHealthBarWidth)
		{
			return true;
		}
		if (method == MethodName.UpdateSelectionReticleWidth)
		{
			return true;
		}
		if (method == MethodName.OnEnergyChanged)
		{
			return true;
		}
		if (method == MethodName.OnStarsChanged)
		{
			return true;
		}
		if (method == MethodName.RefreshCombatValues)
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
		if (method == MethodName.FlashPlayerReady)
		{
			return true;
		}
		if (method == MethodName.UpdateHighlightedState)
		{
			return true;
		}
		if (method == MethodName.BlockChanged)
		{
			return true;
		}
		if (method == MethodName.RefreshConnectedState)
		{
			return true;
		}
		if (method == MethodName.OnPlayerVotesCleared)
		{
			return true;
		}
		if (method == MethodName.OnPlayerEndTurnPing)
		{
			return true;
		}
		if (method == MethodName.FlashEndTurn)
		{
			return true;
		}
		if (method == MethodName.SetNextTweenTime)
		{
			return true;
		}
		if (method == MethodName.OnPlayerScreenChanged)
		{
			return true;
		}
		if (method == MethodName.TweenLocationIconAway)
		{
			return true;
		}
		if (method == MethodName.TweenLocationIconIn)
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			Hitbox = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._healthBar)
		{
			_healthBar = VariantUtils.ConvertTo<NHealthBar>(in value);
			return true;
		}
		if (name == PropertyName._characterIcon)
		{
			_characterIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			_nameplateLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._topContainer)
		{
			_topContainer = VariantUtils.ConvertTo<HBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._turnEndIndicator)
		{
			_turnEndIndicator = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._disconnectedIndicator)
		{
			_disconnectedIndicator = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._networkProblemIndicator)
		{
			_networkProblemIndicator = VariantUtils.ConvertTo<NMultiplayerNetworkProblemIndicator>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._locationIcon)
		{
			_locationIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._locationContainer)
		{
			_locationContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._energyContainer)
		{
			_energyContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._energyImage)
		{
			_energyImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._energyCount)
		{
			_energyCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._starContainer)
		{
			_starContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._starCount)
		{
			_starCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			_cardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cardImage)
		{
			_cardImage = VariantUtils.ConvertTo<NTinyCard>(in value);
			return true;
		}
		if (name == PropertyName._cardCount)
		{
			_cardCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._locationIconTween)
		{
			_locationIconTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._isMouseOver)
		{
			_isMouseOver = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isCreatureHovered)
		{
			_isCreatureHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHighlighted)
		{
			_isHighlighted = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._focusedWhileTargeting)
		{
			_focusedWhileTargeting = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._nextTweenTime)
		{
			_nextTweenTime = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._currentLocationIcon)
		{
			_currentLocationIcon = VariantUtils.ConvertTo<Texture2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			value = VariantUtils.CreateFrom<NButton>(Hitbox);
			return true;
		}
		if (name == PropertyName._healthBar)
		{
			value = VariantUtils.CreateFrom(in _healthBar);
			return true;
		}
		if (name == PropertyName._characterIcon)
		{
			value = VariantUtils.CreateFrom(in _characterIcon);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			value = VariantUtils.CreateFrom(in _nameplateLabel);
			return true;
		}
		if (name == PropertyName._topContainer)
		{
			value = VariantUtils.CreateFrom(in _topContainer);
			return true;
		}
		if (name == PropertyName._turnEndIndicator)
		{
			value = VariantUtils.CreateFrom(in _turnEndIndicator);
			return true;
		}
		if (name == PropertyName._disconnectedIndicator)
		{
			value = VariantUtils.CreateFrom(in _disconnectedIndicator);
			return true;
		}
		if (name == PropertyName._networkProblemIndicator)
		{
			value = VariantUtils.CreateFrom(in _networkProblemIndicator);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._locationIcon)
		{
			value = VariantUtils.CreateFrom(in _locationIcon);
			return true;
		}
		if (name == PropertyName._locationContainer)
		{
			value = VariantUtils.CreateFrom(in _locationContainer);
			return true;
		}
		if (name == PropertyName._energyContainer)
		{
			value = VariantUtils.CreateFrom(in _energyContainer);
			return true;
		}
		if (name == PropertyName._energyImage)
		{
			value = VariantUtils.CreateFrom(in _energyImage);
			return true;
		}
		if (name == PropertyName._energyCount)
		{
			value = VariantUtils.CreateFrom(in _energyCount);
			return true;
		}
		if (name == PropertyName._starContainer)
		{
			value = VariantUtils.CreateFrom(in _starContainer);
			return true;
		}
		if (name == PropertyName._starCount)
		{
			value = VariantUtils.CreateFrom(in _starCount);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			value = VariantUtils.CreateFrom(in _cardContainer);
			return true;
		}
		if (name == PropertyName._cardImage)
		{
			value = VariantUtils.CreateFrom(in _cardImage);
			return true;
		}
		if (name == PropertyName._cardCount)
		{
			value = VariantUtils.CreateFrom(in _cardCount);
			return true;
		}
		if (name == PropertyName._locationIconTween)
		{
			value = VariantUtils.CreateFrom(in _locationIconTween);
			return true;
		}
		if (name == PropertyName._isMouseOver)
		{
			value = VariantUtils.CreateFrom(in _isMouseOver);
			return true;
		}
		if (name == PropertyName._isCreatureHovered)
		{
			value = VariantUtils.CreateFrom(in _isCreatureHovered);
			return true;
		}
		if (name == PropertyName._isHighlighted)
		{
			value = VariantUtils.CreateFrom(in _isHighlighted);
			return true;
		}
		if (name == PropertyName._focusedWhileTargeting)
		{
			value = VariantUtils.CreateFrom(in _focusedWhileTargeting);
			return true;
		}
		if (name == PropertyName._nextTweenTime)
		{
			value = VariantUtils.CreateFrom(in _nextTweenTime);
			return true;
		}
		if (name == PropertyName._currentLocationIcon)
		{
			value = VariantUtils.CreateFrom(in _currentLocationIcon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._healthBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameplateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._topContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._turnEndIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._disconnectedIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._networkProblemIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._locationIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._locationContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._locationIconTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isMouseOver, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isCreatureHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHighlighted, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._focusedWhileTargeting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._nextTweenTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentLocationIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Hitbox, Variant.From<NButton>(Hitbox));
		info.AddProperty(PropertyName._healthBar, Variant.From(in _healthBar));
		info.AddProperty(PropertyName._characterIcon, Variant.From(in _characterIcon));
		info.AddProperty(PropertyName._nameplateLabel, Variant.From(in _nameplateLabel));
		info.AddProperty(PropertyName._topContainer, Variant.From(in _topContainer));
		info.AddProperty(PropertyName._turnEndIndicator, Variant.From(in _turnEndIndicator));
		info.AddProperty(PropertyName._disconnectedIndicator, Variant.From(in _disconnectedIndicator));
		info.AddProperty(PropertyName._networkProblemIndicator, Variant.From(in _networkProblemIndicator));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._locationIcon, Variant.From(in _locationIcon));
		info.AddProperty(PropertyName._locationContainer, Variant.From(in _locationContainer));
		info.AddProperty(PropertyName._energyContainer, Variant.From(in _energyContainer));
		info.AddProperty(PropertyName._energyImage, Variant.From(in _energyImage));
		info.AddProperty(PropertyName._energyCount, Variant.From(in _energyCount));
		info.AddProperty(PropertyName._starContainer, Variant.From(in _starContainer));
		info.AddProperty(PropertyName._starCount, Variant.From(in _starCount));
		info.AddProperty(PropertyName._cardContainer, Variant.From(in _cardContainer));
		info.AddProperty(PropertyName._cardImage, Variant.From(in _cardImage));
		info.AddProperty(PropertyName._cardCount, Variant.From(in _cardCount));
		info.AddProperty(PropertyName._locationIconTween, Variant.From(in _locationIconTween));
		info.AddProperty(PropertyName._isMouseOver, Variant.From(in _isMouseOver));
		info.AddProperty(PropertyName._isCreatureHovered, Variant.From(in _isCreatureHovered));
		info.AddProperty(PropertyName._isHighlighted, Variant.From(in _isHighlighted));
		info.AddProperty(PropertyName._focusedWhileTargeting, Variant.From(in _focusedWhileTargeting));
		info.AddProperty(PropertyName._nextTweenTime, Variant.From(in _nextTweenTime));
		info.AddProperty(PropertyName._currentLocationIcon, Variant.From(in _currentLocationIcon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Hitbox, out var value))
		{
			Hitbox = value.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._healthBar, out var value2))
		{
			_healthBar = value2.As<NHealthBar>();
		}
		if (info.TryGetProperty(PropertyName._characterIcon, out var value3))
		{
			_characterIcon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._nameplateLabel, out var value4))
		{
			_nameplateLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._topContainer, out var value5))
		{
			_topContainer = value5.As<HBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._turnEndIndicator, out var value6))
		{
			_turnEndIndicator = value6.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._disconnectedIndicator, out var value7))
		{
			_disconnectedIndicator = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._networkProblemIndicator, out var value8))
		{
			_networkProblemIndicator = value8.As<NMultiplayerNetworkProblemIndicator>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value9))
		{
			_selectionReticle = value9.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._locationIcon, out var value10))
		{
			_locationIcon = value10.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._locationContainer, out var value11))
		{
			_locationContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._energyContainer, out var value12))
		{
			_energyContainer = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._energyImage, out var value13))
		{
			_energyImage = value13.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._energyCount, out var value14))
		{
			_energyCount = value14.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._starContainer, out var value15))
		{
			_starContainer = value15.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._starCount, out var value16))
		{
			_starCount = value16.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._cardContainer, out var value17))
		{
			_cardContainer = value17.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cardImage, out var value18))
		{
			_cardImage = value18.As<NTinyCard>();
		}
		if (info.TryGetProperty(PropertyName._cardCount, out var value19))
		{
			_cardCount = value19.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._locationIconTween, out var value20))
		{
			_locationIconTween = value20.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._isMouseOver, out var value21))
		{
			_isMouseOver = value21.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isCreatureHovered, out var value22))
		{
			_isCreatureHovered = value22.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHighlighted, out var value23))
		{
			_isHighlighted = value23.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._focusedWhileTargeting, out var value24))
		{
			_focusedWhileTargeting = value24.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._nextTweenTime, out var value25))
		{
			_nextTweenTime = value25.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._currentLocationIcon, out var value26))
		{
			_currentLocationIcon = value26.As<Texture2D>();
		}
	}
}
