using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;

[ScriptPath("res://src/Core/Nodes/Screens/RelicCollection/NRelicCollectionCategory.cs")]
public class NRelicCollectionCategory : VBoxContainer
{
	public new class MethodName : VBoxContainer.MethodName
	{
		public static readonly StringName CreateForSubcategory = "CreateForSubcategory";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName LoadIcon = "LoadIcon";

		public static readonly StringName ClearRelics = "ClearRelics";

		public static readonly StringName OnRelicEntryPressed = "OnRelicEntryPressed";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _collection = "_collection";

		public static readonly StringName _headerLabel = "_headerLabel";

		public static readonly StringName _relicsContainer = "_relicsContainer";

		public static readonly StringName _spacer = "_spacer";

		public static readonly StringName _icon = "_icon";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("screens/relic_collection/relic_collection_subcategory");

	private static readonly List<RelicModel> _relicModelCache = new List<RelicModel>();

	private NRelicCollection _collection;

	private MegaRichTextLabel _headerLabel;

	private GridContainer _relicsContainer;

	private readonly List<NRelicCollectionCategory> _subCategories = new List<NRelicCollectionCategory>();

	private Control _spacer;

	private TextureRect _icon;

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_subCategories.Any())
			{
				return _subCategories.First().DefaultFocusedControl;
			}
			if (GodotObject.IsInstanceValid(_relicsContainer) && _relicsContainer.GetChildCount() > 0)
			{
				return _relicsContainer.GetChild<Control>(0);
			}
			return null;
		}
	}

	private NRelicCollectionCategory CreateForSubcategory()
	{
		return PreloadManager.Cache.GetScene(scenePath).Instantiate<NRelicCollectionCategory>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		_headerLabel = GetNode<MegaRichTextLabel>("%Header");
		_icon = GetNode<TextureRect>("%Icon");
		_relicsContainer = GetNode<GridContainer>("%RelicsContainer");
		_spacer = GetNode<Control>("Spacer");
		_icon.Visible = false;
	}

	public void LoadRelics(RelicRarity relicRarity, NRelicCollection collection, LocString header, HashSet<RelicModel> seenRelics, UnlockState unlockState, HashSet<RelicModel> allUnlockedRelics)
	{
		_subCategories.Clear();
		_headerLabel.Text = header.GetFormattedText();
		_collection = collection;
		_relicModelCache.Clear();
		_relicModelCache.AddRange(ModelDb.AllRelics.Where((RelicModel relic) => relic.Rarity == relicRarity));
		if (relicRarity == RelicRarity.Starter)
		{
			List<RelicModel> list = ModelDb.AllCharacters.SelectMany((CharacterModel c) => c.StartingRelics).ToList();
			NRelicCollectionCategory nRelicCollectionCategory = CreateForSubcategory();
			_subCategories.Add(nRelicCollectionCategory);
			this.AddChildSafely(nRelicCollectionCategory);
			MoveChild(nRelicCollectionCategory, _headerLabel.GetIndex() + 1);
			nRelicCollectionCategory._spacer.Visible = false;
			nRelicCollectionCategory._headerLabel.Visible = false;
			nRelicCollectionCategory.LoadSubcategory(_collection, null, list, seenRelics, allUnlockedRelics);
			IEnumerable<RelicModel> relics = list.Select((RelicModel r) => ModelDb.Relic<TouchOfOrobas>().GetUpgradedStarterRelic(r));
			NRelicCollectionCategory nRelicCollectionCategory2 = CreateForSubcategory();
			_subCategories.Add(nRelicCollectionCategory2);
			this.AddChildSafely(nRelicCollectionCategory2);
			MoveChild(nRelicCollectionCategory2, _headerLabel.GetIndex() + 2);
			nRelicCollectionCategory2._headerLabel.Visible = false;
			nRelicCollectionCategory2.LoadSubcategory(_collection, null, relics, seenRelics, allUnlockedRelics);
			return;
		}
		if (relicRarity == RelicRarity.Ancient)
		{
			int num = 4;
			List<ActModel> list2 = new List<ActModel>(num);
			CollectionsMarshal.SetCount(list2, num);
			Span<ActModel> span = CollectionsMarshal.AsSpan(list2);
			int num2 = 0;
			span[num2] = ModelDb.Act<Overgrowth>();
			num2++;
			span[num2] = ModelDb.Act<Underdocks>();
			num2++;
			span[num2] = ModelDb.Act<Hive>();
			num2++;
			span[num2] = ModelDb.Act<Glory>();
			List<ActModel> list3 = list2;
			if (ModelDb.Acts.Except(list3).Any())
			{
				throw new InvalidOperationException("The act list in NRelicCollectionCategory is out of date!");
			}
			List<AncientEventModel> list4 = list3.Select((ActModel a) => a.AllAncients).SelectMany((IEnumerable<AncientEventModel> a) => a).Concat(ModelDb.AllSharedAncients)
				.Distinct()
				.ToList();
			HashSet<AncientEventModel> hashSet = list3.Select((ActModel a) => a.GetUnlockedAncients(unlockState)).SelectMany((IEnumerable<AncientEventModel> a) => a).Concat(unlockState.SharedAncients)
				.Distinct()
				.ToHashSet();
			IReadOnlyDictionary<ModelId, AncientStats> ancientStats = SaveManager.Instance.Progress.AncientStats;
			LocString locString = new LocString("relic_collection", "UNKNOWN_ANCIENT");
			for (int num3 = 0; num3 < list4.Count; num3++)
			{
				AncientEventModel ancientEventModel = list4[num3];
				if (hashSet.Contains(ancientEventModel))
				{
					NRelicCollectionCategory nRelicCollectionCategory3 = CreateForSubcategory();
					_subCategories.Add(nRelicCollectionCategory3);
					this.AddChildSafely(nRelicCollectionCategory3);
					MoveChild(nRelicCollectionCategory3, _headerLabel.GetIndex() + num3 + 1);
					StringComparer comparer = StringComparer.Create(LocManager.Instance.CultureInfo, CompareOptions.None);
					RelicModel[] array = ancientEventModel.AllPossibleOptions.Select((EventOption o) => o.Relic?.CanonicalInstance).OfType<RelicModel>().Intersect(_relicModelCache)
						.OrderBy<RelicModel, string>((RelicModel r) => r.Title.GetFormattedText(), comparer)
						.ToArray();
					bool flag = ancientStats.ContainsKey(ancientEventModel.Id) || array.Any((RelicModel r) => seenRelics.Contains(r));
					LocString locString2 = new LocString("relic_collection", "ANCIENT_SUBCATEGORY");
					locString2.Add("Ancient", flag ? ancientEventModel.Title : locString);
					nRelicCollectionCategory3.LoadSubcategory(_collection, locString2, array, seenRelics, allUnlockedRelics);
					nRelicCollectionCategory3.LoadIcon(ancientEventModel.RunHistoryIcon);
				}
			}
			return;
		}
		List<RelicModel> list5 = new List<RelicModel>();
		List<RelicModel> list6 = new List<RelicModel>();
		foreach (RelicPoolModel allCharacterRelicPool in ModelDb.AllCharacterRelicPools)
		{
			foreach (RelicModel item in _relicModelCache)
			{
				if (allCharacterRelicPool.AllRelicIds.Contains(item.Id))
				{
					list5.Add(item);
				}
			}
		}
		foreach (RelicModel item2 in _relicModelCache)
		{
			if (!list5.Contains(item2))
			{
				list6.Add(item2);
			}
		}
		StringComparer comparer2 = StringComparer.Create(LocManager.Instance.CultureInfo, CompareOptions.None);
		list6.Sort((RelicModel p1, RelicModel p2) => comparer2.Compare(p1.Title.GetFormattedText(), p2.Title.GetFormattedText()));
		LoadRelicNodes(list6.Concat(list5), seenRelics, allUnlockedRelics);
		_collection.AddRelics(list6);
		_collection.AddRelics(list5);
	}

	private void LoadSubcategory(NRelicCollection collection, LocString? header, IEnumerable<RelicModel> relics, HashSet<RelicModel> seenRelics, HashSet<RelicModel> unlockedRelics)
	{
		_headerLabel.Text = header?.GetFormattedText() ?? "";
		_collection = collection;
		_collection.AddRelics(relics);
		LoadRelicNodes(relics, seenRelics, unlockedRelics);
	}

	private void LoadIcon(Texture2D tex)
	{
		_icon.Texture = tex;
		_icon.Visible = true;
	}

	private void LoadRelicNodes(IEnumerable<RelicModel> relics, HashSet<RelicModel> seenRelics, HashSet<RelicModel> unlockedRelics)
	{
		foreach (Node child in _relicsContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (RelicModel relic in relics)
		{
			ModelVisibility visibility = ((!unlockedRelics.Contains(relic)) ? ModelVisibility.Locked : (seenRelics.Contains(relic) ? ModelVisibility.Visible : ModelVisibility.NotSeen));
			NRelicCollectionEntry nRelicCollectionEntry = NRelicCollectionEntry.Create(relic, visibility);
			_relicsContainer.AddChildSafely(nRelicCollectionEntry);
			nRelicCollectionEntry.Connect(NClickableControl.SignalName.Released, Callable.From<NRelicCollectionEntry>(OnRelicEntryPressed));
		}
	}

	public void ClearRelics()
	{
		foreach (Node child in _relicsContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (NRelicCollectionCategory item in GetChildren().OfType<NRelicCollectionCategory>())
		{
			item.QueueFreeSafely();
		}
	}

	private void OnRelicEntryPressed(NRelicCollectionEntry entry)
	{
		NGame.Instance.GetInspectRelicScreen().Open(_collection.Relics, entry.relic);
		_collection.SetLastFocusedRelic(entry);
	}

	public List<IReadOnlyList<Control>> GetGridItems()
	{
		List<IReadOnlyList<Control>> list = new List<IReadOnlyList<Control>>();
		if (_subCategories.Any())
		{
			foreach (NRelicCollectionCategory subCategory in _subCategories)
			{
				list.AddRange(subCategory.GetGridItems());
			}
		}
		else
		{
			for (int i = 0; i < _relicsContainer.GetChildren().Count; i += _relicsContainer.Columns)
			{
				list.Add(_relicsContainer.GetChildren().OfType<Control>().Skip(i)
					.Take(_relicsContainer.Columns)
					.ToList());
			}
		}
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.CreateForSubcategory, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LoadIcon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tex", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearRelics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelicEntryPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CreateForSubcategory && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NRelicCollectionCategory>(CreateForSubcategory());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadIcon && args.Count == 1)
		{
			LoadIcon(VariantUtils.ConvertTo<Texture2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearRelics && args.Count == 0)
		{
			ClearRelics();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelicEntryPressed && args.Count == 1)
		{
			OnRelicEntryPressed(VariantUtils.ConvertTo<NRelicCollectionEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.CreateForSubcategory)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.LoadIcon)
		{
			return true;
		}
		if (method == MethodName.ClearRelics)
		{
			return true;
		}
		if (method == MethodName.OnRelicEntryPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._collection)
		{
			_collection = VariantUtils.ConvertTo<NRelicCollection>(in value);
			return true;
		}
		if (name == PropertyName._headerLabel)
		{
			_headerLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicsContainer)
		{
			_relicsContainer = VariantUtils.ConvertTo<GridContainer>(in value);
			return true;
		}
		if (name == PropertyName._spacer)
		{
			_spacer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._collection)
		{
			value = VariantUtils.CreateFrom(in _collection);
			return true;
		}
		if (name == PropertyName._headerLabel)
		{
			value = VariantUtils.CreateFrom(in _headerLabel);
			return true;
		}
		if (name == PropertyName._relicsContainer)
		{
			value = VariantUtils.CreateFrom(in _relicsContainer);
			return true;
		}
		if (name == PropertyName._spacer)
		{
			value = VariantUtils.CreateFrom(in _spacer);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._collection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spacer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._collection, Variant.From(in _collection));
		info.AddProperty(PropertyName._headerLabel, Variant.From(in _headerLabel));
		info.AddProperty(PropertyName._relicsContainer, Variant.From(in _relicsContainer));
		info.AddProperty(PropertyName._spacer, Variant.From(in _spacer));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._collection, out var value))
		{
			_collection = value.As<NRelicCollection>();
		}
		if (info.TryGetProperty(PropertyName._headerLabel, out var value2))
		{
			_headerLabel = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicsContainer, out var value3))
		{
			_relicsContainer = value3.As<GridContainer>();
		}
		if (info.TryGetProperty(PropertyName._spacer, out var value4))
		{
			_spacer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value5))
		{
			_icon = value5.As<TextureRect>();
		}
	}
}
