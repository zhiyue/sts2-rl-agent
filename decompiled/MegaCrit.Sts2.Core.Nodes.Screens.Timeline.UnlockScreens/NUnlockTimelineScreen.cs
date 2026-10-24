using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Timeline;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

/// <summary>
/// Unlock screen which isn't really a screen but just an animation.
/// Extends NUnlockScreen so it supports queueing it up like the other unlocks on the TimelineScreen.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockTimelineScreen.cs")]
public class NUnlockTimelineScreen : NUnlockScreen
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NUnlockScreen.MethodName
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
		/// Cached name for the 'Open' method.
		/// </summary>
		public new static readonly StringName Open = "Open";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NUnlockScreen.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NUnlockScreen.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("timeline_screen/unlock_timeline_screen");

	private List<EpochSlotData> _erasToUnlock;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NUnlockTimelineScreen Create()
	{
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NUnlockTimelineScreen>(PackedScene.GenEditState.Disabled);
	}

	/// <summary>
	/// Empty _Ready() function so we don't initialize the Confirm button like other unlock screens.
	/// Better ways to handle but w/e.
	/// </summary>
	public override void _Ready()
	{
	}

	/// <summary>
	/// Set which era slots are unlocked when this "screen" is triggered.
	/// </summary>
	/// <param name="eras"></param>
	public void SetUnlocks(List<EpochSlotData> eras)
	{
		_erasToUnlock = eras.OrderBy((EpochSlotData a) => a.EraPosition).ToList();
	}

	public override void Open()
	{
		base.Open();
		TaskHelper.RunSafely(AnimateExpansion());
	}

	private async Task AnimateExpansion()
	{
		await NTimelineScreen.Instance.HideBackstopAndShowUi(showBackButton: false);
		await NTimelineScreen.Instance.AddEpochSlots(_erasToUnlock, isAnimated: true);
		NTimelineScreen.Instance.ShowHeaderAndActionsUi();
		NTimelineScreen.Instance.SetScreenDraggability();
		await Close();
		NTimelineScreen.Instance.EnableInput();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NUnlockTimelineScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
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
			ret = VariantUtils.CreateFrom<NUnlockTimelineScreen>(Create());
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
		if (method == MethodName.Open)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
