using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;
using MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

[ScriptPath("res://src/Core/Nodes/Screens/MainMenu/NCompendiumSubmenu.cs")]
public class NCompendiumSubmenu : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public static readonly StringName OpenCardLibrary = "OpenCardLibrary";

		public static readonly StringName OpenRelicCollection = "OpenRelicCollection";

		public static readonly StringName OpenPotionLab = "OpenPotionLab";

		public static readonly StringName OpenBestiary = "OpenBestiary";

		public static readonly StringName OpenLeaderboards = "OpenLeaderboards";

		public static readonly StringName OpenStatistics = "OpenStatistics";

		public static readonly StringName OpenRunHistory = "OpenRunHistory";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _confirmButton = "_confirmButton";

		public static readonly StringName _cardLibraryButton = "_cardLibraryButton";

		public static readonly StringName _relicCollectionButton = "_relicCollectionButton";

		public static readonly StringName _potionLabButton = "_potionLabButton";

		public static readonly StringName _bestiaryButton = "_bestiaryButton";

		public static readonly StringName _leaderboardsButton = "_leaderboardsButton";

		public static readonly StringName _statisticsButton = "_statisticsButton";

		public static readonly StringName _runHistoryButton = "_runHistoryButton";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/compendium_submenu");

	private NButton _confirmButton;

	private NShortSubmenuButton _cardLibraryButton;

	private NShortSubmenuButton _relicCollectionButton;

	private NShortSubmenuButton _potionLabButton;

	private NShortSubmenuButton _bestiaryButton;

	private NCompendiumBottomButton _leaderboardsButton;

	private NCompendiumBottomButton _statisticsButton;

	private NCompendiumBottomButton _runHistoryButton;

	private IRunState _runState;

	protected override Control InitialFocusedControl => _cardLibraryButton;

	public static NCompendiumSubmenu? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCompendiumSubmenu>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_cardLibraryButton = GetNode<NShortSubmenuButton>("%CardLibraryButton");
		_cardLibraryButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenCardLibrary));
		_cardLibraryButton.SetIconAndLocalization("COMPENDIUM_CARD_LIBRARY");
		_relicCollectionButton = GetNode<NShortSubmenuButton>("%RelicCollectionButton");
		_relicCollectionButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenRelicCollection));
		_relicCollectionButton.SetIconAndLocalization("COMPENDIUM_RELIC_COLLECTION");
		_potionLabButton = GetNode<NShortSubmenuButton>("%PotionLabButton");
		_potionLabButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenPotionLab));
		_potionLabButton.SetIconAndLocalization("COMPENDIUM_POTION_LAB");
		_bestiaryButton = GetNode<NShortSubmenuButton>("%BestiaryButton");
		_bestiaryButton.Disable();
		_bestiaryButton.SetIconAndLocalization("COMPENDIUM_BESTIARY");
		_leaderboardsButton = GetNode<NCompendiumBottomButton>("%LeaderboardsButton");
		_leaderboardsButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenLeaderboards));
		_leaderboardsButton.SetLocalization("LEADERBOARDS");
		_statisticsButton = GetNode<NCompendiumBottomButton>("%StatisticsButton");
		_statisticsButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenStatistics));
		_statisticsButton.SetLocalization("STATISTICS");
		_runHistoryButton = GetNode<NCompendiumBottomButton>("%RunHistoryButton");
		_runHistoryButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OpenRunHistory));
		_runHistoryButton.SetLocalization("RUN_HISTORY");
		int num = 4;
		List<Control> list = new List<Control>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<Control> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = _cardLibraryButton;
		num2++;
		span[num2] = _relicCollectionButton;
		num2++;
		span[num2] = _potionLabButton;
		num2++;
		span[num2] = _bestiaryButton;
		List<Control> list2 = list;
		num2 = 3;
		List<Control> list3 = new List<Control>(num2);
		CollectionsMarshal.SetCount(list3, num2);
		span = CollectionsMarshal.AsSpan(list3);
		num = 0;
		span[num] = _leaderboardsButton;
		num++;
		span[num] = _statisticsButton;
		num++;
		span[num] = _runHistoryButton;
		List<Control> list4 = list3;
		for (int i = 0; i < list2.Count; i++)
		{
			list2[i].FocusNeighborTop = list2[i].GetPath();
			list2[i].FocusNeighborLeft = ((i > 0) ? list2[i - 1].GetPath() : list2[i].GetPath());
			list2[i].FocusNeighborRight = ((i < list2.Count - 1) ? list2[i + 1].GetPath() : list2[i].GetPath());
		}
		for (int j = 0; j < list4.Count; j++)
		{
			list4[j].FocusNeighborBottom = list4[j].GetPath();
			list4[j].FocusNeighborLeft = ((j > 0) ? list4[j - 1].GetPath() : list4[j].GetPath());
			list4[j].FocusNeighborRight = ((j < list4.Count - 1) ? list4[j + 1].GetPath() : list4[j].GetPath());
		}
		list2[0].FocusNeighborBottom = list4[0].GetPath();
		list2[1].FocusNeighborBottom = list4[0].GetPath();
		list2[2].FocusNeighborBottom = list4[1].GetPath();
		list2[3].FocusNeighborBottom = list4[2].GetPath();
		list4[0].FocusNeighborTop = list2[1].GetPath();
		list4[1].FocusNeighborTop = list2[2].GetPath();
		list4[2].FocusNeighborTop = list2[3].GetPath();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_leaderboardsButton.Visible = false;
		_runHistoryButton.Visible = NRunHistory.CanBeShown();
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
	}

	private void OpenCardLibrary(NButton _)
	{
		NCardLibrary submenuType = _stack.GetSubmenuType<NCardLibrary>();
		submenuType.Initialize(_runState);
		_stack.Push(submenuType);
	}

	private void OpenRelicCollection(NButton _)
	{
		_stack.PushSubmenuType<NRelicCollection>();
	}

	private void OpenPotionLab(NButton _)
	{
		_stack.PushSubmenuType<NPotionLab>();
	}

	private void OpenBestiary(NButton _)
	{
		_stack.PushSubmenuType<NBestiary>();
	}

	private void OpenLeaderboards(NButton _)
	{
	}

	private void OpenStatistics(NButton _)
	{
		_stack.PushSubmenuType<NStatsScreen>();
	}

	private void OpenRunHistory(NButton _)
	{
		_stack.PushSubmenuType<NRunHistory>();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenCardLibrary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenRelicCollection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenPotionLab, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenBestiary, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenLeaderboards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenStatistics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenRunHistory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCompendiumSubmenu>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenCardLibrary && args.Count == 1)
		{
			OpenCardLibrary(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenRelicCollection && args.Count == 1)
		{
			OpenRelicCollection(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenPotionLab && args.Count == 1)
		{
			OpenPotionLab(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenBestiary && args.Count == 1)
		{
			OpenBestiary(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenLeaderboards && args.Count == 1)
		{
			OpenLeaderboards(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenStatistics && args.Count == 1)
		{
			OpenStatistics(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenRunHistory && args.Count == 1)
		{
			OpenRunHistory(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCompendiumSubmenu>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OpenCardLibrary)
		{
			return true;
		}
		if (method == MethodName.OpenRelicCollection)
		{
			return true;
		}
		if (method == MethodName.OpenPotionLab)
		{
			return true;
		}
		if (method == MethodName.OpenBestiary)
		{
			return true;
		}
		if (method == MethodName.OpenLeaderboards)
		{
			return true;
		}
		if (method == MethodName.OpenStatistics)
		{
			return true;
		}
		if (method == MethodName.OpenRunHistory)
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
		if (name == PropertyName._cardLibraryButton)
		{
			_cardLibraryButton = VariantUtils.ConvertTo<NShortSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._relicCollectionButton)
		{
			_relicCollectionButton = VariantUtils.ConvertTo<NShortSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._potionLabButton)
		{
			_potionLabButton = VariantUtils.ConvertTo<NShortSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._bestiaryButton)
		{
			_bestiaryButton = VariantUtils.ConvertTo<NShortSubmenuButton>(in value);
			return true;
		}
		if (name == PropertyName._leaderboardsButton)
		{
			_leaderboardsButton = VariantUtils.ConvertTo<NCompendiumBottomButton>(in value);
			return true;
		}
		if (name == PropertyName._statisticsButton)
		{
			_statisticsButton = VariantUtils.ConvertTo<NCompendiumBottomButton>(in value);
			return true;
		}
		if (name == PropertyName._runHistoryButton)
		{
			_runHistoryButton = VariantUtils.ConvertTo<NCompendiumBottomButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._confirmButton)
		{
			value = VariantUtils.CreateFrom(in _confirmButton);
			return true;
		}
		if (name == PropertyName._cardLibraryButton)
		{
			value = VariantUtils.CreateFrom(in _cardLibraryButton);
			return true;
		}
		if (name == PropertyName._relicCollectionButton)
		{
			value = VariantUtils.CreateFrom(in _relicCollectionButton);
			return true;
		}
		if (name == PropertyName._potionLabButton)
		{
			value = VariantUtils.CreateFrom(in _potionLabButton);
			return true;
		}
		if (name == PropertyName._bestiaryButton)
		{
			value = VariantUtils.CreateFrom(in _bestiaryButton);
			return true;
		}
		if (name == PropertyName._leaderboardsButton)
		{
			value = VariantUtils.CreateFrom(in _leaderboardsButton);
			return true;
		}
		if (name == PropertyName._statisticsButton)
		{
			value = VariantUtils.CreateFrom(in _statisticsButton);
			return true;
		}
		if (name == PropertyName._runHistoryButton)
		{
			value = VariantUtils.CreateFrom(in _runHistoryButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._confirmButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardLibraryButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicCollectionButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionLabButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiaryButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leaderboardsButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statisticsButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._runHistoryButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._confirmButton, Variant.From(in _confirmButton));
		info.AddProperty(PropertyName._cardLibraryButton, Variant.From(in _cardLibraryButton));
		info.AddProperty(PropertyName._relicCollectionButton, Variant.From(in _relicCollectionButton));
		info.AddProperty(PropertyName._potionLabButton, Variant.From(in _potionLabButton));
		info.AddProperty(PropertyName._bestiaryButton, Variant.From(in _bestiaryButton));
		info.AddProperty(PropertyName._leaderboardsButton, Variant.From(in _leaderboardsButton));
		info.AddProperty(PropertyName._statisticsButton, Variant.From(in _statisticsButton));
		info.AddProperty(PropertyName._runHistoryButton, Variant.From(in _runHistoryButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._confirmButton, out var value))
		{
			_confirmButton = value.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._cardLibraryButton, out var value2))
		{
			_cardLibraryButton = value2.As<NShortSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._relicCollectionButton, out var value3))
		{
			_relicCollectionButton = value3.As<NShortSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._potionLabButton, out var value4))
		{
			_potionLabButton = value4.As<NShortSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._bestiaryButton, out var value5))
		{
			_bestiaryButton = value5.As<NShortSubmenuButton>();
		}
		if (info.TryGetProperty(PropertyName._leaderboardsButton, out var value6))
		{
			_leaderboardsButton = value6.As<NCompendiumBottomButton>();
		}
		if (info.TryGetProperty(PropertyName._statisticsButton, out var value7))
		{
			_statisticsButton = value7.As<NCompendiumBottomButton>();
		}
		if (info.TryGetProperty(PropertyName._runHistoryButton, out var value8))
		{
			_runHistoryButton = value8.As<NCompendiumBottomButton>();
		}
	}
}
