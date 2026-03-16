using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunLeaderboardRow.cs")]
public class NDailyRunLeaderboardRow : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName CreateHeader = "CreateHeader";

		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _rank = "_rank";

		public static readonly StringName _name = "_name";

		public static readonly StringName _score = "_score";

		public static readonly StringName _isHeader = "_isHeader";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_leaderboard_row");

	private MegaLabel _rank;

	private MegaRichTextLabel _name;

	private MegaLabel _score;

	private LeaderboardEntry? _entry;

	private bool _isHeader;

	public static NDailyRunLeaderboardRow? Create(LeaderboardEntry entry)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDailyRunLeaderboardRow nDailyRunLeaderboardRow = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDailyRunLeaderboardRow>(PackedScene.GenEditState.Disabled);
		nDailyRunLeaderboardRow._entry = entry;
		return nDailyRunLeaderboardRow;
	}

	public static NDailyRunLeaderboardRow? CreateHeader()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDailyRunLeaderboardRow nDailyRunLeaderboardRow = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDailyRunLeaderboardRow>(PackedScene.GenEditState.Disabled);
		nDailyRunLeaderboardRow._isHeader = true;
		return nDailyRunLeaderboardRow;
	}

	public override void _Ready()
	{
		_rank = GetNode<MegaLabel>("Rank");
		_name = GetNode<MegaRichTextLabel>("Name");
		_score = GetNode<MegaLabel>("Score");
		if (_isHeader)
		{
			_rank.SetTextAutoSize(" " + new LocString("main_menu_ui", "LEADERBOARDS.rankHeader").GetRawText());
			_name.SetTextAutoSize(new LocString("main_menu_ui", "LEADERBOARDS.nameHeader").GetRawText());
			_score.SetTextAutoSize(new LocString("main_menu_ui", "LEADERBOARDS.scoreHeader").GetRawText() + " ");
		}
		else if (_entry != null)
		{
			IEnumerable<string> values = _entry.userIds.Select((ulong id) => PlatformUtil.GetPlayerName(LeaderboardManager.CurrentPlatform, id));
			_rank.SetTextAutoSize($" {_entry.rank + 1}");
			_name.SetTextAutoSize(string.Join(",", values));
			_score.SetTextAutoSize($"{_entry.score} ");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName.CreateHeader, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CreateHeader && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDailyRunLeaderboardRow>(CreateHeader());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CreateHeader && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NDailyRunLeaderboardRow>(CreateHeader());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.CreateHeader)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._rank)
		{
			_rank = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._name)
		{
			_name = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._score)
		{
			_score = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._isHeader)
		{
			_isHeader = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._rank)
		{
			value = VariantUtils.CreateFrom(in _rank);
			return true;
		}
		if (name == PropertyName._name)
		{
			value = VariantUtils.CreateFrom(in _name);
			return true;
		}
		if (name == PropertyName._score)
		{
			value = VariantUtils.CreateFrom(in _score);
			return true;
		}
		if (name == PropertyName._isHeader)
		{
			value = VariantUtils.CreateFrom(in _isHeader);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rank, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._score, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._rank, Variant.From(in _rank));
		info.AddProperty(PropertyName._name, Variant.From(in _name));
		info.AddProperty(PropertyName._score, Variant.From(in _score));
		info.AddProperty(PropertyName._isHeader, Variant.From(in _isHeader));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._rank, out var value))
		{
			_rank = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._name, out var value2))
		{
			_name = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._score, out var value3))
		{
			_score = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._isHeader, out var value4))
		{
			_isHeader = value4.As<bool>();
		}
	}
}
