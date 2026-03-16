using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSleepingVfx.cs")]
public class NSleepingVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Play = "Play";

		public static readonly StringName SetFloatingDirection = "SetFloatingDirection";

		public static readonly StringName Stop = "Stop";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _burstParticles = "_burstParticles";

		public static readonly StringName _continuousParticles = "_continuousParticles";

		public static readonly StringName _zParticles = "_zParticles";

		public static readonly StringName _localizedZTexture = "_localizedZTexture";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _direction = new StringName("direction");

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_sleeping");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _burstParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _continuousParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private GpuParticles2D? _zParticles;

	[Export(PropertyHint.None, "")]
	private LocalizedTexture? _localizedZTexture;

	private CancellationTokenSource? _cts;

	public static NSleepingVfx? Create(Vector2 targetTalkPosition, bool goingRight = true)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSleepingVfx nSleepingVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NSleepingVfx>(PackedScene.GenEditState.Disabled);
		nSleepingVfx.GlobalPosition = targetTalkPosition;
		nSleepingVfx.SetFloatingDirection(goingRight);
		if (nSleepingVfx != null && nSleepingVfx._zParticles != null && nSleepingVfx._localizedZTexture != null && nSleepingVfx._localizedZTexture.TryGetTexture(out Texture2D texture))
		{
			nSleepingVfx._zParticles.Texture = texture;
		}
		return nSleepingVfx;
	}

	public override void _Ready()
	{
		Play();
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}

	private void Play()
	{
		foreach (GpuParticles2D burstParticle in _burstParticles)
		{
			burstParticle.Restart();
		}
		foreach (GpuParticles2D continuousParticle in _continuousParticles)
		{
			continuousParticle.Restart();
			continuousParticle.Emitting = true;
		}
	}

	public void SetFloatingDirection(bool goingRight)
	{
		_zParticles.ProcessMaterial = (ParticleProcessMaterial)_zParticles.ProcessMaterial.Duplicate();
		_zParticles.ProcessMaterial.Set(_direction, new Vector3(goingRight ? 0.5f : (-0.5f), -1f, 0f));
	}

	public void Stop()
	{
		TaskHelper.RunSafely(Stopping());
	}

	private async Task Stopping()
	{
		_cts = new CancellationTokenSource();
		foreach (GpuParticles2D continuousParticle in _continuousParticles)
		{
			continuousParticle.Emitting = false;
		}
		await Cmd.Wait(5f, _cts.Token);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetTalkPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "goingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Play, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetFloatingDirection, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "goingRight", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Stop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NSleepingVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
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
		if (method == MethodName.Play && args.Count == 0)
		{
			Play();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetFloatingDirection && args.Count == 1)
		{
			SetFloatingDirection(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Stop && args.Count == 0)
		{
			Stop();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NSleepingVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Play)
		{
			return true;
		}
		if (method == MethodName.SetFloatingDirection)
		{
			return true;
		}
		if (method == MethodName.Stop)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._burstParticles)
		{
			_burstParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._continuousParticles)
		{
			_continuousParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._zParticles)
		{
			_zParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._localizedZTexture)
		{
			_localizedZTexture = VariantUtils.ConvertTo<LocalizedTexture>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._burstParticles)
		{
			value = VariantUtils.CreateFromArray(_burstParticles);
			return true;
		}
		if (name == PropertyName._continuousParticles)
		{
			value = VariantUtils.CreateFromArray(_continuousParticles);
			return true;
		}
		if (name == PropertyName._zParticles)
		{
			value = VariantUtils.CreateFrom(in _zParticles);
			return true;
		}
		if (name == PropertyName._localizedZTexture)
		{
			value = VariantUtils.CreateFrom(in _localizedZTexture);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._burstParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._continuousParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._zParticles, PropertyHint.NodeType, "GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._localizedZTexture, PropertyHint.ResourceType, "LocalizedTexture", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._burstParticles, Variant.CreateFrom(_burstParticles));
		info.AddProperty(PropertyName._continuousParticles, Variant.CreateFrom(_continuousParticles));
		info.AddProperty(PropertyName._zParticles, Variant.From(in _zParticles));
		info.AddProperty(PropertyName._localizedZTexture, Variant.From(in _localizedZTexture));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._burstParticles, out var value))
		{
			_burstParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._continuousParticles, out var value2))
		{
			_continuousParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._zParticles, out var value3))
		{
			_zParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._localizedZTexture, out var value4))
		{
			_localizedZTexture = value4.As<LocalizedTexture>();
		}
	}
}
