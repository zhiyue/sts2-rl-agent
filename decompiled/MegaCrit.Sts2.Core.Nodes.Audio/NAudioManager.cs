using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Audio;

[ScriptPath("res://src/Core/Nodes/Audio/NAudioManager.cs")]
public class NAudioManager : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public static readonly StringName PlayLoop = "PlayLoop";

		public static readonly StringName StopLoop = "StopLoop";

		public static readonly StringName SetParam = "SetParam";

		public static readonly StringName StopAllLoops = "StopAllLoops";

		public static readonly StringName PlayOneShot = "PlayOneShot";

		public static readonly StringName PlayMusic = "PlayMusic";

		public static readonly StringName UpdateMusicParameter = "UpdateMusicParameter";

		public static readonly StringName StopMusic = "StopMusic";

		public static readonly StringName SetMasterVol = "SetMasterVol";

		public static readonly StringName SetSfxVol = "SetSfxVol";

		public static readonly StringName SetAmbienceVol = "SetAmbienceVol";

		public static readonly StringName SetBgmVol = "SetBgmVol";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _audioNode = "_audioNode";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private static readonly StringName _setBgmVolume = new StringName("set_bgm_volume");

	private static readonly StringName _setAmbienceVolume = new StringName("set_ambience_volume");

	private static readonly StringName _setSfxVolume = new StringName("set_sfx_volume");

	private static readonly StringName _setMasterVolume = new StringName("set_master_volume");

	private static readonly StringName _stopMusic = new StringName("stop_music");

	private static readonly StringName _playMusic = new StringName("play_music");

	private static readonly StringName _playOneShot = new StringName("play_one_shot");

	private static readonly StringName _stopAllLoops = new StringName("stop_all_loops");

	private static readonly StringName _setParam = new StringName("set_param");

	private static readonly StringName _stopLoop = new StringName("stop_loop");

	private static readonly StringName _playLoop = new StringName("play_loop");

	private static readonly StringName _updateMusicParameterCallback = new StringName("update_music_parameter");

	private Node _audioNode;

	public static NAudioManager? Instance => NGame.Instance?.AudioManager;

	public override void _EnterTree()
	{
		_audioNode = GetNode<Node>("Proxy");
	}

	public void PlayLoop(string path, bool usesLoopParam)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_playLoop, path, usesLoopParam);
		}
	}

	public void StopLoop(string path)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_stopLoop, path);
		}
	}

	public void SetParam(string path, string param, float value)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_setParam, path, param, value);
		}
	}

	public void StopAllLoops()
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_stopAllLoops);
		}
	}

	public void PlayOneShot(string path, System.Collections.Generic.Dictionary<string, float> parameters, float volume = 1f)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		Dictionary dictionary = new Dictionary();
		foreach (KeyValuePair<string, float> parameter in parameters)
		{
			dictionary.Add(parameter.Key, parameter.Value);
		}
		_audioNode.Call(_playOneShot, path, dictionary, volume);
	}

	public void PlayOneShot(string path, float volume = 1f)
	{
		if (!TestMode.IsOn)
		{
			PlayOneShot(path, new System.Collections.Generic.Dictionary<string, float>(), volume);
		}
	}

	public void PlayMusic(string music)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_playMusic, music);
		}
	}

	public void UpdateMusicParameter(string parameter, string value)
	{
		if (!NonInteractiveMode.IsActive)
		{
			_audioNode.Call(_updateMusicParameterCallback, parameter, value);
		}
	}

	public void StopMusic()
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_stopMusic);
		}
	}

	public void SetMasterVol(float volume)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_setMasterVolume, Mathf.Pow(volume, 2f));
		}
	}

	public void SetSfxVol(float volume)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_setSfxVolume, Mathf.Pow(volume, 2f));
		}
	}

	public void SetAmbienceVol(float volume)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_setAmbienceVolume, Mathf.Pow(volume, 2f));
		}
	}

	public void SetBgmVol(float volume)
	{
		if (!TestMode.IsOn)
		{
			_audioNode.Call(_setBgmVolume, Mathf.Pow(volume, 2f));
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayLoop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "usesLoopParam", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopLoop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetParam, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "param", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopAllLoops, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayOneShot, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "path", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "music", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateMusicParameter, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "parameter", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.String, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopMusic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMasterVol, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetSfxVol, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetAmbienceVol, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetBgmVol, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "volume", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayLoop && args.Count == 2)
		{
			PlayLoop(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopLoop && args.Count == 1)
		{
			StopLoop(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetParam && args.Count == 3)
		{
			SetParam(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopAllLoops && args.Count == 0)
		{
			StopAllLoops();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayOneShot && args.Count == 2)
		{
			PlayOneShot(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayMusic && args.Count == 1)
		{
			PlayMusic(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateMusicParameter && args.Count == 2)
		{
			UpdateMusicParameter(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopMusic && args.Count == 0)
		{
			StopMusic();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMasterVol && args.Count == 1)
		{
			SetMasterVol(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSfxVol && args.Count == 1)
		{
			SetSfxVol(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAmbienceVol && args.Count == 1)
		{
			SetAmbienceVol(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetBgmVol && args.Count == 1)
		{
			SetBgmVol(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName.PlayLoop)
		{
			return true;
		}
		if (method == MethodName.StopLoop)
		{
			return true;
		}
		if (method == MethodName.SetParam)
		{
			return true;
		}
		if (method == MethodName.StopAllLoops)
		{
			return true;
		}
		if (method == MethodName.PlayOneShot)
		{
			return true;
		}
		if (method == MethodName.PlayMusic)
		{
			return true;
		}
		if (method == MethodName.UpdateMusicParameter)
		{
			return true;
		}
		if (method == MethodName.StopMusic)
		{
			return true;
		}
		if (method == MethodName.SetMasterVol)
		{
			return true;
		}
		if (method == MethodName.SetSfxVol)
		{
			return true;
		}
		if (method == MethodName.SetAmbienceVol)
		{
			return true;
		}
		if (method == MethodName.SetBgmVol)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._audioNode)
		{
			_audioNode = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._audioNode)
		{
			value = VariantUtils.CreateFrom(in _audioNode);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._audioNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._audioNode, Variant.From(in _audioNode));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._audioNode, out var value))
		{
			_audioNode = value.As<Node>();
		}
	}
}
