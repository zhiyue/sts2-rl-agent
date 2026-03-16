using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NKinPriestGrenadeVfx.cs")]
public class NKinPriestGrenadeVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _cryptoParticles = "_cryptoParticles";

		public static readonly StringName _noiseParticles = "_noiseParticles";

		public static readonly StringName _explosionBase = "_explosionBase";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/monsters/kin_priest_grenade_vfx");

	private GpuParticles2D _cryptoParticles;

	private GpuParticles2D _noiseParticles;

	private GpuParticles2D _explosionBase;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	public static NKinPriestGrenadeVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}
		NKinPriestGrenadeVfx nKinPriestGrenadeVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NKinPriestGrenadeVfx>(PackedScene.GenEditState.Disabled);
		nKinPriestGrenadeVfx.GlobalPosition = creatureNode.GetBottomOfHitbox();
		return nKinPriestGrenadeVfx;
	}

	public override void _Ready()
	{
		_cryptoParticles = GetNode<GpuParticles2D>("CryptoParticles");
		_noiseParticles = GetNode<GpuParticles2D>("NoiseParticles");
		_explosionBase = GetNode<GpuParticles2D>("ExplosionBaseParticle");
		_cryptoParticles.Emitting = false;
		_cryptoParticles.OneShot = true;
		_noiseParticles.Emitting = false;
		_noiseParticles.OneShot = true;
		_explosionBase.Emitting = false;
		_explosionBase.OneShot = true;
		TaskHelper.RunSafely(Play());
	}

	private async Task Play()
	{
		NDebugAudioManager.Instance?.Play("blunt_attack.mp3");
		_noiseParticles.SetEmitting(emitting: true);
		_explosionBase.SetEmitting(emitting: true);
		await Task.Delay(100, _cancelToken.Token);
		_cryptoParticles.SetEmitting(emitting: true);
		await Task.Delay(5000, _cancelToken.Token);
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
		if (name == PropertyName._cryptoParticles)
		{
			_cryptoParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._noiseParticles)
		{
			_noiseParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._explosionBase)
		{
			_explosionBase = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._cryptoParticles)
		{
			value = VariantUtils.CreateFrom(in _cryptoParticles);
			return true;
		}
		if (name == PropertyName._noiseParticles)
		{
			value = VariantUtils.CreateFrom(in _noiseParticles);
			return true;
		}
		if (name == PropertyName._explosionBase)
		{
			value = VariantUtils.CreateFrom(in _explosionBase);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cryptoParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._noiseParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._explosionBase, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cryptoParticles, Variant.From(in _cryptoParticles));
		info.AddProperty(PropertyName._noiseParticles, Variant.From(in _noiseParticles));
		info.AddProperty(PropertyName._explosionBase, Variant.From(in _explosionBase));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cryptoParticles, out var value))
		{
			_cryptoParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._noiseParticles, out var value2))
		{
			_noiseParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._explosionBase, out var value3))
		{
			_explosionBase = value3.As<GpuParticles2D>();
		}
	}
}
