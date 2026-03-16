using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NSimpleCardsViewScreen.cs")]
public class NSimpleCardsViewScreen : NCardsViewScreen
{
	public new class MethodName : NCardsViewScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName ConnectSignals = "ConnectSignals";

		public new static readonly StringName AfterCapstoneOpened = "AfterCapstoneOpened";
	}

	public new class PropertyName : NCardsViewScreen.PropertyName
	{
		public new static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName _confirmButton = "_confirmButton";
	}

	public new class SignalName : NCardsViewScreen.SignalName
	{
	}

	private NButton _confirmButton;

	private List<CardPileAddResult> _cardResults;

	private static string ScenePath => SceneHelper.GetScenePath("screens/simple_cards_view_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public override NetScreenType ScreenType => NetScreenType.SimpleCardsView;

	public override void _Ready()
	{
		_confirmButton = GetNode<NButton>("ConfirmButton");
		GetNode<MegaLabel>("%ViewUpgradesLabel").SetTextAutoSize(new LocString("gameplay_ui", "VIEW_UPGRADES").GetFormattedText());
		ConnectSignals();
		NCardGrid grid = _grid;
		List<CardModel> cards = _cards;
		int num = 1;
		List<SortingOrders> list = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = SortingOrders.Ascending;
		grid.SetCards(cards, PileType.Deck, list);
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		_backButton.Disable();
		_confirmButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(base.OnReturnButtonPressed));
		_confirmButton.Enable();
	}

	public static NCardsViewScreen? ShowScreen(List<CardPileAddResult> cards, LocString infoText)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSimpleCardsViewScreen nSimpleCardsViewScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NSimpleCardsViewScreen>(PackedScene.GenEditState.Disabled);
		nSimpleCardsViewScreen._cards = cards.Select((CardPileAddResult c) => c.cardAdded).ToList();
		nSimpleCardsViewScreen._cardResults = cards;
		nSimpleCardsViewScreen._infoText = infoText;
		NDebugAudioManager.Instance?.Play("map_open.mp3");
		NCapstoneContainer.Instance.Open(nSimpleCardsViewScreen);
		return nSimpleCardsViewScreen;
	}

	public override void AfterCapstoneOpened()
	{
		base.AfterCapstoneOpened();
		TaskHelper.RunSafely(FlashRelicsOnModifiedCards());
	}

	private async Task FlashRelicsOnModifiedCards()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		foreach (CardPileAddResult result in _cardResults)
		{
			NGridCardHolder nGridCardHolder = _grid.CurrentlyDisplayedCardHolders.FirstOrDefault((NGridCardHolder h) => h.CardModel == result.cardAdded);
			if (nGridCardHolder == null || result.modifyingModels == null || result.modifyingModels.Count == 0)
			{
				continue;
			}
			foreach (RelicModel item in result.modifyingModels.OfType<RelicModel>())
			{
				item.Flash();
				nGridCardHolder.CardNode?.FlashRelicOnCard(item);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened && args.Count == 0)
		{
			AfterCapstoneOpened();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._confirmButton)
		{
			_confirmButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._confirmButton, out var value))
		{
			_confirmButton = value.As<NButton>();
		}
	}
}
