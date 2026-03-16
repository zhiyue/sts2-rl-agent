using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NMapPointHistory.cs")]
public class NMapPointHistory : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetDeckHistory = "SetDeckHistory";

		public static readonly StringName SetRelicHistory = "SetRelicHistory";

		public static readonly StringName HighlightRelevantEntries = "HighlightRelevantEntries";

		public static readonly StringName UnHighlightEntries = "UnHighlightEntries";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _actContainer = "_actContainer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _actContainer;

	private readonly List<NActHistoryEntry> _actHistories = new List<NActHistoryEntry>();

	private List<NMapPointHistoryEntry> MapHistories => _actHistories.SelectMany((NActHistoryEntry a) => a.Entries).ToList();

	public Control? DefaultFocusedControl => MapHistories.FirstOrDefault();

	public override void _Ready()
	{
		_actContainer = GetNode<Control>("%Acts");
	}

	public void LoadHistory(RunHistory history)
	{
		foreach (Node child in _actContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		_actHistories.Clear();
		int num = 1;
		for (int i = 0; i < history.MapPointHistory.Count; i++)
		{
			LocString title = ModelDb.GetById<ActModel>(history.Acts[i]).Title;
			NActHistoryEntry nActHistoryEntry = NActHistoryEntry.Create(title, history, history.MapPointHistory[i], num);
			_actContainer.AddChildSafely(nActHistoryEntry);
			_actHistories.Add(nActHistoryEntry);
			num += history.MapPointHistory[i].Count;
		}
	}

	public void SetPlayer(RunHistoryPlayer player)
	{
		foreach (NActHistoryEntry actHistory in _actHistories)
		{
			actHistory.SetPlayer(player);
		}
	}

	public void SetDeckHistory(NDeckHistory deckHistory)
	{
		deckHistory.Connect(NDeckHistory.SignalName.Hovered, Callable.From<NDeckHistoryEntry>(HighlightRelevantEntries));
		deckHistory.Connect(NDeckHistory.SignalName.Unhovered, Callable.From((Action<NDeckHistoryEntry>)UnHighlightEntries));
	}

	public void SetRelicHistory(NRelicHistory relicHistory)
	{
		relicHistory.Connect(NRelicHistory.SignalName.Hovered, Callable.From<NRelicBasicHolder>(HighlightRelevantEntries));
		relicHistory.Connect(NRelicHistory.SignalName.Unhovered, Callable.From((Action<NRelicBasicHolder>)UnHighlightEntries));
	}

	private void HighlightRelevantEntries(NDeckHistoryEntry historyEntry)
	{
		foreach (int floorNumber in historyEntry.FloorsAddedToDeck)
		{
			MapHistories.FirstOrDefault((NMapPointHistoryEntry e) => e.FloorNum == floorNumber)?.Highlight();
		}
	}

	private void HighlightRelevantEntries(NRelicBasicHolder holder)
	{
		if (holder.Relic.Model.FloorAddedToDeck > 0)
		{
			MapHistories.FirstOrDefault((NMapPointHistoryEntry e) => e.FloorNum == holder.Relic.Model.FloorAddedToDeck)?.Highlight();
		}
	}

	private void UnHighlightEntries(NRelicBasicHolder _)
	{
		UnHighlightEntries();
	}

	private void UnHighlightEntries(NDeckHistoryEntry _)
	{
		UnHighlightEntries();
	}

	private void UnHighlightEntries()
	{
		foreach (NMapPointHistoryEntry mapHistory in MapHistories)
		{
			mapHistory.Unhighlight();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetDeckHistory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "deckHistory", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetRelicHistory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "relicHistory", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("VBoxContainer"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HighlightRelevantEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "historyEntry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UnHighlightEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UnHighlightEntries, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetDeckHistory && args.Count == 1)
		{
			SetDeckHistory(VariantUtils.ConvertTo<NDeckHistory>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetRelicHistory && args.Count == 1)
		{
			SetRelicHistory(VariantUtils.ConvertTo<NRelicHistory>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HighlightRelevantEntries && args.Count == 1)
		{
			HighlightRelevantEntries(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnHighlightEntries && args.Count == 1)
		{
			UnHighlightEntries(VariantUtils.ConvertTo<NRelicBasicHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnHighlightEntries && args.Count == 0)
		{
			UnHighlightEntries();
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
		if (method == MethodName.SetDeckHistory)
		{
			return true;
		}
		if (method == MethodName.SetRelicHistory)
		{
			return true;
		}
		if (method == MethodName.HighlightRelevantEntries)
		{
			return true;
		}
		if (method == MethodName.UnHighlightEntries)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._actContainer)
		{
			_actContainer = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName._actContainer)
		{
			value = VariantUtils.CreateFrom(in _actContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._actContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._actContainer, Variant.From(in _actContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._actContainer, out var value))
		{
			_actContainer = value.As<Control>();
		}
	}
}
