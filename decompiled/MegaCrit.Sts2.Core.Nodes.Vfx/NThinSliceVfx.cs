using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NThinSliceVfx.cs")]
public class NThinSliceVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SetColor' method.
		/// </summary>
		public static readonly StringName SetColor = "SetColor";

		/// <summary>
		/// Cached name for the 'GenerateSpawnPosition' method.
		/// </summary>
		public static readonly StringName GenerateSpawnPosition = "GenerateSpawnPosition";

		/// <summary>
		/// Cached name for the 'GetAngle' method.
		/// </summary>
		public static readonly StringName GetAngle = "GetAngle";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_slash' field.
		/// </summary>
		public static readonly StringName _slash = "_slash";

		/// <summary>
		/// Cached name for the '_sparkle' field.
		/// </summary>
		public static readonly StringName _sparkle = "_sparkle";

		/// <summary>
		/// Cached name for the '_creatureCenter' field.
		/// </summary>
		public static readonly StringName _creatureCenter = "_creatureCenter";

		/// <summary>
		/// Cached name for the '_vfxColor' field.
		/// </summary>
		public static readonly StringName _vfxColor = "_vfxColor";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _scenePath = "res://scenes/vfx/thin_slice_vfx.tscn";

	private CancellationTokenSource? _cts;

	private GpuParticles2D _slash;

	private GpuParticles2D _sparkle;

	private Vector2 _creatureCenter;

	private VfxColor _vfxColor;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/vfx/thin_slice_vfx.tscn");

	/// <summary>
	/// Thin Slice vfx will intersect the given position. Pass in the creature vfx center position!
	/// </summary>
	/// <param name="target"></param>
	/// <param name="vfxColor"></param>
	/// <returns></returns>
	public static NThinSliceVfx? Create(Creature? target, VfxColor vfxColor = VfxColor.Cyan)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature == null)
		{
			return null;
		}
		Vector2 vfxSpawnPosition = nCreature.VfxSpawnPosition;
		NThinSliceVfx nThinSliceVfx = PreloadManager.Cache.GetScene("res://scenes/vfx/thin_slice_vfx.tscn").Instantiate<NThinSliceVfx>(PackedScene.GenEditState.Disabled);
		nThinSliceVfx._vfxColor = vfxColor;
		Vector2 vector = new Vector2(Rng.Chaotic.NextFloat(-50f, 50f), Rng.Chaotic.NextFloat(-50f, 50f));
		nThinSliceVfx._creatureCenter = vfxSpawnPosition + vector;
		return nThinSliceVfx;
	}

	public override void _Ready()
	{
		_slash = GetNode<GpuParticles2D>("Slash");
		_slash.GlobalPosition = GenerateSpawnPosition();
		_slash.Rotation = GetAngle();
		_slash.Emitting = true;
		_sparkle = _slash.GetNode<GpuParticles2D>("Sparkle");
		_sparkle.GlobalPosition = _creatureCenter;
		_sparkle.Emitting = true;
		SetColor();
		TaskHelper.RunSafely(SelfDestruct());
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	private void SetColor()
	{
		ParticleProcessMaterial particleProcessMaterial = (ParticleProcessMaterial)_slash.GetProcessMaterial();
		switch (_vfxColor)
		{
		case VfxColor.Red:
			particleProcessMaterial.Color = new Color("FF9900");
			break;
		case VfxColor.White:
			particleProcessMaterial.Color = Colors.White;
			break;
		case VfxColor.Cyan:
			particleProcessMaterial.Color = new Color("C4FFE6");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case VfxColor.Green:
		case VfxColor.Blue:
		case VfxColor.Purple:
		case VfxColor.Black:
			break;
		}
	}

	/// <summary>
	/// Spawns our slash particle in a random position around the given centerPoint
	/// </summary>
	private Vector2 GenerateSpawnPosition()
	{
		float s = Rng.Chaotic.NextFloat(0f, (float)Math.PI * 2f);
		float num = Rng.Chaotic.NextFloat(400f, 500f);
		return new Vector2(_creatureCenter.X + num * Mathf.Cos(s), _creatureCenter.Y + num * Mathf.Sin(s));
	}

	/// <summary>
	/// Returns the angle from one vector to another in radians.
	/// Written because Vector2.AngleTo() was confusing
	/// </summary>
	/// <returns></returns>
	private float GetAngle()
	{
		Vector2 vector = _creatureCenter - _slash.GlobalPosition;
		return Mathf.Atan2(vector.Y, vector.X);
	}

	private async Task SelfDestruct()
	{
		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		await Task.Delay(1000, _cts.Token);
		if (!_cts.IsCancellationRequested)
		{
			this.QueueFreeSafely();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GenerateSpawnPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetAngle, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetColor && args.Count == 0)
		{
			SetColor();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GenerateSpawnPosition && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GenerateSpawnPosition());
			return true;
		}
		if (method == MethodName.GetAngle && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetAngle());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
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
		if (method == MethodName.SetColor)
		{
			return true;
		}
		if (method == MethodName.GenerateSpawnPosition)
		{
			return true;
		}
		if (method == MethodName.GetAngle)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._slash)
		{
			_slash = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._sparkle)
		{
			_sparkle = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._creatureCenter)
		{
			_creatureCenter = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			_vfxColor = VariantUtils.ConvertTo<VfxColor>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._slash)
		{
			value = VariantUtils.CreateFrom(in _slash);
			return true;
		}
		if (name == PropertyName._sparkle)
		{
			value = VariantUtils.CreateFrom(in _sparkle);
			return true;
		}
		if (name == PropertyName._creatureCenter)
		{
			value = VariantUtils.CreateFrom(in _creatureCenter);
			return true;
		}
		if (name == PropertyName._vfxColor)
		{
			value = VariantUtils.CreateFrom(in _vfxColor);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sparkle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._creatureCenter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._slash, Variant.From(in _slash));
		info.AddProperty(PropertyName._sparkle, Variant.From(in _sparkle));
		info.AddProperty(PropertyName._creatureCenter, Variant.From(in _creatureCenter));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._slash, out var value))
		{
			_slash = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._sparkle, out var value2))
		{
			_sparkle = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._creatureCenter, out var value3))
		{
			_creatureCenter = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value4))
		{
			_vfxColor = value4.As<VfxColor>();
		}
	}
}
