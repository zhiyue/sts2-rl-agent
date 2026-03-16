using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Audio;

[ScriptPath("res://src/Core/Nodes/Audio/NRunMusicController.cs")]
public class NRunMusicController : Node
{
	private enum MusicProgressTrack
	{
		Init,
		Enemy,
		Merchant,
		Rest,
		Unknown,
		Treasure,
		Elite,
		CombatEnd,
		Elite2,
		MerchantEnd
	}

	private enum CampfireState
	{
		On,
		Off
	}

	public new class MethodName : Node.MethodName
	{
		public static readonly StringName GetTrack = "GetTrack";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateMusic = "UpdateMusic";

		public static readonly StringName PlayCustomMusic = "PlayCustomMusic";

		public static readonly StringName UpdateCustomTrack = "UpdateCustomTrack";

		public static readonly StringName StopCustomMusic = "StopCustomMusic";

		public static readonly StringName UpdateAmbience = "UpdateAmbience";

		public static readonly StringName UpdateTrack = "UpdateTrack";

		public static readonly StringName UpdateMusicParameter = "UpdateMusicParameter";

		public static readonly StringName ToggleMerchantTrack = "ToggleMerchantTrack";

		public static readonly StringName TriggerEliteSecondPhase = "TriggerEliteSecondPhase";

		public static readonly StringName TriggerCampfireGoingOut = "TriggerCampfireGoingOut";

		public static readonly StringName StopMusic = "StopMusic";

		public static readonly StringName LoadActBank = "LoadActBank";

		public static readonly StringName UnloadActBanks = "UnloadActBanks";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _proxy = "_proxy";

		public static readonly StringName _currentTrack = "_currentTrack";

		public static readonly StringName _currentAmbience = "_currentAmbience";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _stopAmbience = new StringName("stop_ambience");

	private static readonly StringName _stopMusic = new StringName("stop_music");

	private const string _musicProgressParameter = "Progress";

	private const string _updateGlobalParameterCallback = "update_global_parameter";

	private const string _updateMusicParameterCallback = "update_music_parameter";

	private const string _updateMusicCallback = "update_music";

	private const string _updateAmbienceCallback = "update_ambience";

	private const string _updateCampfireAmbienceCallback = "update_campfire_ambience";

	private const string _updateCustomTrack = "update_custom_track";

	private const string _loadActBanksCallback = "load_act_banks";

	private const string _unloadActBanksCallback = "unload_act_banks";

	private IRunState _runState = NullRunState.Instance;

	private Node _proxy;

	private string _currentTrack;

	private string _currentAmbience;

	public static NRunMusicController? Instance => NRun.Instance?.RunMusicController;

	private MusicProgressTrack GetTrack(RoomType roomType)
	{
		if (roomType.IsCombatRoom() && !CombatManager.Instance.IsInProgress)
		{
			return MusicProgressTrack.CombatEnd;
		}
		switch (roomType)
		{
		case RoomType.Shop:
			return MusicProgressTrack.Merchant;
		case RoomType.RestSite:
			return MusicProgressTrack.Rest;
		case RoomType.Treasure:
			return MusicProgressTrack.Treasure;
		case RoomType.Monster:
			return MusicProgressTrack.Enemy;
		case RoomType.Event:
			if (_runState.CurrentRoom is EventRoom eventRoom && eventRoom.CanonicalEvent is AncientEventModel)
			{
				return MusicProgressTrack.Init;
			}
			return MusicProgressTrack.Unknown;
		case RoomType.Elite:
			return MusicProgressTrack.Elite;
		case RoomType.Boss:
			return MusicProgressTrack.Elite;
		default:
			return MusicProgressTrack.Init;
		}
	}

	public override void _Ready()
	{
		_proxy = GetNode<Node>("Proxy");
	}

	public override void _ExitTree()
	{
		StopMusic();
	}

	public void SetRunState(IRunState runState)
	{
		_runState = runState;
	}

	public void UpdateMusic()
	{
		if (!NonInteractiveMode.IsActive)
		{
			string[] bgMusicOptions = _runState.Act.BgMusicOptions;
			string[] musicBankPaths = _runState.Act.MusicBankPaths;
			int num = new Rng(_runState.Rng.Seed).NextInt(0, bgMusicOptions.Length);
			LoadActBank(musicBankPaths[num]);
			_currentTrack = bgMusicOptions[num];
			_proxy.Call("update_music", _currentTrack);
			_proxy.Call("update_global_parameter", "Progress", 0);
			UpdateAmbience();
		}
	}

	public void PlayCustomMusic(string customMusic)
	{
		if (!NonInteractiveMode.IsActive)
		{
			_proxy.Call(_stopMusic);
			_proxy.Call("update_music", customMusic);
		}
	}

	public void UpdateCustomTrack(string customTrack, float label)
	{
		if (!NonInteractiveMode.IsActive && RunManager.Instance.IsInProgress)
		{
			_proxy.Call("update_custom_track", customTrack, label);
		}
	}

	public void StopCustomMusic()
	{
		if (!NonInteractiveMode.IsActive)
		{
			_proxy.Call(_stopMusic);
			_proxy.Call("update_music", _currentTrack);
			_proxy.Call("update_global_parameter", "Progress", 7);
		}
	}

	public void UpdateAmbience()
	{
		if (!NonInteractiveMode.IsActive)
		{
			string ambientSfx = _runState.Act.AmbientSfx;
			EncounterModel encounterModel = (_runState.CurrentRoom as CombatRoom)?.Encounter;
			if (encounterModel != null && encounterModel.HasAmbientSfx)
			{
				ambientSfx = encounterModel.AmbientSfx;
			}
			if (_currentAmbience != ambientSfx)
			{
				_currentAmbience = ambientSfx;
				_proxy.Call("update_ambience", _currentAmbience);
			}
		}
	}

	public void UpdateTrack()
	{
		if (!NonInteractiveMode.IsActive)
		{
			MusicProgressTrack track = GetTrack(_runState.CurrentRoom.RoomType);
			UpdateTrack("Progress", (float)track);
			if (_runState.CurrentRoom is RestSiteRoom)
			{
				_proxy.Call("update_campfire_ambience", 0);
			}
		}
	}

	private void UpdateTrack(string label, float trackIndex)
	{
		_proxy.Call("update_global_parameter", label, trackIndex);
	}

	public void UpdateMusicParameter(string label, float trackIndex)
	{
		if (!NonInteractiveMode.IsActive)
		{
			_proxy.Call("update_music_parameter", label, trackIndex);
		}
	}

	public void ToggleMerchantTrack()
	{
		if (!NonInteractiveMode.IsActive && _runState.CurrentRoom != null)
		{
			if (_runState.CurrentRoom.RoomType != RoomType.Shop)
			{
				throw new InvalidOperationException("You can only trigger the merchant transition in a merchant room");
			}
			NMapScreen? instance = NMapScreen.Instance;
			MusicProgressTrack musicProgressTrack = ((instance != null && instance.IsVisible()) ? MusicProgressTrack.MerchantEnd : MusicProgressTrack.Merchant);
			_proxy.Call("update_global_parameter", "Progress", (int)musicProgressTrack);
		}
	}

	public void TriggerEliteSecondPhase()
	{
		if (!NonInteractiveMode.IsActive)
		{
			if (_runState.CurrentRoom.RoomType != RoomType.Elite)
			{
				throw new InvalidOperationException("You can only trigger the elite transition in an elite room");
			}
			_proxy.Call("update_global_parameter", "Progress", 8);
		}
	}

	public void TriggerCampfireGoingOut()
	{
		if (!NonInteractiveMode.IsActive)
		{
			if (_runState.CurrentRoom.RoomType != RoomType.RestSite)
			{
				throw new InvalidOperationException("You can only trigger the rest site transition in a rest site room");
			}
			_proxy.Call("update_campfire_ambience", 1);
		}
	}

	public void StopMusic()
	{
		if (!NonInteractiveMode.IsActive)
		{
			_proxy.Call(_stopMusic);
			_proxy.Call(_stopAmbience);
			UnloadActBanks();
		}
	}

	private void LoadActBank(string bankPath)
	{
		Godot.Collections.Array array = new Godot.Collections.Array { bankPath };
		_proxy.Call("load_act_banks", array);
	}

	private void UnloadActBanks()
	{
		_proxy.Call("unload_act_banks");
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(17);
		list.Add(new MethodInfo(MethodName.GetTrack, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "roomType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayCustomMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "customMusic", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCustomTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "customTrack", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "label", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopCustomMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateAmbience, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "label", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "trackIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateMusicParameter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "label", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "trackIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleMerchantTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TriggerEliteSecondPhase, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TriggerCampfireGoingOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LoadActBank, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "bankPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UnloadActBanks, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.GetTrack && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<MusicProgressTrack>(GetTrack(VariantUtils.ConvertTo<RoomType>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateMusic && args.Count == 0)
		{
			UpdateMusic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayCustomMusic && args.Count == 1)
		{
			PlayCustomMusic(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCustomTrack && args.Count == 2)
		{
			UpdateCustomTrack(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopCustomMusic && args.Count == 0)
		{
			StopCustomMusic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateAmbience && args.Count == 0)
		{
			UpdateAmbience();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTrack && args.Count == 0)
		{
			UpdateTrack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTrack && args.Count == 2)
		{
			UpdateTrack(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateMusicParameter && args.Count == 2)
		{
			UpdateMusicParameter(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleMerchantTrack && args.Count == 0)
		{
			ToggleMerchantTrack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TriggerEliteSecondPhase && args.Count == 0)
		{
			TriggerEliteSecondPhase();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TriggerCampfireGoingOut && args.Count == 0)
		{
			TriggerCampfireGoingOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopMusic && args.Count == 0)
		{
			StopMusic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LoadActBank && args.Count == 1)
		{
			LoadActBank(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnloadActBanks && args.Count == 0)
		{
			UnloadActBanks();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.GetTrack)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.UpdateMusic)
		{
			return true;
		}
		if (method == MethodName.PlayCustomMusic)
		{
			return true;
		}
		if (method == MethodName.UpdateCustomTrack)
		{
			return true;
		}
		if (method == MethodName.StopCustomMusic)
		{
			return true;
		}
		if (method == MethodName.UpdateAmbience)
		{
			return true;
		}
		if (method == MethodName.UpdateTrack)
		{
			return true;
		}
		if (method == MethodName.UpdateMusicParameter)
		{
			return true;
		}
		if (method == MethodName.ToggleMerchantTrack)
		{
			return true;
		}
		if (method == MethodName.TriggerEliteSecondPhase)
		{
			return true;
		}
		if (method == MethodName.TriggerCampfireGoingOut)
		{
			return true;
		}
		if (method == MethodName.StopMusic)
		{
			return true;
		}
		if (method == MethodName.LoadActBank)
		{
			return true;
		}
		if (method == MethodName.UnloadActBanks)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._proxy)
		{
			_proxy = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._currentTrack)
		{
			_currentTrack = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		if (name == PropertyName._currentAmbience)
		{
			_currentAmbience = VariantUtils.ConvertTo<string>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._proxy)
		{
			value = VariantUtils.CreateFrom(in _proxy);
			return true;
		}
		if (name == PropertyName._currentTrack)
		{
			value = VariantUtils.CreateFrom(in _currentTrack);
			return true;
		}
		if (name == PropertyName._currentAmbience)
		{
			value = VariantUtils.CreateFrom(in _currentAmbience);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proxy, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._currentTrack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName._currentAmbience, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._proxy, Variant.From(in _proxy));
		info.AddProperty(PropertyName._currentTrack, Variant.From(in _currentTrack));
		info.AddProperty(PropertyName._currentAmbience, Variant.From(in _currentAmbience));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._proxy, out var value))
		{
			_proxy = value.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._currentTrack, out var value2))
		{
			_currentTrack = value2.As<string>();
		}
		if (info.TryGetProperty(PropertyName._currentAmbience, out var value3))
		{
			_currentAmbience = value3.As<string>();
		}
	}
}
