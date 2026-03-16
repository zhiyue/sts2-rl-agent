using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NMapNodeSelectVfx.cs")]
public class NMapNodeSelectVfx : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Create = "Create";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _lifeTimer = "_lifeTimer";

		public static readonly StringName _particles = "_particles";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _path = "res://scenes/vfx/map_node_select_vfx.tscn";

	private static readonly string[] _textures = new string[1] { "res://images/vfx/brush_particle_2.png" };

	private double _lifeTimer;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private GpuParticles2D _particles;

	public static IEnumerable<string> AssetPaths => _textures.Append("res://scenes/vfx/map_node_select_vfx.tscn");

	public override void _Ready()
	{
		_particles = GetNode<GpuParticles2D>("Particles");
		_particles.Emitting = true;
		TaskHelper.RunSafely(Play());
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_cancelToken.Cancel();
	}

	public static NMapNodeSelectVfx? Create(float scaleMultiplier)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NMapNodeSelectVfx nMapNodeSelectVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/map_node_select_vfx.tscn").Instantiate<NMapNodeSelectVfx>(PackedScene.GenEditState.Disabled);
		nMapNodeSelectVfx.Scale = Vector2.One * scaleMultiplier;
		return nMapNodeSelectVfx;
	}

	private async Task Play()
	{
		await Task.Delay(1000, _cancelToken.Token);
		if (!_cancelToken.IsCancellationRequested)
		{
			this.QueueFreeSafely();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "scaleMultiplier", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMapNodeSelectVfx>(Create(VariantUtils.ConvertTo<float>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NMapNodeSelectVfx>(Create(VariantUtils.ConvertTo<float>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._lifeTimer)
		{
			_lifeTimer = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._particles)
		{
			_particles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._lifeTimer)
		{
			value = VariantUtils.CreateFrom(in _lifeTimer);
			return true;
		}
		if (name == PropertyName._particles)
		{
			value = VariantUtils.CreateFrom(in _particles);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._lifeTimer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._lifeTimer, Variant.From(in _lifeTimer));
		info.AddProperty(PropertyName._particles, Variant.From(in _particles));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._lifeTimer, out var value))
		{
			_lifeTimer = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._particles, out var value2))
		{
			_particles = value2.As<GpuParticles2D>();
		}
	}
}
