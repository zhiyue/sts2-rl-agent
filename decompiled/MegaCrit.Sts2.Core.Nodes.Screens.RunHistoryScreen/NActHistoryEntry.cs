using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NActHistoryEntry.cs")]
public class NActHistoryEntry : HBoxContainer
{
	public new class MethodName : HBoxContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : HBoxContainer.PropertyName
	{
		public static readonly StringName _actLabel = "_actLabel";

		public static readonly StringName _baseFloorNum = "_baseFloorNum";
	}

	public new class SignalName : HBoxContainer.SignalName
	{
	}

	private MegaLabel _actLabel;

	private LocString _actName;

	private RunHistory _runHistory;

	private IReadOnlyList<MapPointHistoryEntry> _entries;

	private int _baseFloorNum;

	private static string ScenePath => SceneHelper.GetScenePath("screens/run_history_screen/act_history_entry");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public List<NMapPointHistoryEntry> Entries { get; private set; } = new List<NMapPointHistoryEntry>();

	public override void _Ready()
	{
		_actLabel = GetNode<MegaLabel>("%Title");
		_actLabel.SetTextAutoSize(_actName.GetFormattedText());
		for (int i = 0; i < _entries.Count; i++)
		{
			NMapPointHistoryEntry nMapPointHistoryEntry = NMapPointHistoryEntry.Create(_runHistory, _entries[i], i + _baseFloorNum);
			this.AddChildSafely(nMapPointHistoryEntry);
			Entries.Add(nMapPointHistoryEntry);
		}
	}

	public void SetPlayer(RunHistoryPlayer player)
	{
		foreach (NMapPointHistoryEntry entry in Entries)
		{
			entry.SetPlayer(player);
		}
	}

	public static NActHistoryEntry? Create(LocString actName, RunHistory runHistory, IReadOnlyList<MapPointHistoryEntry> logs, int baseFloorNum)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NActHistoryEntry nActHistoryEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NActHistoryEntry>(PackedScene.GenEditState.Disabled);
		nActHistoryEntry._actName = actName;
		nActHistoryEntry._runHistory = runHistory;
		nActHistoryEntry._entries = logs;
		nActHistoryEntry._baseFloorNum = baseFloorNum;
		return nActHistoryEntry;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._actLabel)
		{
			_actLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._baseFloorNum)
		{
			_baseFloorNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._actLabel)
		{
			value = VariantUtils.CreateFrom(in _actLabel);
			return true;
		}
		if (name == PropertyName._baseFloorNum)
		{
			value = VariantUtils.CreateFrom(in _baseFloorNum);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._baseFloorNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._actLabel, Variant.From(in _actLabel));
		info.AddProperty(PropertyName._baseFloorNum, Variant.From(in _baseFloorNum));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._actLabel, out var value))
		{
			_actLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._baseFloorNum, out var value2))
		{
			_baseFloorNum = value2.As<int>();
		}
	}
}
