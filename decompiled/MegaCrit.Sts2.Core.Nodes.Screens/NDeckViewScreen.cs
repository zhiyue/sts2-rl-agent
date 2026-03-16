using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NDeckViewScreen.cs")]
public class NDeckViewScreen : NCardsViewScreen
{
	public new class MethodName : NCardsViewScreen.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName AfterCapstoneClosed = "AfterCapstoneClosed";

		public static readonly StringName OnPileContentsChanged = "OnPileContentsChanged";

		public static readonly StringName OnObtainedSort = "OnObtainedSort";

		public static readonly StringName OnCardTypeSort = "OnCardTypeSort";

		public static readonly StringName OnCostSort = "OnCostSort";

		public static readonly StringName OnAlphabetSort = "OnAlphabetSort";

		public static readonly StringName DisplayCards = "DisplayCards";
	}

	public new class PropertyName : NCardsViewScreen.PropertyName
	{
		public new static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName _obtainedSorter = "_obtainedSorter";

		public static readonly StringName _typeSorter = "_typeSorter";

		public static readonly StringName _costSorter = "_costSorter";

		public static readonly StringName _alphabetSorter = "_alphabetSorter";

		public static readonly StringName _bg = "_bg";
	}

	public new class SignalName : NCardsViewScreen.SignalName
	{
	}

	private Player _player;

	private CardPile _pile;

	private NCardViewSortButton _obtainedSorter;

	private NCardViewSortButton _typeSorter;

	private NCardViewSortButton _costSorter;

	private NCardViewSortButton _alphabetSorter;

	private Control _bg;

	private readonly List<SortingOrders> _sortingPriority;

	private static string ScenePath => SceneHelper.GetScenePath("screens/deck_view_screen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public override NetScreenType ScreenType => NetScreenType.DeckView;

	public static NDeckViewScreen? ShowScreen(Player player)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDeckViewScreen nDeckViewScreen = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckViewScreen>(PackedScene.GenEditState.Disabled);
		nDeckViewScreen._player = player;
		NDebugAudioManager.Instance?.Play("map_open.mp3");
		NCapstoneContainer.Instance.Open(nDeckViewScreen);
		return nDeckViewScreen;
	}

	public override void _Ready()
	{
		_cards = _pile.Cards.ToList();
		_infoText = new LocString("gameplay_ui", "DECK_PILE_INFO");
		_bg = GetNode<Control>("%SortingBg");
		_obtainedSorter = GetNode<NCardViewSortButton>("%ObtainedSorter");
		_typeSorter = GetNode<NCardViewSortButton>("%CardTypeSorter");
		_costSorter = GetNode<NCardViewSortButton>("%CostSorter");
		_alphabetSorter = GetNode<NCardViewSortButton>("%AlphabeticalSorter");
		_obtainedSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnObtainedSort));
		_typeSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCardTypeSort));
		_costSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnCostSort));
		_alphabetSorter.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnAlphabetSort));
		_obtainedSorter.SetLabel(new LocString("gameplay_ui", "SORT_OBTAINED").GetRawText());
		_typeSorter.SetLabel(new LocString("gameplay_ui", "SORT_TYPE").GetRawText());
		_costSorter.SetLabel(new LocString("gameplay_ui", "SORT_COST").GetRawText());
		_alphabetSorter.SetLabel(new LocString("gameplay_ui", "SORT_ALPHABET").GetRawText());
		GetNode<MegaLabel>("%ViewUpgradesLabel").SetTextAutoSize(new LocString("gameplay_ui", "VIEW_UPGRADES").GetFormattedText());
		ShaderMaterial shaderMaterial = (ShaderMaterial)_player.Character.CardPool.FrameMaterial;
		_bg.Material = shaderMaterial;
		_obtainedSorter.SetHue(shaderMaterial);
		_typeSorter.SetHue(shaderMaterial);
		_costSorter.SetHue(shaderMaterial);
		_alphabetSorter.SetHue(shaderMaterial);
		ConnectSignals();
		DisplayCards();
		Control[] array = new Control[4] { _obtainedSorter, _typeSorter, _costSorter, _alphabetSorter };
		for (int i = 0; i < array.Length; i++)
		{
			array[i].FocusNeighborTop = array[i].GetPath();
			array[i].FocusNeighborBottom = ((_grid.DefaultFocusedControl != null) ? _grid.DefaultFocusedControl.GetPath() : array[i].GetPath());
			array[i].FocusNeighborLeft = ((i > 0) ? array[i - 1].GetPath() : array[i].GetPath());
			array[i].FocusNeighborRight = ((i < array.Length - 1) ? array[i + 1].GetPath() : array[i].GetPath());
		}
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		_pile = PileType.Deck.GetPile(_player);
		_pile.ContentsChanged += OnPileContentsChanged;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_pile.ContentsChanged -= OnPileContentsChanged;
	}

	public override void AfterCapstoneClosed()
	{
		base.AfterCapstoneClosed();
		NRun.Instance?.GlobalUi.TopBar.Deck.ToggleAnimState();
	}

	private void OnPileContentsChanged()
	{
		_cards = _pile.Cards.ToList();
		DisplayCards();
	}

	private void OnObtainedSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.Ascending);
		_sortingPriority.Remove(SortingOrders.Descending);
		if (_obtainedSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.Descending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.Ascending);
		}
		DisplayCards();
	}

	private void OnCardTypeSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.TypeAscending);
		_sortingPriority.Remove(SortingOrders.TypeDescending);
		if (_typeSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.TypeDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.TypeAscending);
		}
		DisplayCards();
	}

	private void OnCostSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.CostAscending);
		_sortingPriority.Remove(SortingOrders.CostDescending);
		if (_costSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.CostDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.CostAscending);
		}
		DisplayCards();
	}

	private void OnAlphabetSort(NButton button)
	{
		_sortingPriority.Remove(SortingOrders.AlphabetAscending);
		_sortingPriority.Remove(SortingOrders.AlphabetDescending);
		if (_alphabetSorter.IsDescending)
		{
			_sortingPriority.Insert(0, SortingOrders.AlphabetDescending);
		}
		else
		{
			_sortingPriority.Insert(0, SortingOrders.AlphabetAscending);
		}
		DisplayCards();
	}

	private void DisplayCards()
	{
		_grid.YOffset = 100;
		_grid.SetCards(_cards, _pile.Type, _sortingPriority);
		IEnumerable<NGridCardHolder> topRowOfCardNodes = _grid.GetTopRowOfCardNodes();
		if (topRowOfCardNodes == null)
		{
			return;
		}
		foreach (NGridCardHolder item in topRowOfCardNodes)
		{
			item.FocusNeighborTop = _obtainedSorter.GetPath();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPileContentsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnObtainedSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCardTypeSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCostSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnAlphabetSort, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisplayCards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed && args.Count == 0)
		{
			AfterCapstoneClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPileContentsChanged && args.Count == 0)
		{
			OnPileContentsChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnObtainedSort && args.Count == 1)
		{
			OnObtainedSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardTypeSort && args.Count == 1)
		{
			OnCardTypeSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCostSort && args.Count == 1)
		{
			OnCostSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAlphabetSort && args.Count == 1)
		{
			OnAlphabetSort(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisplayCards && args.Count == 0)
		{
			DisplayCards();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed)
		{
			return true;
		}
		if (method == MethodName.OnPileContentsChanged)
		{
			return true;
		}
		if (method == MethodName.OnObtainedSort)
		{
			return true;
		}
		if (method == MethodName.OnCardTypeSort)
		{
			return true;
		}
		if (method == MethodName.OnCostSort)
		{
			return true;
		}
		if (method == MethodName.OnAlphabetSort)
		{
			return true;
		}
		if (method == MethodName.DisplayCards)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._obtainedSorter)
		{
			_obtainedSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._typeSorter)
		{
			_typeSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._costSorter)
		{
			_costSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._alphabetSorter)
		{
			_alphabetSorter = VariantUtils.ConvertTo<NCardViewSortButton>(in value);
			return true;
		}
		if (name == PropertyName._bg)
		{
			_bg = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._obtainedSorter)
		{
			value = VariantUtils.CreateFrom(in _obtainedSorter);
			return true;
		}
		if (name == PropertyName._typeSorter)
		{
			value = VariantUtils.CreateFrom(in _typeSorter);
			return true;
		}
		if (name == PropertyName._costSorter)
		{
			value = VariantUtils.CreateFrom(in _costSorter);
			return true;
		}
		if (name == PropertyName._alphabetSorter)
		{
			value = VariantUtils.CreateFrom(in _alphabetSorter);
			return true;
		}
		if (name == PropertyName._bg)
		{
			value = VariantUtils.CreateFrom(in _bg);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._obtainedSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._typeSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._costSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._alphabetSorter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._obtainedSorter, Variant.From(in _obtainedSorter));
		info.AddProperty(PropertyName._typeSorter, Variant.From(in _typeSorter));
		info.AddProperty(PropertyName._costSorter, Variant.From(in _costSorter));
		info.AddProperty(PropertyName._alphabetSorter, Variant.From(in _alphabetSorter));
		info.AddProperty(PropertyName._bg, Variant.From(in _bg));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._obtainedSorter, out var value))
		{
			_obtainedSorter = value.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._typeSorter, out var value2))
		{
			_typeSorter = value2.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._costSorter, out var value3))
		{
			_costSorter = value3.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._alphabetSorter, out var value4))
		{
			_alphabetSorter = value4.As<NCardViewSortButton>();
		}
		if (info.TryGetProperty(PropertyName._bg, out var value5))
		{
			_bg = value5.As<Control>();
		}
	}

	public NDeckViewScreen()
	{
		int num = 4;
		List<SortingOrders> list = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = SortingOrders.Ascending;
		num2++;
		span[num2] = SortingOrders.TypeAscending;
		num2++;
		span[num2] = SortingOrders.CostAscending;
		span[num2 + 1] = SortingOrders.AlphabetAscending;
		_sortingPriority = list;
		base._002Ector();
	}
}
