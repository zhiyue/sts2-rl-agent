using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerExpandedState.cs")]
public class NMultiplayerPlayerExpandedState : Control, ICapstoneScreen, IScreenContext
{
	private class CardGroupKey
	{
		private readonly CardModel _card;

		public CardGroupKey(CardModel card)
		{
			_card = card;
		}

		public override bool Equals(object? obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			CardGroupKey cardGroupKey = (CardGroupKey)obj;
			if (_card.Id.Equals(cardGroupKey._card.Id) && _card.CurrentUpgradeLevel == cardGroupKey._card.CurrentUpgradeLevel && _card.Enchantment?.Id == cardGroupKey._card.Enchantment?.Id)
			{
				return _card.Enchantment?.Amount == cardGroupKey._card.Enchantment?.Amount;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(_card.Id, _card.CurrentUpgradeLevel, _card.Enchantment?.Id, _card.Enchantment?.Amount);
		}
	}

	public new class MethodName : Control.MethodName
	{
		public static readonly StringName AfterCapstoneOpened = "AfterCapstoneOpened";

		public static readonly StringName AfterCapstoneClosed = "AfterCapstoneClosed";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ShowEntry = "ShowEntry";

		public static readonly StringName BackButtonPressed = "BackButtonPressed";

		public static readonly StringName OnRelicClicked = "OnRelicClicked";

		public static readonly StringName UpdateNavigation = "UpdateNavigation";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _playerNameLabel = "_playerNameLabel";

		public static readonly StringName _cardsHeader = "_cardsHeader";

		public static readonly StringName _cardContainer = "_cardContainer";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _potionsHeader = "_potionsHeader";

		public static readonly StringName _potionContainer = "_potionContainer";

		public static readonly StringName _relicsHeader = "_relicsHeader";

		public static readonly StringName _relicContainer = "_relicContainer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/multiplayer_player_expanded_state");

	private MegaRichTextLabel _playerNameLabel;

	private MegaRichTextLabel _cardsHeader;

	private Control _cardContainer;

	private NBackButton _backButton;

	private MegaRichTextLabel _potionsHeader;

	private Control _potionContainer;

	private MegaRichTextLabel _relicsHeader;

	private Control _relicContainer;

	private Player _player;

	private List<CardModel> _cards = new List<CardModel>();

	public bool UseSharedBackstop => true;

	public NetScreenType ScreenType => NetScreenType.RemotePlayerExpandedState;

	public Control? DefaultFocusedControl => null;

	public static NMultiplayerPlayerExpandedState Create(Player player)
	{
		NMultiplayerPlayerExpandedState nMultiplayerPlayerExpandedState = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMultiplayerPlayerExpandedState>(PackedScene.GenEditState.Disabled);
		nMultiplayerPlayerExpandedState._player = player;
		return nMultiplayerPlayerExpandedState;
	}

	public void AfterCapstoneOpened()
	{
		_backButton.Enable();
		NGlobalUi globalUi = NRun.Instance.GlobalUi;
		globalUi.TopBar.AnimHide();
		globalUi.RelicInventory.AnimHide();
		globalUi.MultiplayerPlayerContainer.AnimHide();
		globalUi.MoveChild(globalUi.AboveTopBarVfxContainer, globalUi.CapstoneContainer.GetIndex());
		globalUi.MoveChild(globalUi.CardPreviewContainer, globalUi.CapstoneContainer.GetIndex());
		globalUi.MoveChild(globalUi.MessyCardPreviewContainer, globalUi.CapstoneContainer.GetIndex());
	}

