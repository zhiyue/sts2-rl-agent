using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.StatsScreen;

[ScriptPath("res://src/Core/Nodes/Screens/StatsScreen/NStatsScreen.cs")]
public class NStatsScreen : NSubmenu
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
		/// Cached name for the 'OnSubmenuOpened' method.
		/// </summary>
		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		/// <summary>
		/// Cached name for the 'OpenStatsMenu' method.
		/// </summary>
		public static readonly StringName OpenStatsMenu = "OpenStatsMenu";

		/// <summary>
		/// Cached name for the 'OpenAchievementsMenu' method.
		/// </summary>
		public static readonly StringName OpenAchievementsMenu = "OpenAchievementsMenu";
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
		/// Cached name for the '_statsTabManager' field.
		/// </summary>
		public static readonly StringName _statsTabManager = "_statsTabManager";

		/// <summary>
		/// Cached name for the '_statsTab' field.
		/// </summary>
		public static readonly StringName _statsTab = "_statsTab";

		/// <summary>
		/// Cached name for the '_achievementsTab' field.
		/// </summary>
		public static readonly StringName _achievementsTab = "_achievementsTab";

		/// <summary>
		/// Cached name for the '_statsGrid' field.
		/// </summary>
		public static readonly StringName _statsGrid = "_statsGrid";

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

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/stats_screen/stats_screen");

	private NStatsTabManager _statsTabManager;

	private NSettingsTab _statsTab;

	private NSettingsTab _achievementsTab;

	private NGeneralStatsGrid _statsGrid;

	private Tween? _screenTween;

	public static string[] AssetPaths
	{
		get
		{
			string scenePath = _scenePath;
			string[] assetPaths = NGeneralStatsGrid.AssetPaths;
			int num = 0;
			string[] array = new string[1 + assetPaths.Length];
			array[num] = scenePath;
			num++;
			ReadOnlySpan<string> readOnlySpan = new ReadOnlySpan<string>(assetPaths);
			readOnlySpan.CopyTo(new Span<string>(array).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			return array;
		}
	}

	protected override Control InitialFocusedControl => _statsGrid.DefaultFocusedControl;

	public static NStatsScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NStatsScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_statsTab = GetNode<NSettingsTab>("%StatsTab");
		_statsTab.SetLabel(new LocString("stats_screen", "TAB_STATS.header").GetFormattedText());
		_achievementsTab = GetNode<NSettingsTab>("%Achievements");
		_achievementsTab.SetLabel(new LocString("stats_screen", "TAB_ACHIEVEMENT.header").GetFormattedText());
		_statsTab.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
		{
			OpenStatsMenu();
		}));
		_statsTabManager = GetNode<NStatsTabManager>("%Tabs");
		_statsGrid = GetNode<NGeneralStatsGrid>("%StatsGrid");
		GetNode<MegaLabel>("%OverallStatsHeader").SetTextAutoSize(new LocString("main_menu_ui", "STATISTICS.OVERALL.title").GetFormattedText());
		GetNode<MegaLabel>("%CharacterStatsHeader").SetTextAutoSize(new LocString("main_menu_ui", "STATISTICS.title").GetFormattedText());
		_achievementsTab.Disable();
	}

	public override void OnSubmenuOpened()
	{
		_screenTween?.Kill();
		_screenTween = CreateTween();
		_screenTween.TweenProperty(this, "modulate:a", 1f, 0.4).From(0f);
		base.Visible = true;
		OpenStatsMenu();
		_statsTabManager.ResetTabs();
	}

	private void OpenStatsMenu()
	{
		_statsGrid.Visible = true;
		_statsGrid.LoadStats();
		ActiveScreenContext.Instance.Update();
	}

	private void OpenAchievementsMenu()
	{
		_statsGrid.Visible = false;
		ActiveScreenContext.Instance.Update();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenStatsMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenAchievementsMenu, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NStatsScreen>(Create());
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
		if (method == MethodName.OpenStatsMenu && args.Count == 0)
		{
			OpenStatsMenu();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenAchievementsMenu && args.Count == 0)
		{
			OpenAchievementsMenu();
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
			ret = VariantUtils.CreateFrom<NStatsScreen>(Create());
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
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OpenStatsMenu)
		{
			return true;
		}
		if (method == MethodName.OpenAchievementsMenu)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._statsTabManager)
		{
			_statsTabManager = VariantUtils.ConvertTo<NStatsTabManager>(in value);
			return true;
		}
		if (name == PropertyName._statsTab)
		{
			_statsTab = VariantUtils.ConvertTo<NSettingsTab>(in value);
			return true;
		}
		if (name == PropertyName._achievementsTab)
		{
			_achievementsTab = VariantUtils.ConvertTo<NSettingsTab>(in value);
			return true;
		}
		if (name == PropertyName._statsGrid)
		{
			_statsGrid = VariantUtils.ConvertTo<NGeneralStatsGrid>(in value);
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
		if (name == PropertyName._statsTabManager)
		{
			value = VariantUtils.CreateFrom(in _statsTabManager);
			return true;
		}
		if (name == PropertyName._statsTab)
		{
			value = VariantUtils.CreateFrom(in _statsTab);
			return true;
		}
		if (name == PropertyName._achievementsTab)
		{
			value = VariantUtils.CreateFrom(in _achievementsTab);
			return true;
		}
		if (name == PropertyName._statsGrid)
		{
			value = VariantUtils.CreateFrom(in _statsGrid);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsTabManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._achievementsTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._statsGrid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._screenTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._statsTabManager, Variant.From(in _statsTabManager));
		info.AddProperty(PropertyName._statsTab, Variant.From(in _statsTab));
		info.AddProperty(PropertyName._achievementsTab, Variant.From(in _achievementsTab));
		info.AddProperty(PropertyName._statsGrid, Variant.From(in _statsGrid));
		info.AddProperty(PropertyName._screenTween, Variant.From(in _screenTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._statsTabManager, out var value))
		{
			_statsTabManager = value.As<NStatsTabManager>();
		}
		if (info.TryGetProperty(PropertyName._statsTab, out var value2))
		{
			_statsTab = value2.As<NSettingsTab>();
		}
		if (info.TryGetProperty(PropertyName._achievementsTab, out var value3))
		{
			_achievementsTab = value3.As<NSettingsTab>();
		}
		if (info.TryGetProperty(PropertyName._statsGrid, out var value4))
		{
			_statsGrid = value4.As<NGeneralStatsGrid>();
		}
		if (info.TryGetProperty(PropertyName._screenTween, out var value5))
		{
			_screenTween = value5.As<Tween>();
		}
	}
}
