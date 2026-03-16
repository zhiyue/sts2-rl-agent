using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RelicCollection;

[ScriptPath("res://src/Core/Nodes/Screens/RelicCollection/NRelicCollection.cs")]
public class NRelicCollection : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public static readonly StringName ClearRelics = "ClearRelics";

		public static readonly StringName SetLastFocusedRelic = "SetLastFocusedRelic";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _screenContents = "_screenContents";

		public static readonly StringName _starter = "_starter";

		public static readonly StringName _common = "_common";

		public static readonly StringName _uncommon = "_uncommon";

		public static readonly StringName _rare = "_rare";

		public static readonly StringName _shop = "_shop";

		public static readonly StringName _ancient = "_ancient";

		public static readonly StringName _event = "_event";

		public static readonly StringName _screenTween = "_screenTween";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/relic_collection/relic_collection");

	private NScrollableContainer _screenContents;

	private NRelicCollectionCategory _starter;

	private NRelicCollectionCategory _common;

	private NRelicCollectionCategory _uncommon;

	private NRelicCollectionCategory _rare;

	private NRelicCollectionCategory _shop;

	private NRelicCollectionCategory _ancient;

	private NRelicCollectionCategory _event;

	private Tween? _screenTween;

	private Task? _loadTask;

	private readonly List<RelicModel> _relics = new List<RelicModel>();

	public IReadOnlyList<RelicModel> Relics => _relics;

	public static string[] AssetPaths => new string[4]
	{
		_scenePath,
		NRelicCollectionEntry.scenePath,
		NRelicCollectionCategory.scenePath,
		NRelicCollectionEntry.lockedIconPath
	};

	protected override Control? InitialFocusedControl => _starter.DefaultFocusedControl;

	public static NRelicCollection? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRelicCollection>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_screenContents = GetNode<NScrollableContainer>("%ScreenContents");
		_starter = GetNode<NRelicCollectionCategory>("%Starter");
		_common = GetNode<NRelicCollectionCategory>("%Common");
		_uncommon = GetNode<NRelicCollectionCategory>("%Uncommon");
		_rare = GetNode<NRelicCollectionCategory>("%Rare");
		_shop = GetNode<NRelicCollectionCategory>("%Shop");
		_ancient = GetNode<NRelicCollectionCategory>("%Ancient");
		_event = GetNode<NRelicCollectionCategory>("%Event");
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_relics.Clear();
		_loadTask = TaskHelper.RunSafely(LoadRelics());
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		_screenTween?.Kill();
		ClearRelics();
	}

	protected override void OnSubmenuShown()
	{
		base.OnSubmenuShown();
		TaskHelper.RunSafely(TweenAfterLoading());
	}

	private async Task TweenAfterLoading()
	{
		_screenContents.Modulate = new Color(1f, 1f, 1f, 0f);
		if (_loadTask != null)
		{
			await _loadTask;
		}
		_screenTween?.Kill();
		_screenTween = CreateTween();
		_screenTween.TweenProperty(_screenContents, "modulate:a", 1f, 0.4).From(0f);
	}

	private async Task LoadRelics()
	{
		_starter.Modulate = Colors.Transparent;
		_common.Modulate = Colors.Transparent;
		_uncommon.Modulate = Colors.Transparent;
		_rare.Modulate = Colors.Transparent;
		_shop.Modulate = Colors.Transparent;
		_ancient.Modulate = Colors.Transparent;
		_event.Modulate = Colors.Transparent;
		HashSet<RelicModel> seenRelics = SaveManager.Instance.Progress.DiscoveredRelics.Select(ModelDb.GetByIdOrNull<RelicModel>).OfType<RelicModel>().ToHashSet();
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		HashSet<RelicModel> allUnlockedRelics = unlockState.Relics.ToHashSet();
		_starter.LoadRelics(RelicRarity.Starter, this, new LocString("relic_collection", "STARTER"), seenRelics, unlockState, allUnlockedRelics);
		_common.LoadRelics(RelicRarity.Common, this, new LocString("relic_collection", "COMMON"), seenRelics, unlockState, allUnlockedRelics);
		_uncommon.LoadRelics(RelicRarity.Uncommon, this, new LocString("relic_collection", "UNCOMMON"), seenRelics, unlockState, allUnlockedRelics);
		_rare.LoadRelics(RelicRarity.Rare, this, new LocString("relic_collection", "RARE"), seenRelics, unlockState, allUnlockedRelics);
		_shop.LoadRelics(RelicRarity.Shop, this, new LocString("relic_collection", "SHOP"), seenRelics, unlockState, allUnlockedRelics);
		_ancient.LoadRelics(RelicRarity.Ancient, this, new LocString("relic_collection", "ANCIENT"), seenRelics, unlockState, allUnlockedRelics);
		_event.LoadRelics(RelicRarity.Event, this, new LocString("relic_collection", "EVENT"), seenRelics, unlockState, allUnlockedRelics);
		List<IReadOnlyList<Control>> list = new List<IReadOnlyList<Control>>();
		list.AddRange(_starter.GetGridItems());
		list.AddRange(_common.GetGridItems());
		list.AddRange(_uncommon.GetGridItems());
		list.AddRange(_rare.GetGridItems());
		list.AddRange(_shop.GetGridItems());
		list.AddRange(_ancient.GetGridItems());
		list.AddRange(_event.GetGridItems());
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Count; j++)
			{
				Control control = list[i][j];
				NodePath path;
				if (j <= 0)
				{
					IReadOnlyList<Control> readOnlyList = list[i];
					path = readOnlyList[readOnlyList.Count - 1].GetPath();
				}
				else
				{
					path = list[i][j - 1].GetPath();
				}
				control.FocusNeighborLeft = path;
				control.FocusNeighborRight = ((j < list[i].Count - 1) ? list[i][j + 1].GetPath() : list[i][0].GetPath());
				if (i > 0)
				{
					control.FocusNeighborTop = ((j < list[i - 1].Count) ? list[i - 1][j].GetPath() : list[i - 1][list[i - 1].Count - 1].GetPath());
				}
				else
				{
					control.FocusNeighborTop = list[i][j].GetPath();
				}
				if (i < list.Count - 1)
				{
					control.FocusNeighborBottom = ((j < list[i + 1].Count) ? list[i + 1][j].GetPath() : list[i + 1][list[i + 1].Count - 1].GetPath());
				}
				else
				{
					control.FocusNeighborBottom = list[i][j].GetPath();
				}
			}
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_starter.Modulate = Colors.White;
		_common.Modulate = Colors.White;
		_uncommon.Modulate = Colors.White;
		_rare.Modulate = Colors.White;
		_shop.Modulate = Colors.White;
		_ancient.Modulate = Colors.White;
		_event.Modulate = Colors.White;
		_screenContents.InstantlyScrollToTop();
		InitialFocusedControl?.TryGrabFocus();
	}

	public void AddRelics(IEnumerable<RelicModel> relics)
	{
		_relics.AddRange(relics);
	}

	private void ClearRelics()
	{
		_starter.ClearRelics();
		_common.ClearRelics();
		_uncommon.ClearRelics();
		_rare.ClearRelics();
		_shop.ClearRelics();
		_ancient.ClearRelics();
		_event.ClearRelics();
		_relics.Clear();
	}

	public void SetLastFocusedRelic(NRelicCollectionEntry relic)
	{
		_lastFocusedControl = relic;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearRelics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetLastFocusedRelic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "relic", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NRelicCollection>(Create());
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
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearRelics && args.Count == 0)
		{
			ClearRelics();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetLastFocusedRelic && args.Count == 1)
		{
			SetLastFocusedRelic(VariantUtils.ConvertTo<NRelicCollectionEntry>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NRelicCollection>(Create());
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
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.ClearRelics)
		{
			return true;
		}
		if (method == MethodName.SetLastFocusedRelic)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._screenContents)
		{
			_screenContents = VariantUtils.ConvertTo<NScrollableContainer>(in value);
			return true;
		}
		if (name == PropertyName._starter)
		{
			_starter = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._common)
		{
			_common = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._uncommon)
		{
			_uncommon = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._rare)
		{
			_rare = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._shop)
		{
			_shop = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._ancient)
		{
			_ancient = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._event)
		{
			_event = VariantUtils.ConvertTo<NRelicCollectionCategory>(in value);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			_screenTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._screenContents)
		{
			value = VariantUtils.CreateFrom(in _screenContents);
			return true;
		}
		if (name == PropertyName._starter)
		{
			value = VariantUtils.CreateFrom(in _starter);
			return true;
		}
		if (name == PropertyName._common)
		{
			value = VariantUtils.CreateFrom(in _common);
			return true;
		}
		if (name == PropertyName._uncommon)
		{
			value = VariantUtils.CreateFrom(in _uncommon);
			return true;
		}
		if (name == PropertyName._rare)
		{
			value = VariantUtils.CreateFrom(in _rare);
			return true;
		}
		if (name == PropertyName._shop)
		{
			value = VariantUtils.CreateFrom(in _shop);
			return true;
		}
		if (name == PropertyName._ancient)
		{
			value = VariantUtils.CreateFrom(in _ancient);
			return true;
		}
		if (name == PropertyName._event)
		{
			value = VariantUtils.CreateFrom(in _event);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			value = VariantUtils.CreateFrom(in _screenTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._common, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uncommon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rare, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancient, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._event, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._screenContents, Variant.From(in _screenContents));
		info.AddProperty(PropertyName._starter, Variant.From(in _starter));
		info.AddProperty(PropertyName._common, Variant.From(in _common));
		info.AddProperty(PropertyName._uncommon, Variant.From(in _uncommon));
		info.AddProperty(PropertyName._rare, Variant.From(in _rare));
		info.AddProperty(PropertyName._shop, Variant.From(in _shop));
		info.AddProperty(PropertyName._ancient, Variant.From(in _ancient));
		info.AddProperty(PropertyName._event, Variant.From(in _event));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._screenContents, out var value))
		{
			_screenContents = value.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._starter, out var value2))
		{
			_starter = value2.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._common, out var value3))
		{
			_common = value3.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._uncommon, out var value4))
		{
			_uncommon = value4.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._rare, out var value5))
		{
			_rare = value5.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._shop, out var value6))
		{
			_shop = value6.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._ancient, out var value7))
		{
			_ancient = value7.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._event, out var value8))
		{
			_event = value8.As<NRelicCollectionCategory>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value9))
		{
			_screenTween = value9.As<Tween>();
		}
	}
}
