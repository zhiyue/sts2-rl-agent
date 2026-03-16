using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Audio.Debug;

[ScriptPath("res://src/Core/Audio/Debug/NDebugAudioManager.cs")]
public class NDebugAudioManager : Node
{
	private struct PlayingSound
	{
		public int id;

		public AudioStreamPlayer player;

		public Callable callable;
	}

	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Play = "Play";

		public static readonly StringName Stop = "Stop";

		public static readonly StringName StopInternalById = "StopInternalById";

		public static readonly StringName StopInternal = "StopInternal";

		public static readonly StringName SetMasterAudioVolume = "SetMasterAudioVolume";

		public static readonly StringName SetSfxAudioVolume = "SetSfxAudioVolume";

		public static readonly StringName PlayerFinished = "PlayerFinished";

		public static readonly StringName GetRandomPitchScale = "GetRandomPitchScale";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _nextId = "_nextId";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _sfx = new StringName("SFX");

	private static readonly StringName _master = new StringName("Master");

	private List<AudioStreamPlayer> _freeAudioPlayers = new List<AudioStreamPlayer>();

	private readonly List<PlayingSound> _playingSounds = new List<PlayingSound>();

	private int _nextId;

	public static NDebugAudioManager? Instance => NGame.Instance?.DebugAudio;

	public override void _Ready()
	{
		_freeAudioPlayers.AddRange(GetChildren().OfType<AudioStreamPlayer>());
	}

	public int Play(string streamName, float volume = 1f, PitchVariance variance = PitchVariance.None)
	{
		AudioStreamPlayer streamPlayer;
		if (_freeAudioPlayers.Count > 0)
		{
			List<AudioStreamPlayer> freeAudioPlayers = _freeAudioPlayers;
			streamPlayer = freeAudioPlayers[freeAudioPlayers.Count - 1];
			_freeAudioPlayers.RemoveAt(_freeAudioPlayers.Count - 1);
		}
		else
		{
			streamPlayer = new AudioStreamPlayer();
			this.AddChildSafely(streamPlayer);
		}
		AudioStream asset = PreloadManager.Cache.GetAsset<AudioStream>(TmpSfx.GetPath(streamName));
		streamPlayer.Name = "StreamPlayer-" + streamName;
		streamPlayer.Stream = asset;
		streamPlayer.VolumeLinear = volume;
		streamPlayer.PitchScale = GetRandomPitchScale(variance);
		streamPlayer.Bus = _sfx;
		Callable callable = Callable.From(delegate
		{
			PlayerFinished(streamPlayer);
		});
		streamPlayer.Connect(AudioStreamPlayer.SignalName.Finished, callable);
		streamPlayer.Play();
		PlayingSound item = new PlayingSound
		{
			id = _nextId,
			player = streamPlayer,
			callable = callable
		};
		_playingSounds.Add(item);
		_nextId++;
		return item.id;
	}

	public void Stop(int id, float fadeTime = 0.5f)
	{
		int i;
		for (i = 0; i < _playingSounds.Count; i++)
		{
			PlayingSound playingSound = _playingSounds[i];
			if (playingSound.id != id)
			{
				continue;
			}
			if (fadeTime > 0f)
			{
				Tween tween = CreateTween();
				tween.TweenProperty(playingSound.player, "volume_linear", 0f, fadeTime);
				tween.TweenCallback(Callable.From(delegate
				{
					StopInternalById(i);
				}));
			}
			else
			{
				StopInternal(i);
			}
			return;
		}
		Log.Warn($"Tried to stop sound with ID {id} but no sound with that ID was found!");
	}

	private void StopInternalById(int id)
	{
		for (int i = 0; i < _playingSounds.Count; i++)
		{
			if (_playingSounds[i].id == id)
			{
				StopInternal(i);
				break;
			}
		}
	}

	private void StopInternal(int soundIndex)
	{
		PlayingSound playingSound = _playingSounds[soundIndex];
		if (playingSound.player.IsPlaying())
		{
			playingSound.player.Stop();
		}
		playingSound.player.Disconnect(AudioStreamPlayer.SignalName.Finished, playingSound.callable);
		_playingSounds.RemoveAt(soundIndex);
		_freeAudioPlayers.Add(playingSound.player);
	}

	public void SetMasterAudioVolume(float linearVolume)
	{
		AudioServer.Singleton.SetBusVolumeDb(AudioServer.Singleton.GetBusIndex(_master), Mathf.LinearToDb(Mathf.Pow(linearVolume, 2f)));
	}

	public void SetSfxAudioVolume(float linearVolume)
	{
		AudioServer.Singleton.SetBusVolumeDb(AudioServer.Singleton.GetBusIndex(_sfx), Mathf.LinearToDb(Mathf.Pow(linearVolume, 2f)));
	}

	private void PlayerFinished(AudioStreamPlayer player)
	{
		for (int i = 0; i < _playingSounds.Count; i++)
		{
			if (_playingSounds[i].player == player)
			{
				StopInternal(i);
				break;
			}
		}
	}

	private float GetRandomPitchScale(PitchVariance variance)
	{
		float num = variance switch
		{
			PitchVariance.None => 0f, 
			PitchVariance.Small => 0.02f, 
			PitchVariance.Medium => 0.05f, 
			PitchVariance.Large => 0.1f, 
			PitchVariance.TooMuch => 0.2f, 
			_ => throw new ArgumentOutOfRangeException("variance", variance, null), 
		};
		if (num == 0f)
		{
			return 1f;
		}
		return 1f + Rng.Chaotic.NextFloat(0f - num, num);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Play, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "streamName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "variance", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Stop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "fadeTime", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopInternalById, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopInternal, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "soundIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetMasterAudioVolume, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "linearVolume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetSfxAudioVolume, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "linearVolume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayerFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "player", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("AudioStreamPlayer"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetRandomPitchScale, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "variance", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.Play && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<int>(Play(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]), VariantUtils.ConvertTo<PitchVariance>(in args[2])));
			return true;
		}
		if (method == MethodName.Stop && args.Count == 2)
		{
			Stop(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopInternalById && args.Count == 1)
		{
			StopInternalById(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopInternal && args.Count == 1)
		{
			StopInternal(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMasterAudioVolume && args.Count == 1)
		{
			SetMasterAudioVolume(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSfxAudioVolume && args.Count == 1)
		{
			SetSfxAudioVolume(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayerFinished && args.Count == 1)
		{
			PlayerFinished(VariantUtils.ConvertTo<AudioStreamPlayer>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetRandomPitchScale && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetRandomPitchScale(VariantUtils.ConvertTo<PitchVariance>(in args[0])));
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
		if (method == MethodName.Play)
		{
			return true;
		}
		if (method == MethodName.Stop)
		{
			return true;
		}
		if (method == MethodName.StopInternalById)
		{
			return true;
		}
		if (method == MethodName.StopInternal)
		{
			return true;
		}
		if (method == MethodName.SetMasterAudioVolume)
		{
			return true;
		}
		if (method == MethodName.SetSfxAudioVolume)
		{
			return true;
		}
		if (method == MethodName.PlayerFinished)
		{
			return true;
		}
		if (method == MethodName.GetRandomPitchScale)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._nextId)
		{
			_nextId = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._nextId)
		{
			value = VariantUtils.CreateFrom(in _nextId);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._nextId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._nextId, Variant.From(in _nextId));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._nextId, out var value))
		{
			_nextId = value.As<int>();
		}
	}
}
