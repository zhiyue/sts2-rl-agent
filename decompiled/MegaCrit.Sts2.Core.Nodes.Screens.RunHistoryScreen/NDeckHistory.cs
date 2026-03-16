using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NDeckHistory.cs")]
public class NDeckHistory : VBoxContainer
{
	[Signal]
	public delegate void HoveredEventHandler(NDeckHistoryEntry deckHistoryEntry);

	[Signal]
	public delegate void UnhoveredEventHandler(NDeckHistoryEntry deckHistoryEntry);

	public new class MethodName : VBoxContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ShowEntry = "ShowEntry";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName _headerLabel = "_headerLabel";

		public static readonly StringName _cardContainer = "_cardContainer";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
		public static readonly StringName Hovered = "Hovered";

		public static readonly StringName Unhovered = "Unhovered";
	}

	private readonly LocString _deckHeader = new LocString("run_history", "DECK_HISTORY.header");

	private readonly LocString _cardCategories = new LocString("run_history", "DECK_HISTORY.categories");

	private MegaRichTextLabel _headerLabel;

	private Control _cardContainer;

	private readonly List<CardModel> _allCards = new List<CardModel>();

	private HoveredEventHandler backing_Hovered;

	private UnhoveredEventHandler backing_Unhovered;

	public event HoveredEventHandler Hovered
	{
		add
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Combine(backing_Hovered, value);
		}
		remove
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Remove(backing_Hovered, value);
		}
	}

	public event UnhoveredEventHandler Unhovered
	{
		add
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Combine(backing_Unhovered, value);
		}
		remove
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Remove(backing_Unhovered, value);
		}
	}

	public override void _Ready()
	{
		_headerLabel = GetNode<MegaRichTextLabel>("Header");
		_cardContainer = GetNode<Control>("%CardContainer");
	}

	public void LoadDeck(Player player, IEnumerable<SerializableCard> cards)
	{
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<CardRarity, int> dictionary = new Dictionary<CardRarity, int>();
		CardRarity[] values = Enum.GetValues<CardRarity>();
		foreach (CardRarity key in values)
		{
			dictionary.Add(key, 0);
		}
		List<SerializableCard> list = cards.ToList();
		CardRarity key2;
		int value;
		foreach (SerializableCard item in list)
		{
			CardModel cardModel = SaveUtil.CardOrDeprecated(item.Id);
			key2 = cardModel.Rarity;
			value = dictionary[key2]++;
		}
		_deckHeader.Add("totalCards", list.Count);
		foreach (KeyValuePair<CardRarity, int> item2 in dictionary)
		{
			item2.Deconstruct(out key2, out value);
			CardRarity cardRarity = key2;
			int num = value;
			_cardCategories.Add(cardRarity.ToString() + "Cards", num);
		}
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder2);
		handler.AppendLiteral("[gold][b]");
		handler.AppendFormatted(_deckHeader.GetFormattedText());
		handler.AppendLiteral("[/b][/gold]");
		stringBuilder2.Append(ref handler);
		stringBuilder.Append(_cardCategories.GetFormattedText().Trim(','));
		_headerLabel.Text = stringBuilder.ToString();
		PopulateCards(player, list);
	}

	private void PopulateCards(Player player, IEnumerable<SerializableCard> cards)
	{
		foreach (Node child in _cardContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		_allCards.Clear();
		foreach (IGrouping<SerializableCard, SerializableCard> item in from x in cards
			group x by x)
		{
			CardModel cardModel = CardModel.FromSerializable(item.Key);
			cardModel.Owner = player;
			_allCards.Add(cardModel);
			NDeckHistoryEntry entry = NDeckHistoryEntry.Create(cardModel, item.Count(), from c in item
				where c.FloorAddedToDeck.HasValue
				select c.FloorAddedToDeck.Value);
			entry.Connect(NDeckHistoryEntry.SignalName.Clicked, Callable.From<NDeckHistoryEntry>(ShowEntry));
			entry.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(delegate
			{
				EmitSignal(SignalName.Hovered, entry);
			}));
			entry.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(delegate
			{
				EmitSignal(SignalName.Unhovered, entry);
			}));
			_cardContainer.AddChildSafely(entry);
		}
	}

	private void ShowEntry(NDeckHistoryEntry entry)
	{
		NGame.Instance.GetInspectCardScreen().Open(_allCards, _allCards.IndexOf(entry.Card));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowEntry, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.ShowEntry && args.Count == 1)
		{
			ShowEntry(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
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
		if (method == MethodName.ShowEntry)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._headerLabel)
		{
			_headerLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			_cardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._headerLabel)
		{
			value = VariantUtils.CreateFrom(in _headerLabel);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			value = VariantUtils.CreateFrom(in _cardContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._headerLabel, Variant.From(in _headerLabel));
		info.AddProperty(PropertyName._cardContainer, Variant.From(in _cardContainer));
		info.AddSignalEventDelegate(SignalName.Hovered, backing_Hovered);
		info.AddSignalEventDelegate(SignalName.Unhovered, backing_Unhovered);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._headerLabel, out var value))
		{
			_headerLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._cardContainer, out var value2))
		{
			_cardContainer = value2.As<Control>();
		}
		if (info.TryGetSignalEventDelegate<HoveredEventHandler>(SignalName.Hovered, out var value3))
		{
			backing_Hovered = value3;
		}
		if (info.TryGetSignalEventDelegate<UnhoveredEventHandler>(SignalName.Unhovered, out var value4))
		{
			backing_Unhovered = value4;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Hovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "deckHistoryEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.Unhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "deckHistoryEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHovered(NDeckHistoryEntry deckHistoryEntry)
	{
		EmitSignal(SignalName.Hovered, deckHistoryEntry);
	}

	protected void EmitSignalUnhovered(NDeckHistoryEntry deckHistoryEntry)
	{
		EmitSignal(SignalName.Unhovered, deckHistoryEntry);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Hovered && args.Count == 1)
		{
			backing_Hovered?.Invoke(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
		}
		else if (signal == SignalName.Unhovered && args.Count == 1)
		{
			backing_Unhovered?.Invoke(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Hovered)
		{
			return true;
		}
		if (signal == SignalName.Unhovered)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
