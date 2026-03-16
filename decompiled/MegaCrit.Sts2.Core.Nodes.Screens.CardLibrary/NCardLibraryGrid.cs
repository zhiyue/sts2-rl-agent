using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

[ScriptPath("res://src/Core/Nodes/Screens/CardLibrary/NCardLibraryGrid.cs")]
public class NCardLibraryGrid : NCardGrid
{
	private struct InitialSorter : IComparer<CardModel>
	{
		private List<CardPoolModel> _cardPoolModels;

		public InitialSorter(List<CardPoolModel> cardPoolModels)
		{
			_cardPoolModels = cardPoolModels;
		}

		public int Compare(CardModel? x, CardModel? y)
		{
			if (x == null)
			{
				if (y != null)
				{
					return -1;
				}
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			int num = _cardPoolModels.IndexOf(x.Pool).CompareTo(_cardPoolModels.IndexOf(y.Pool));
			if (num != 0)
			{
				return num;
			}
			int num2 = x.Rarity.CompareTo(y.Rarity);
			if (num2 != 0)
			{
				return num2;
			}
			return x.Id.CompareTo(y.Id);
		}
	}

	public new class MethodName : NCardGrid.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshVisibility = "RefreshVisibility";

		public new static readonly StringName InitGrid = "InitGrid";

		public new static readonly StringName UpdateGridNavigation = "UpdateGridNavigation";
	}

	public new class PropertyName : NCardGrid.PropertyName
	{
		public new static readonly StringName IsCardLibrary = "IsCardLibrary";

		public new static readonly StringName CenterGrid = "CenterGrid";

		public static readonly StringName ShowStats = "ShowStats";

		public static readonly StringName _showStats = "_showStats";
	}

	public new class SignalName : NCardGrid.SignalName
	{
	}

	private readonly List<CardModel> _allCards = new List<CardModel>();

	private HashSet<ModelId> _seenCards;

	private HashSet<CardModel> _unlockedCards;

	private bool _showStats;

	protected override bool IsCardLibrary => true;

	protected override bool CenterGrid => false;

	public bool ShowStats
	{
		get
		{
			return _showStats;
		}
		set
		{
			_showStats = value;
			foreach (NGridCardHolder item in _cardRows.SelectMany((List<NGridCardHolder> r) => r))
			{
				if (item.CardNode != null)
				{
					NCardLibraryStats node = item.GetNode<NCardLibraryStats>("CardLibraryStats");
					node.Visible = _showStats;
				}
			}
		}
	}

	public IEnumerable<CardModel> VisibleCards => _cards;

	public override void _Ready()
	{
		ConnectSignals();
		List<CardPoolModel> cardPoolModels = ModelDb.AllCardPools.ToList();
		foreach (CardModel allCard in ModelDb.AllCards)
		{
			if (allCard.ShouldShowInCardLibrary)
			{
				_allCards.Add(allCard);
			}
		}
		_allCards.Sort(new InitialSorter(cardPoolModels));
		RefreshVisibility();
	}

	public void RefreshVisibility()
	{
		_seenCards = SaveManager.Instance.Progress.DiscoveredCards.ToHashSet();
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		_unlockedCards = ModelDb.AllCardPools.Select((CardPoolModel p) => p.GetUnlockedCards(unlockState, CardMultiplayerConstraint.None)).SelectMany((IEnumerable<CardModel> c) => c).ToHashSet();
	}

	public void FilterCards(Func<CardModel, bool> filter)
	{
		int num = 1;
		List<SortingOrders> list = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = SortingOrders.AlphabetAscending;
		FilterCards(filter, list);
	}

	public void FilterCards(Func<CardModel, bool> filter, List<SortingOrders> sortingPriority)
	{
		List<CardModel> cards = _allCards.Where(filter).ToList();
		DisplayCards(cards, sortingPriority);
	}

	private void DisplayCards(List<CardModel> cards, List<SortingOrders> sortingPriority)
	{
		SetCards(cards, PileType.None, sortingPriority, Task.CompletedTask);
	}

	protected override void InitGrid()
	{
		base.InitGrid();
		foreach (NGridCardHolder item in _cardRows.SelectMany((List<NGridCardHolder> r) => r))
		{
			if (item.CardNode != null)
			{
				CardModel model = item.CardNode.Model;
				bool flag = _seenCards.Contains(model.Id);
				item.EnsureCardLibraryStatsExists();
				NCardLibraryStats cardLibraryStats = item.CardLibraryStats;
				cardLibraryStats.UpdateStats(item.CardNode.Model);
				cardLibraryStats.Visible = ShowStats;
				item.Hitbox.MouseDefaultCursorShape = (CursorShape)(flag ? 16 : 0);
			}
		}
	}

	protected override void AssignCardsToRow(List<NGridCardHolder> row, int startIndex)
	{
		base.AssignCardsToRow(row, startIndex);
		foreach (NGridCardHolder item in row)
		{
			if (item.CardNode != null)
			{
				CardModel model = item.CardNode.Model;
				bool flag = _seenCards.Contains(model.Id);
				item.CardLibraryStats.UpdateStats(model);
				item.Hitbox.MouseDefaultCursorShape = (CursorShape)(flag ? 16 : 0);
			}
		}
	}

	protected override ModelVisibility GetCardVisibility(CardModel card)
	{
		if (!_unlockedCards.Contains(card))
		{
			return ModelVisibility.Locked;
		}
		if (!_seenCards.Contains(card.Id))
		{
			return ModelVisibility.NotSeen;
		}
		return ModelVisibility.Visible;
	}

	protected override void UpdateGridNavigation()
	{
		for (int i = 0; i < _cardRows.Count; i++)
		{
			for (int j = 0; j < _cardRows[i].Count; j++)
			{
				NCardHolder nCardHolder = _cardRows[i][j];
				nCardHolder.FocusNeighborLeft = ((j > 0) ? _cardRows[i][j - 1].GetPath() : null);
				nCardHolder.FocusNeighborRight = ((j < _cardRows[i].Count - 1) ? _cardRows[i][j + 1].GetPath() : null);
				nCardHolder.FocusNeighborTop = ((i > 0) ? _cardRows[i - 1][j].GetPath() : null);
				nCardHolder.FocusNeighborBottom = ((i < _cardRows.Count - 1 && j < _cardRows[i + 1].Count) ? _cardRows[i + 1][j].GetPath() : null);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateGridNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RefreshVisibility && args.Count == 0)
		{
			RefreshVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitGrid && args.Count == 0)
		{
			InitGrid();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateGridNavigation && args.Count == 0)
		{
			UpdateGridNavigation();
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
		if (method == MethodName.RefreshVisibility)
		{
			return true;
		}
		if (method == MethodName.InitGrid)
		{
			return true;
		}
		if (method == MethodName.UpdateGridNavigation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ShowStats)
		{
			ShowStats = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._showStats)
		{
			_showStats = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.IsCardLibrary)
		{
			from = IsCardLibrary;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CenterGrid)
		{
			from = CenterGrid;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ShowStats)
		{
			from = ShowStats;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._showStats)
		{
			value = VariantUtils.CreateFrom(in _showStats);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsCardLibrary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CenterGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._showStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShowStats, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ShowStats, Variant.From<bool>(ShowStats));
		info.AddProperty(PropertyName._showStats, Variant.From(in _showStats));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ShowStats, out var value))
		{
			ShowStats = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._showStats, out var value2))
		{
			_showStats = value2.As<bool>();
		}
	}
}
