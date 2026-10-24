using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

/// <summary>
/// ScreenShake VFX script. Lives as a Node which shakes a specified Control node (_shakeTarget).
/// Multiple effects can be stacked
/// Can be called from anywhere using NGame.Instance.ScreenShake(), Screenpunch(), Rumble(), etc.
/// See CombatRoom for example to attach this effect to other screens.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NScreenShake.cs")]
public class NScreenShake : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'SetTarget' method.
		/// </summary>
		public static readonly StringName SetTarget = "SetTarget";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'Shake' method.
		/// </summary>
		public static readonly StringName Shake = "Shake";

		/// <summary>
		/// Cached name for the 'Rumble' method.
		/// </summary>
		public static readonly StringName Rumble = "Rumble";

		/// <summary>
		/// Cached name for the 'AddTrauma' method.
		/// </summary>
		public static readonly StringName AddTrauma = "AddTrauma";

		/// <summary>
		/// Cached name for the 'ClearTarget' method.
		/// </summary>
		public static readonly StringName ClearTarget = "ClearTarget";

		/// <summary>
		/// Cached name for the 'StopRumble' method.
		/// </summary>
		public static readonly StringName StopRumble = "StopRumble";

		/// <summary>
		/// Cached name for the 'SetMultiplier' method.
		/// </summary>
		public static readonly StringName SetMultiplier = "SetMultiplier";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the 'ShakeTarget' property.
		/// </summary>
		public static readonly StringName ShakeTarget = "ShakeTarget";

		/// <summary>
		/// Cached name for the '_originalTargetPosition' field.
		/// </summary>
		public static readonly StringName _originalTargetPosition = "_originalTargetPosition";

		/// <summary>
		/// Cached name for the '_multiplier' field.
		/// </summary>
		public static readonly StringName _multiplier = "_multiplier";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private Vector2 _originalTargetPosition;

	private ScreenPunchInstance? _shakeInstance;

	private ScreenRumbleInstance? _rumbleInstance;

	private ScreenTraumaRumble _traumaRumble;

	private float _multiplier;

	private readonly Dictionary<ShakeStrength, float> _strength = new Dictionary<ShakeStrength, float>();

	private readonly Dictionary<ShakeDuration, double> _duration = new Dictionary<ShakeDuration, double>();

	public Control? ShakeTarget { get; private set; }

	public override void _Ready()
	{
		_traumaRumble = new ScreenTraumaRumble();
		_strength.Add(ShakeStrength.VeryWeak, 2f);
		_strength.Add(ShakeStrength.Weak, 5f);
		_strength.Add(ShakeStrength.Medium, 20f);
		_strength.Add(ShakeStrength.Strong, 40f);
		_strength.Add(ShakeStrength.TooMuch, 80f);
		_duration.Add(ShakeDuration.Short, 0.3);
		_duration.Add(ShakeDuration.Normal, 0.8);
		_duration.Add(ShakeDuration.Long, 1.2);
		_duration.Add(ShakeDuration.Forever, 999999999.0);
	}

	public void SetTarget(Control targetScreen)
	{
		ShakeTarget = targetScreen;
		_originalTargetPosition = targetScreen.Position;
	}

	public override void _Process(double delta)
	{
		Vector2 vector = Vector2.Zero;
		if (_rumbleInstance != null)
		{
			vector = _rumbleInstance.Update(delta);
			if (_rumbleInstance.IsDone)
			{
				_rumbleInstance = null;
			}
		}
		if (_shakeInstance != null)
		{
			vector = _shakeInstance.Update(delta);
			if (_shakeInstance.IsDone)
			{
				_shakeInstance = null;
			}
		}
		vector += _traumaRumble.Update(delta);
		if (ShakeTarget != null && ShakeTarget.IsValid())
		{
			ShakeTarget.Position = _originalTargetPosition + vector;
		}
	}

	public void Shake(ShakeStrength strength, ShakeDuration duration, float degAngle)
	{
		if (ShakeTarget == null)
		{
			Log.Error("Missing screenShake target!");
		}
		else
		{
			_shakeInstance = new ScreenPunchInstance(_strength[strength] * _multiplier, _duration[duration], degAngle);
		}
	}

	public void Rumble(ShakeStrength strength, ShakeDuration duration, RumbleStyle style)
	{
		if (ShakeTarget == null)
		{
			Log.Error("Missing screenShake target!");
		}
		else
		{
			_rumbleInstance = new ScreenRumbleInstance(_strength[strength] * _multiplier, _duration[duration], 1f, style);
		}
	}

	public void AddTrauma(ShakeStrength strength)
	{
		_traumaRumble.AddTrauma(strength);
	}

	public void ClearTarget()
	{
		ShakeTarget = null;
		_shakeInstance = null;
		_rumbleInstance = null;
	}

	private void StopRumble()
	{
		_rumbleInstance = null;
	}

	public void SetMultiplier(float multiplier)
	{
		_multiplier = multiplier;
		_traumaRumble.SetMultiplier(multiplier);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "targetScreen", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Shake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "degAngle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Rumble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "style", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddTrauma, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearTarget, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StopRumble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMultiplier, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "multiplier", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetTarget && args.Count == 1)
		{
			SetTarget(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Shake && args.Count == 3)
		{
			Shake(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]), VariantUtils.ConvertTo<float>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Rumble && args.Count == 3)
		{
			Rumble(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]), VariantUtils.ConvertTo<RumbleStyle>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddTrauma && args.Count == 1)
		{
			AddTrauma(VariantUtils.ConvertTo<ShakeStrength>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearTarget && args.Count == 0)
		{
			ClearTarget();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopRumble && args.Count == 0)
		{
			StopRumble();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMultiplier && args.Count == 1)
		{
			SetMultiplier(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName.SetTarget)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.Shake)
		{
			return true;
		}
		if (method == MethodName.Rumble)
		{
			return true;
		}
		if (method == MethodName.AddTrauma)
		{
			return true;
		}
		if (method == MethodName.ClearTarget)
		{
			return true;
		}
		if (method == MethodName.StopRumble)
		{
			return true;
		}
		if (method == MethodName.SetMultiplier)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ShakeTarget)
		{
			ShakeTarget = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._originalTargetPosition)
		{
			_originalTargetPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._multiplier)
		{
			_multiplier = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ShakeTarget)
		{
			value = VariantUtils.CreateFrom<Control>(ShakeTarget);
			return true;
		}
		if (name == PropertyName._originalTargetPosition)
		{
			value = VariantUtils.CreateFrom(in _originalTargetPosition);
			return true;
		}
		if (name == PropertyName._multiplier)
		{
			value = VariantUtils.CreateFrom(in _multiplier);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalTargetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._multiplier, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ShakeTarget, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ShakeTarget, Variant.From<Control>(ShakeTarget));
		info.AddProperty(PropertyName._originalTargetPosition, Variant.From(in _originalTargetPosition));
		info.AddProperty(PropertyName._multiplier, Variant.From(in _multiplier));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ShakeTarget, out var value))
		{
			ShakeTarget = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._originalTargetPosition, out var value2))
		{
			_originalTargetPosition = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._multiplier, out var value3))
		{
			_multiplier = value3.As<float>();
		}
	}
}
