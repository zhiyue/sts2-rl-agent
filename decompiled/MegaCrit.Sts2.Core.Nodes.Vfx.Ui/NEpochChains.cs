using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Ui;

[ScriptPath("res://src/Core/Nodes/Vfx/Ui/NEpochChains.cs")]
public class NEpochChains : TextureRect
{
	[Signal]
	public delegate void OnAnimationFinishedEventHandler();

	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : TextureRect.MethodName
	{
		/// <summary>
		/// Cached name for the 'UpdateParticles' method.
		/// </summary>
		public static readonly StringName UpdateParticles = "UpdateParticles";

		/// <summary>
		/// Cached name for the 'SetProperties' method.
		/// </summary>
		public static readonly StringName SetProperties = "SetProperties";

		/// <summary>
		/// Cached name for the 'Unlock' method.
		/// </summary>
		public static readonly StringName Unlock = "Unlock";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : TextureRect.PropertyName
	{
		/// <summary>
		/// Cached name for the '_duration' field.
		/// </summary>
		public static readonly StringName _duration = "_duration";

		/// <summary>
		/// Cached name for the '_particles' field.
		/// </summary>
		public static readonly StringName _particles = "_particles";

		/// <summary>
		/// Cached name for the '_endParticles' field.
		/// </summary>
		public static readonly StringName _endParticles = "_endParticles";

		/// <summary>
		/// Cached name for the '_particlesCurve' field.
		/// </summary>
		public static readonly StringName _particlesCurve = "_particlesCurve";

		/// <summary>
		/// Cached name for the '_brightEnabledCurve' field.
		/// </summary>
		public static readonly StringName _brightEnabledCurve = "_brightEnabledCurve";

		/// <summary>
		/// Cached name for the '_erosionEnabledCurve' field.
		/// </summary>
		public static readonly StringName _erosionEnabledCurve = "_erosionEnabledCurve";

		/// <summary>
		/// Cached name for the '_erosionBaseCurve' field.
		/// </summary>
		public static readonly StringName _erosionBaseCurve = "_erosionBaseCurve";

		/// <summary>
		/// Cached name for the '_previousParticleIndex' field.
		/// </summary>
		public static readonly StringName _previousParticleIndex = "_previousParticleIndex";

		/// <summary>
		/// Cached name for the '_asShaderMaterial' field.
		/// </summary>
		public static readonly StringName _asShaderMaterial = "_asShaderMaterial";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : TextureRect.SignalName
	{
		/// <summary>
		/// Cached name for the 'OnAnimationFinished' signal.
		/// </summary>
		public static readonly StringName OnAnimationFinished = "OnAnimationFinished";
	}

	[Export(PropertyHint.None, "")]
	private float _duration = 0.5f;

	[Export(PropertyHint.None, "")]
	private Array<NParticlesContainer>? _particles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _endParticles;

	[Export(PropertyHint.None, "")]
	private Curve? _particlesCurve;

	[Export(PropertyHint.None, "")]
	private Curve _brightEnabledCurve;

	[Export(PropertyHint.None, "")]
	private Curve _erosionEnabledCurve;

	[Export(PropertyHint.None, "")]
	private Curve _erosionBaseCurve;

	private static readonly StringName _brightEnabledString = new StringName("bright_enabled");

	private static readonly StringName _erosionEnabledString = new StringName("erosion_enabled");

	private static readonly StringName _erosionBaseString = new StringName("erosion_base");

	private int _previousParticleIndex = -1;

	private ShaderMaterial? _asShaderMaterial;

	private OnAnimationFinishedEventHandler backing_OnAnimationFinished;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Vfx.Ui.NEpochChains.OnAnimationFinishedEventHandler" />
	public event OnAnimationFinishedEventHandler OnAnimationFinished
	{
		add
		{
			backing_OnAnimationFinished = (OnAnimationFinishedEventHandler)Delegate.Combine(backing_OnAnimationFinished, value);
		}
		remove
		{
			backing_OnAnimationFinished = (OnAnimationFinishedEventHandler)Delegate.Remove(backing_OnAnimationFinished, value);
		}
	}

	private void UpdateParticles(int index)
	{
		if (_previousParticleIndex == index)
		{
			return;
		}
		_previousParticleIndex = index;
		for (int i = 0; i < _particles.Count; i++)
		{
			if (i == index)
			{
				_particles[i].Restart();
			}
		}
	}

	private void SetProperties(float interpolation)
	{
		if (_asShaderMaterial != null)
		{
			float num = _brightEnabledCurve.Sample(interpolation);
			float num2 = _erosionEnabledCurve.Sample(interpolation);
			float num3 = _erosionBaseCurve.Sample(interpolation);
			_asShaderMaterial.SetShaderParameter(_brightEnabledString, num);
			_asShaderMaterial.SetShaderParameter(_erosionEnabledString, num2);
			_asShaderMaterial.SetShaderParameter(_erosionBaseString, num3);
		}
	}

	public void Unlock()
	{
		TaskHelper.RunSafely(Unlocking());
	}

	public async Task Unlocking()
	{
		_previousParticleIndex = -1;
		base.SelfModulate = Colors.White;
		double timer = 0.0;
		Material originalMaterial = base.Material;
		_asShaderMaterial = (ShaderMaterial)originalMaterial.Duplicate(deep: true);
		base.Material = _asShaderMaterial;
		SetProperties(0f);
		while (timer < (double)_duration)
		{
			float num = (float)timer / _duration;
			float s = _particlesCurve.Sample(num);
			SetProperties(num);
			UpdateParticles(Mathf.FloorToInt(s));
			timer += GetProcessDeltaTime();
			await this.AwaitProcessFrame();
		}
		SetProperties(1f);
		base.Material = originalMaterial;
		_asShaderMaterial.Dispose();
		base.SelfModulate = new Color(1f, 1f, 1f, 0f);
		_endParticles.Restart();
		EmitSignal(SignalName.OnAnimationFinished);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.UpdateParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetProperties, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "interpolation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Unlock, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.UpdateParticles && args.Count == 1)
		{
			UpdateParticles(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetProperties && args.Count == 1)
		{
			SetProperties(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Unlock && args.Count == 0)
		{
			Unlock();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.UpdateParticles)
		{
			return true;
		}
		if (method == MethodName.SetProperties)
		{
			return true;
		}
		if (method == MethodName.Unlock)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._duration)
		{
			_duration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._particles)
		{
			_particles = VariantUtils.ConvertToArray<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			_endParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._particlesCurve)
		{
			_particlesCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._brightEnabledCurve)
		{
			_brightEnabledCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._erosionEnabledCurve)
		{
			_erosionEnabledCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._erosionBaseCurve)
		{
			_erosionBaseCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._previousParticleIndex)
		{
			_previousParticleIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._asShaderMaterial)
		{
			_asShaderMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._particles)
		{
			value = VariantUtils.CreateFromArray(_particles);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			value = VariantUtils.CreateFrom(in _endParticles);
			return true;
		}
		if (name == PropertyName._particlesCurve)
		{
			value = VariantUtils.CreateFrom(in _particlesCurve);
			return true;
		}
		if (name == PropertyName._brightEnabledCurve)
		{
			value = VariantUtils.CreateFrom(in _brightEnabledCurve);
			return true;
		}
		if (name == PropertyName._erosionEnabledCurve)
		{
			value = VariantUtils.CreateFrom(in _erosionEnabledCurve);
			return true;
		}
		if (name == PropertyName._erosionBaseCurve)
		{
			value = VariantUtils.CreateFrom(in _erosionBaseCurve);
			return true;
		}
		if (name == PropertyName._previousParticleIndex)
		{
			value = VariantUtils.CreateFrom(in _previousParticleIndex);
			return true;
		}
		if (name == PropertyName._asShaderMaterial)
		{
			value = VariantUtils.CreateFrom(in _asShaderMaterial);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._particles, PropertyHint.TypeString, "24/34:Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._endParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._particlesCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._brightEnabledCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._erosionEnabledCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._erosionBaseCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._previousParticleIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._asShaderMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._particles, Variant.CreateFrom(_particles));
		info.AddProperty(PropertyName._endParticles, Variant.From(in _endParticles));
		info.AddProperty(PropertyName._particlesCurve, Variant.From(in _particlesCurve));
		info.AddProperty(PropertyName._brightEnabledCurve, Variant.From(in _brightEnabledCurve));
		info.AddProperty(PropertyName._erosionEnabledCurve, Variant.From(in _erosionEnabledCurve));
		info.AddProperty(PropertyName._erosionBaseCurve, Variant.From(in _erosionBaseCurve));
		info.AddProperty(PropertyName._previousParticleIndex, Variant.From(in _previousParticleIndex));
		info.AddProperty(PropertyName._asShaderMaterial, Variant.From(in _asShaderMaterial));
		info.AddSignalEventDelegate(SignalName.OnAnimationFinished, backing_OnAnimationFinished);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._duration, out var value))
		{
			_duration = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._particles, out var value2))
		{
			_particles = value2.AsGodotArray<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._endParticles, out var value3))
		{
			_endParticles = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._particlesCurve, out var value4))
		{
			_particlesCurve = value4.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._brightEnabledCurve, out var value5))
		{
			_brightEnabledCurve = value5.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._erosionEnabledCurve, out var value6))
		{
			_erosionEnabledCurve = value6.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._erosionBaseCurve, out var value7))
		{
			_erosionBaseCurve = value7.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._previousParticleIndex, out var value8))
		{
			_previousParticleIndex = value8.As<int>();
		}
		if (info.TryGetProperty(PropertyName._asShaderMaterial, out var value9))
		{
			_asShaderMaterial = value9.As<ShaderMaterial>();
		}
		if (info.TryGetSignalEventDelegate<OnAnimationFinishedEventHandler>(SignalName.OnAnimationFinished, out var value10))
		{
			backing_OnAnimationFinished = value10;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.OnAnimationFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	protected void EmitSignalOnAnimationFinished()
	{
		EmitSignal(SignalName.OnAnimationFinished);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.OnAnimationFinished && args.Count == 0)
		{
			backing_OnAnimationFinished?.Invoke();
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.OnAnimationFinished)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
