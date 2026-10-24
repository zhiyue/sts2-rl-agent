using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes.Screens.PotionLab;

[ScriptPath("res://src/Core/Nodes/Screens/PotionLab/NPotionLab.cs")]
public class NPotionLab : NSubmenu
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NSubmenu.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OnSubmenuClosed' method.
		/// </summary>
		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		/// <summary>
		/// Cached name for the 'OnSubmenuShown' method.
		/// </summary>
		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		/// <summary>
		/// Cached name for the 'ClearPotions' method.
		/// </summary>
		public static readonly StringName ClearPotions = "ClearPotions";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NSubmenu.PropertyName
	{
		/// <summary>
		/// Cached name for the 'InitialFocusedControl' property.
		/// </summary>
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		/// <summary>
		/// Cached name for the '_screenContents' field.
		/// </summary>
		public static readonly StringName _screenContents = "_screenContents";

		/// <summary>
		/// Cached name for the '_common' field.
		/// </summary>
		public static readonly StringName _common = "_common";

		/// <summary>
		/// Cached name for the '_uncommon' field.
		/// </summary>
		public static readonly StringName _uncommon = "_uncommon";

		/// <summary>
		/// Cached name for the '_rare' field.
		/// </summary>
		public static readonly StringName _rare = "_rare";

		/// <summary>
		/// Cached name for the '_special' field.
		/// </summary>
		public static readonly StringName _special = "_special";

		/// <summary>
		/// Cached name for the '_screenTween' field.
		/// </summary>
		public static readonly StringName _screenTween = "_screenTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/potion_lab/potion_lab");

	private NScrollableContainer _screenContents;

	private NPotionLabCategory _common;

	private NPotionLabCategory _uncommon;

	private NPotionLabCategory _rare;

	private NPotionLabCategory _special;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private Tween? _screenTween;

	private Task? _loadTask;

	public static string[] AssetPaths => new string[3]
	{
		_scenePath,
		NLabPotionHolder.scenePath,
		NLabPotionHolder.lockedIconPath
	};

	protected override Control? InitialFocusedControl => _common.DefaultFocusedControl;

	public static NPotionLab? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NPotionLab>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_screenContents = GetNode<NScrollableContainer>("%ScreenContents");
		_common = GetNode<NPotionLabCategory>("%Common");
		_uncommon = GetNode<NPotionLabCategory>("%Uncommon");
		_rare = GetNode<NPotionLabCategory>("%Rare");
		_special = GetNode<NPotionLabCategory>("%Special");
	}

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
	}

	public override void OnSubmenuOpened()
	{
		base.OnSubmenuOpened();
		_loadTask = TaskHelper.RunSafely(LoadPotions());
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		_screenTween?.Kill();
		ClearPotions();
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
		_screenTween.TweenProperty(_screenContents, "modulate:a", 1f, 0.25).From(0f);
	}

	private async Task LoadPotions()
	{
		_common.Modulate = Colors.Transparent;
		_uncommon.Modulate = Colors.Transparent;
		_rare.Modulate = Colors.Transparent;
		_special.Modulate = Colors.Transparent;
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		HashSet<PotionModel> allUnlockedPotions = unlockState.Potions.ToHashSet();
		HashSet<PotionModel> seenPotions = SaveManager.Instance.Progress.DiscoveredPotions.Select(ModelDb.GetByIdOrNull<PotionModel>).OfType<PotionModel>().ToHashSet();
		_common.LoadPotions(PotionRarity.Common, new LocString("potion_lab", "COMMON"), seenPotions, unlockState, allUnlockedPotions);
		_uncommon.LoadPotions(PotionRarity.Uncommon, new LocString("potion_lab", "UNCOMMON"), seenPotions, unlockState, allUnlockedPotions);
		_rare.LoadPotions(PotionRarity.Rare, new LocString("potion_lab", "RARE"), seenPotions, unlockState, allUnlockedPotions);
		_special.LoadPotions(PotionRarity.Event, new LocString("potion_lab", "SPECIAL"), seenPotions, unlockState, allUnlockedPotions, PotionRarity.Token);
		List<IReadOnlyList<Control>> list = new List<IReadOnlyList<Control>>();
		list.AddRange(_common.GetGridItems());
		list.AddRange(_uncommon.GetGridItems());
		list.AddRange(_rare.GetGridItems());
		list.AddRange(_special.GetGridItems());
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
		await this.AwaitProcessFrame(_cts.Token);
		_common.Modulate = Colors.White;
		_uncommon.Modulate = Colors.White;
		_rare.Modulate = Colors.White;
		_special.Modulate = Colors.White;
		_screenContents.InstantlyScrollToTop();
		InitialFocusedControl?.TryGrabFocus();
	}

	private void ClearPotions()
	{
		_common.ClearPotions();
		_uncommon.ClearPotions();
		_rare.ClearPotions();
		_special.ClearPotions();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearPotions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NPotionLab>(Create());
			return true;
		}
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
		if (method == MethodName.ClearPotions && args.Count == 0)
		{
			ClearPotions();
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
			ret = VariantUtils.CreateFrom<NPotionLab>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
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
		if (method == MethodName.ClearPotions)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._screenContents)
		{
			_screenContents = VariantUtils.ConvertTo<NScrollableContainer>(in value);
			return true;
		}
		if (name == PropertyName._common)
		{
			_common = VariantUtils.ConvertTo<NPotionLabCategory>(in value);
			return true;
		}
		if (name == PropertyName._uncommon)
		{
			_uncommon = VariantUtils.ConvertTo<NPotionLabCategory>(in value);
			return true;
		}
		if (name == PropertyName._rare)
		{
			_rare = VariantUtils.ConvertTo<NPotionLabCategory>(in value);
			return true;
		}
		if (name == PropertyName._special)
		{
			_special = VariantUtils.ConvertTo<NPotionLabCategory>(in value);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			_screenTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
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
		if (name == PropertyName._special)
		{
			value = VariantUtils.CreateFrom(in _special);
			return true;
		}
		if (name == PropertyName._screenTween)
		{
			value = VariantUtils.CreateFrom(in _screenTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._common, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uncommon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rare, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._special, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._screenContents, Variant.From(in _screenContents));
		info.AddProperty(PropertyName._common, Variant.From(in _common));
		info.AddProperty(PropertyName._uncommon, Variant.From(in _uncommon));
		info.AddProperty(PropertyName._rare, Variant.From(in _rare));
		info.AddProperty(PropertyName._special, Variant.From(in _special));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._screenContents, out var value))
		{
			_screenContents = value.As<NScrollableContainer>();
		}
		if (info.TryGetProperty(PropertyName._common, out var value2))
		{
			_common = value2.As<NPotionLabCategory>();
		}
		if (info.TryGetProperty(PropertyName._uncommon, out var value3))
		{
			_uncommon = value3.As<NPotionLabCategory>();
		}
		if (info.TryGetProperty(PropertyName._rare, out var value4))
		{
			_rare = value4.As<NPotionLabCategory>();
		}
		if (info.TryGetProperty(PropertyName._special, out var value5))
		{
			_special = value5.As<NPotionLabCategory>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value6))
		{
			_screenTween = value6.As<Tween>();
		}
	}
}
