using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Leaderboard;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunLeaderboardRow.cs")]
public class NDailyRunLeaderboardRow : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'FormatHoursAndMinutes' method.
		/// </summary>
		public static readonly StringName FormatHoursAndMinutes = "FormatHoursAndMinutes";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_rank' field.
		/// </summary>
		public static readonly StringName _rank = "_rank";

		/// <summary>
		/// Cached name for the '_name' field.
		/// </summary>
		public static readonly StringName _name = "_name";

		/// <summary>
		/// Cached name for the '_floor' field.
		/// </summary>
		public static readonly StringName _floor = "_floor";

		/// <summary>
		/// Cached name for the '_badges' field.
		/// </summary>
		public static readonly StringName _badges = "_badges";

		/// <summary>
		/// Cached name for the '_time' field.
		/// </summary>
		public static readonly StringName _time = "_time";

		/// <summary>
		/// Cached name for the '_isYou' field.
		/// </summary>
		public static readonly StringName _isYou = "_isYou";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/daily_run/daily_run_leaderboard_row");

	private MegaLabel _rank;

	private MegaLabel _name;

	private MegaLabel _floor;

	private MegaLabel _badges;

	private MegaLabel _time;

	private LeaderboardEntry _entry;

	private bool _isYou;

	public static NDailyRunLeaderboardRow? Create(LeaderboardEntry entry, bool isYou)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NDailyRunLeaderboardRow nDailyRunLeaderboardRow = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NDailyRunLeaderboardRow>(PackedScene.GenEditState.Disabled);
		nDailyRunLeaderboardRow._entry = entry;
		nDailyRunLeaderboardRow._isYou = isYou;
		return nDailyRunLeaderboardRow;
	}

	public override void _Ready()
	{
		_rank = GetNode<MegaLabel>("%Rank");
		_floor = GetNode<MegaLabel>("%Floor");
		_name = GetNode<MegaLabel>("%Name");
		_badges = GetNode<MegaLabel>("%Badges");
		_time = GetNode<MegaLabel>("%Time");
		IEnumerable<string> values = _entry.userIds.Select((ulong id) => PlatformUtil.GetPlayerNameRaw(LeaderboardManager.CurrentPlatform, id));
		DecodedDailyScore decodedDailyScore = ScoreUtility.DecodeDailyScore(_entry.score);
		if (!decodedDailyScore.isValid)
		{
			this.QueueFreeSafely();
			return;
		}
		_rank.SetTextAutoSize($"{_entry.rank + 1} ");
		_name.SetTextAutoSize(string.Join(",", values));
		if (_isYou)
		{
			_name.Modulate = StsColors.blue;
		}
		_floor.SetTextAutoSize($"{decodedDailyScore.floors}");
		if (decodedDailyScore.victory == 2)
		{
			GetNode<Control>("%Tick").Visible = true;
		}
		_badges.SetTextAutoSize($"{decodedDailyScore.badges}");
		_time.SetTextAutoSize(FormatHoursAndMinutes(decodedDailyScore.runTime));
	}

	private static string FormatHoursAndMinutes(int value)
	{
		if (value >= 9999)
		{
			return "--:--";
		}
		int value2 = value / 60;
		int value3 = value % 60;
		return $"{value2}:{value3:D2}";
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FormatHoursAndMinutes, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FormatHoursAndMinutes && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatHoursAndMinutes(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.FormatHoursAndMinutes && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(FormatHoursAndMinutes(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.FormatHoursAndMinutes)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
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
			_name = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._floor)
		{
			_floor = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._badges)
		{
			_badges = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._time)
		{
			_time = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._isYou)
		{
			_isYou = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
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
		if (name == PropertyName._floor)
		{
			value = VariantUtils.CreateFrom(in _floor);
			return true;
		}
		if (name == PropertyName._badges)
		{
			value = VariantUtils.CreateFrom(in _badges);
			return true;
		}
		if (name == PropertyName._time)
		{
			value = VariantUtils.CreateFrom(in _time);
			return true;
		}
		if (name == PropertyName._isYou)
		{
			value = VariantUtils.CreateFrom(in _isYou);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rank, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._floor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._badges, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._time, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isYou, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._rank, Variant.From(in _rank));
		info.AddProperty(PropertyName._name, Variant.From(in _name));
		info.AddProperty(PropertyName._floor, Variant.From(in _floor));
		info.AddProperty(PropertyName._badges, Variant.From(in _badges));
		info.AddProperty(PropertyName._time, Variant.From(in _time));
		info.AddProperty(PropertyName._isYou, Variant.From(in _isYou));
	}

	/// <inheritdoc />
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
			_name = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._floor, out var value3))
		{
			_floor = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._badges, out var value4))
		{
			_badges = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._time, out var value5))
		{
			_time = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._isYou, out var value6))
		{
			_isYou = value6.As<bool>();
		}
	}
}