	public void AfterCapstoneClosed()
	{
		NGlobalUi globalUi = NRun.Instance.GlobalUi;
		globalUi.TopBar.AnimShow();
		globalUi.RelicInventory.AnimShow();
		globalUi.MultiplayerPlayerContainer.AnimShow();
		globalUi.MoveChild(globalUi.AboveTopBarVfxContainer, globalUi.TopBar.GetIndex() + 1);
		globalUi.MoveChild(globalUi.CardPreviewContainer, globalUi.TopBar.GetIndex() + 1);
		globalUi.MoveChild(globalUi.MessyCardPreviewContainer, globalUi.TopBar.GetIndex() + 1);
		this.QueueFreeSafely();
		_backButton.Disable();
	}

	public override void _Ready()
	{
		_playerNameLabel = GetNode<MegaRichTextLabel>("%PlayerNameLabel");
		_cardsHeader = GetNode<MegaRichTextLabel>("%CardsHeader");
		_cardContainer = GetNode<Control>("%CardContainer");
		_relicsHeader = GetNode<MegaRichTextLabel>("%RelicsHeader");
		_relicContainer = GetNode<Control>("%RelicContainer");
		_potionsHeader = GetNode<MegaRichTextLabel>("%PotionsHeader");
		_potionContainer = GetNode<Control>("%PotionContainer");
		_backButton = GetNode<NBackButton>("%BackButton");
		LocString locString = new LocString("gameplay_ui", "MULTIPLAYER_EXPANDED_STATE.title");
		locString.Add("PlayerName", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, _player.NetId));
		locString.Add("Character", _player.Character.Title);
		_playerNameLabel.Text = locString.GetFormattedText();
		LocString locString2 = new LocString("gameplay_ui", "MULTIPLAYER_EXPANDED_STATE.relicHeader");
		_relicsHeader.Text = locString2.GetFormattedText();
		LocString locString3 = new LocString("gameplay_ui", "MULTIPLAYER_EXPANDED_STATE.cardHeader");
		_cardsHeader.Text = locString3.GetFormattedText();
		LocString locString4 = new LocString("gameplay_ui", "MULTIPLAYER_EXPANDED_STATE.potionHeader");
		_potionsHeader.Text = locString4.GetFormattedText();
		foreach (RelicModel relic in _player.Relics)
		{
			NRelicBasicHolder holder = NRelicBasicHolder.Create(relic);
			_relicContainer.AddChildSafely(holder);
			holder.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
			{
				OnRelicClicked(holder.Relic);
			}));
			holder.MouseDefaultCursorShape = CursorShape.Help;
		}
		foreach (PotionModel potion in _player.Potions)
		{
			NPotionHolder nPotionHolder = NPotionHolder.Create(isUsable: false);
			NPotion nPotion = NPotion.Create(potion);
			_potionContainer.AddChildSafely(nPotionHolder);
			nPotionHolder.AddPotion(nPotion);
			nPotion.Position = Vector2.Zero;
		}
		_cards.Clear();
		_cards.AddRange(_player.Deck.Cards);
		foreach (IGrouping<CardGroupKey, CardModel> item in from x in _player.Deck.Cards
			group x by new CardGroupKey(x))
		{
			NDeckHistoryEntry nDeckHistoryEntry = NDeckHistoryEntry.Create(item.First(), item.Count());
			nDeckHistoryEntry.Connect(NDeckHistoryEntry.SignalName.Clicked, Callable.From<NDeckHistoryEntry>(ShowEntry));
			_cardContainer.AddChildSafely(nDeckHistoryEntry);
		}
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(BackButtonPressed));
		UpdateNavigation();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (!IsVisibleInTree() || NDevConsole.Instance.Visible || !NControllerManager.Instance.IsUsingController)
		{
			return;
		}
		Control control = GetViewport().GuiGetFocusOwner();
		bool flag = ((control is TextEdit || control is LineEdit) ? true : false);
		if (!flag && ActiveScreenContext.Instance.IsCurrent(this))
		{
			Control control2 = GetViewport().GuiGetFocusOwner();
			if ((control2 == null || !IsAncestorOf(control2)) && (inputEvent.IsActionPressed(MegaInput.left) || inputEvent.IsActionPressed(MegaInput.right) || inputEvent.IsActionPressed(MegaInput.up) || inputEvent.IsActionPressed(MegaInput.down) || inputEvent.IsActionPressed(MegaInput.select)))
			{
				_relicContainer.GetChild<NRelicBasicHolder>(0).TryGrabFocus();
				GetViewport()?.SetInputAsHandled();
			}
		}
	}

	private void ShowEntry(NDeckHistoryEntry entry)
	{
		NGame.Instance.GetInspectCardScreen().Open(_cards, _cards.IndexOf(entry.Card));
	}

	private void BackButtonPressed(NButton _)
	{
		NCapstoneContainer.Instance.Close();
	}

	private void OnRelicClicked(NRelic node)
	{
		List<RelicModel> list = new List<RelicModel>();
		foreach (NRelicBasicHolder item in _relicContainer.GetChildren().OfType<NRelicBasicHolder>())
		{
			list.Add(item.Relic.Model);
		}
		NGame.Instance.GetInspectRelicScreen().Open(list, node.Model);
	}

	private void UpdateNavigation()
	{
		for (int i = 0; i < _relicContainer.GetChildCount(); i++)
		{
			NRelicBasicHolder child = _relicContainer.GetChild<NRelicBasicHolder>(i);
			child.FocusNeighborLeft = ((i > 0) ? _relicContainer.GetChild<NRelicBasicHolder>(i - 1).GetPath() : _relicContainer.GetChild<NRelicBasicHolder>(i).GetPath());
			child.FocusNeighborRight = ((i < _relicContainer.GetChildCount() - 1) ? _relicContainer.GetChild<NRelicBasicHolder>(i + 1).GetPath() : _relicContainer.GetChild<NRelicBasicHolder>(i).GetPath());
			child.FocusNeighborTop = child.GetPath();
			if (_potionContainer.GetChildCount() > 0)
			{
				child.FocusNeighborBottom = _potionContainer.GetChild<Control>(Mathf.Min(i, _potionContainer.GetChildCount() - 1))?.GetPath();
			}
			else if (_cardContainer.GetChildCount() > 0)
			{
				child.FocusNeighborBottom = _cardContainer.GetChild<Control>(Mathf.Min(i, _cardContainer.GetChildCount() - 1))?.GetPath();
			}
			else
			{
				child.FocusNeighborBottom = child.GetPath();
			}
		}
		for (int j = 0; j < _potionContainer.GetChildCount(); j++)
		{
			NPotionHolder child2 = _potionContainer.GetChild<NPotionHolder>(j);
			child2.FocusNeighborLeft = ((j > 0) ? _potionContainer.GetChild<NPotionHolder>(j - 1).GetPath() : _potionContainer.GetChild<NPotionHolder>(j).GetPath());
			child2.FocusNeighborRight = ((j < _potionContainer.GetChildCount() - 1) ? _potionContainer.GetChild<NPotionHolder>(j + 1).GetPath() : _potionContainer.GetChild<NPotionHolder>(j).GetPath());
			if (_relicContainer.GetChildCount() > 0)
			{
				child2.FocusNeighborTop = _relicContainer.GetChild<Control>(Mathf.Min(j, _relicContainer.GetChildCount() - 1))?.GetPath();
			}
			else
			{
				child2.FocusNeighborTop = child2.GetPath();
			}
			if (_cardContainer.GetChildCount() > 0)
			{
				child2.FocusNeighborBottom = _cardContainer.GetChild<Control>(Mathf.Min(j, _cardContainer.GetChildCount() - 1))?.GetPath();
			}
			else
			{
				child2.FocusNeighborBottom = child2.GetPath();
			}
		}
		for (int k = 0; k < _cardContainer.GetChildCount(); k++)
		{
			NDeckHistoryEntry child3 = _cardContainer.GetChild<NDeckHistoryEntry>(k);
			child3.FocusNeighborLeft = ((k > 0) ? _cardContainer.GetChild<NDeckHistoryEntry>(k - 1).GetPath() : _cardContainer.GetChild<NDeckHistoryEntry>(k).GetPath());
			child3.FocusNeighborRight = ((k < _cardContainer.GetChildCount() - 1) ? _cardContainer.GetChild<NDeckHistoryEntry>(k + 1).GetPath() : _cardContainer.GetChild<NDeckHistoryEntry>(k).GetPath());
			if (_potionContainer.GetChildCount() > 0)
			{
				child3.FocusNeighborTop = _potionContainer.GetChild<Control>(Mathf.Min(k, _potionContainer.GetChildCount() - 1))?.GetPath();
			}
			else if (_relicContainer.GetChildCount() > 0)
			{
				child3.FocusNeighborTop = _relicContainer.GetChild<Control>(Mathf.Min(k, _relicContainer.GetChildCount() - 1))?.GetPath();
			}
			else
			{
				child3.FocusNeighborTop = child3.GetPath();
			}
			child3.FocusNeighborBottom = child3.GetPath();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.AfterCapstoneOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.BackButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRelicClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.AfterCapstoneOpened && args.Count == 0)
		{
			AfterCapstoneOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed && args.Count == 0)
		{
			AfterCapstoneClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowEntry && args.Count == 1)
		{
			ShowEntry(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BackButtonPressed && args.Count == 1)
		{
			BackButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelicClicked && args.Count == 1)
		{
			OnRelicClicked(VariantUtils.ConvertTo<NRelic>(in args[0]));
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
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.AfterCapstoneOpened)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ShowEntry)
		{
			return true;
		}
		if (method == MethodName.BackButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnRelicClicked)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._playerNameLabel)
		{
			_playerNameLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._cardsHeader)
		{
			_cardsHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			_cardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._potionsHeader)
		{
			_potionsHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			_potionContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._relicsHeader)
		{
			_relicsHeader = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicContainer)
		{
			_relicContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._playerNameLabel)
		{
			value = VariantUtils.CreateFrom(in _playerNameLabel);
			return true;
		}
		if (name == PropertyName._cardsHeader)
		{
			value = VariantUtils.CreateFrom(in _cardsHeader);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			value = VariantUtils.CreateFrom(in _cardContainer);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._potionsHeader)
		{
			value = VariantUtils.CreateFrom(in _potionsHeader);
			return true;
		}
		if (name == PropertyName._potionContainer)
		{
			value = VariantUtils.CreateFrom(in _potionContainer);
			return true;
		}
		if (name == PropertyName._relicsHeader)
		{
			value = VariantUtils.CreateFrom(in _relicsHeader);
			return true;
		}
		if (name == PropertyName._relicContainer)
		{
			value = VariantUtils.CreateFrom(in _relicContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardsHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionsHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicsHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._playerNameLabel, Variant.From(in _playerNameLabel));
		info.AddProperty(PropertyName._cardsHeader, Variant.From(in _cardsHeader));
		info.AddProperty(PropertyName._cardContainer, Variant.From(in _cardContainer));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._potionsHeader, Variant.From(in _potionsHeader));
		info.AddProperty(PropertyName._potionContainer, Variant.From(in _potionContainer));
		info.AddProperty(PropertyName._relicsHeader, Variant.From(in _relicsHeader));
		info.AddProperty(PropertyName._relicContainer, Variant.From(in _relicContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._playerNameLabel, out var value))
		{
			_playerNameLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._cardsHeader, out var value2))
		{
			_cardsHeader = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._cardContainer, out var value3))
		{
			_cardContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value4))
		{
			_backButton = value4.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._potionsHeader, out var value5))
		{
			_potionsHeader = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._potionContainer, out var value6))
		{
			_potionContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._relicsHeader, out var value7))
		{
			_relicsHeader = value7.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicContainer, out var value8))
		{
			_relicContainer = value8.As<Control>();
		}
	}
}
