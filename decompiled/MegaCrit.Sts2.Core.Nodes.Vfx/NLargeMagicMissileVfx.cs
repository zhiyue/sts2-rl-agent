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

[ScriptPath("res://src/Core/Nodes/Vfx/NLargeMagicMissileVfx.cs")]
public class NLargeMagicMissileVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName GetProjectileDirection = "GetProjectileDirection";

		public static readonly StringName GetTopPosition = "GetTopPosition";

		public static readonly StringName Initialize = "Initialize";

		public static readonly StringName ModulateParticles = "ModulateParticles";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName WaitTime = "WaitTime";

		public static readonly StringName _anticipationParticles = "_anticipationParticles";

		public static readonly StringName _projectileStartParticles = "_projectileStartParticles";

		public static readonly StringName _projectileParticles = "_projectileParticles";

		public static readonly StringName _impactParticles = "_impactParticles";

		public static readonly StringName _modulateParticles = "_modulateParticles";

		public static readonly StringName _anticipationContainer = "_anticipationContainer";

		public static readonly StringName _anticipationDuration = "_anticipationDuration";

		public static readonly StringName _projectileContainer = "_projectileContainer";

		public static readonly StringName _projectileStartPoint = "_projectileStartPoint";

		public static readonly StringName _projectileEndPoint = "_projectileEndPoint";

		public static readonly StringName _projectileOffset = "_projectileOffset";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_large_magic_missile");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _anticipationParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _projectileStartParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _projectileParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _impactParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _modulateParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Node2D? _anticipationContainer;

	[Export(PropertyHint.None, "")]
	private float _anticipationDuration = 0.2f;

	[Export(PropertyHint.None, "")]
	private Node2D? _projectileContainer;

	[Export(PropertyHint.None, "")]
	private Node2D? _projectileStartPoint;

	[Export(PropertyHint.None, "")]
	private Node2D? _projectileEndPoint;

	[Export(PropertyHint.None, "")]
	private float _projectileOffset = 100f;

	private CancellationTokenSource? _cts;

	[field: Export(PropertyHint.None, "")]
	public float WaitTime { get; private set; } = 0.2f;

	public static NLargeMagicMissileVfx? Create(Vector2 targetFloorPosition, Color tint)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NLargeMagicMissileVfx nLargeMagicMissileVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NLargeMagicMissileVfx>(PackedScene.GenEditState.Disabled);
		nLargeMagicMissileVfx.GlobalPosition = targetFloorPosition;
		nLargeMagicMissileVfx.Initialize();
		nLargeMagicMissileVfx.ModulateParticles(tint);
		return nLargeMagicMissileVfx;
	}

	private Vector2 GetProjectileDirection()
	{
		Vector3 vector = Quaternion.FromEuler(new Vector3(0f, 0f, Mathf.DegToRad(-30f))) * Vector3.Up;
		return new Vector2(vector.X, vector.Y).Normalized();
	}

	private Vector2 GetTopPosition(Vector2 projectileDirection)
	{
		return (Vector2)Geometry2D.LineIntersectsLine(base.GlobalPosition, projectileDirection, new Vector2(0f, 80f), Vector2.Right);
	}

	private void Initialize()
	{
		Vector2 projectileDirection = GetProjectileDirection();
		Vector2 topPosition = GetTopPosition(projectileDirection);
		_anticipationContainer.GlobalPosition = topPosition;
		_projectileStartPoint.GlobalPosition = topPosition + projectileDirection * _projectileOffset;
		_projectileEndPoint.GlobalPosition = base.GlobalPosition + projectileDirection * _projectileOffset;
		_projectileContainer.Visible = false;
	}

	private void ModulateParticles(Color tint)
	{
		for (int i = 0; i < _modulateParticles.Count; i++)
		{
			_modulateParticles[i].SelfModulate = tint;
		}
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}

	private async Task PlaySequence()
	{
		_cts = new CancellationTokenSource();
		for (int i = 0; i < _anticipationParticles.Count; i++)
		{
			_anticipationParticles[i].Restart();
		}
		await Cmd.Wait(_anticipationDuration, _cts.Token);
		for (int j = 0; j < _projectileStartParticles.Count; j++)
		{
			_projectileStartParticles[j].Restart();
		}
		_projectileContainer.GlobalPosition = _projectileStartPoint.GlobalPosition;
		_projectileContainer.Visible = true;
		for (int k = 0; k < _projectileParticles.Count; k++)
		{
			_projectileParticles[k].Restart();
		}
		double timer = 0.0;
		while (timer < (double)WaitTime && !_cts.IsCancellationRequested)
		{
			float weight = (float)timer / WaitTime;
			_projectileContainer.GlobalPosition = _projectileStartPoint.GlobalPosition.Lerp(_projectileEndPoint.GlobalPosition, weight);
			timer += GetProcessDeltaTime();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (!_cts.IsCancellationRequested)
		{
			_projectileContainer.Visible = false;
			for (int l = 0; l < _impactParticles.Count; l++)
			{
				_impactParticles[l].Restart();
			}
			NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
			await Cmd.Wait(2f, _cts.Token);
			this.QueueFreeSafely();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetFloorPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetProjectileDirection, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetTopPosition, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "projectileDirection", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ModulateParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Color, "tint", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NLargeMagicMissileVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
			return true;
		}
		if (method == MethodName.GetProjectileDirection && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetProjectileDirection());
			return true;
		}
		if (method == MethodName.GetTopPosition && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetTopPosition(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 0)
		{
			Initialize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ModulateParticles && args.Count == 1)
		{
			ModulateParticles(VariantUtils.ConvertTo<Color>(in args[0]));
			ret = default(godot_variant);
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NLargeMagicMissileVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Color>(in args[1])));
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
		if (method == MethodName.GetProjectileDirection)
		{
			return true;
		}
		if (method == MethodName.GetTopPosition)
		{
			return true;
		}
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName.ModulateParticles)
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
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.WaitTime)
		{
			WaitTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._anticipationParticles)
		{
			_anticipationParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._projectileStartParticles)
		{
			_projectileStartParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._projectileParticles)
		{
			_projectileParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._impactParticles)
		{
			_impactParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._modulateParticles)
		{
			_modulateParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._anticipationContainer)
		{
			_anticipationContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			_anticipationDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._projectileContainer)
		{
			_projectileContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._projectileStartPoint)
		{
			_projectileStartPoint = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._projectileEndPoint)
		{
			_projectileEndPoint = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._projectileOffset)
		{
			_projectileOffset = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.WaitTime)
		{
			value = VariantUtils.CreateFrom<float>(WaitTime);
			return true;
		}
		if (name == PropertyName._anticipationParticles)
		{
			value = VariantUtils.CreateFromArray(_anticipationParticles);
			return true;
		}
		if (name == PropertyName._projectileStartParticles)
		{
			value = VariantUtils.CreateFromArray(_projectileStartParticles);
			return true;
		}
		if (name == PropertyName._projectileParticles)
		{
			value = VariantUtils.CreateFromArray(_projectileParticles);
			return true;
		}
		if (name == PropertyName._impactParticles)
		{
			value = VariantUtils.CreateFromArray(_impactParticles);
			return true;
		}
		if (name == PropertyName._modulateParticles)
		{
			value = VariantUtils.CreateFromArray(_modulateParticles);
			return true;
		}
		if (name == PropertyName._anticipationContainer)
		{
			value = VariantUtils.CreateFrom(in _anticipationContainer);
			return true;
		}
		if (name == PropertyName._anticipationDuration)
		{
			value = VariantUtils.CreateFrom(in _anticipationDuration);
			return true;
		}
		if (name == PropertyName._projectileContainer)
		{
			value = VariantUtils.CreateFrom(in _projectileContainer);
			return true;
		}
		if (name == PropertyName._projectileStartPoint)
		{
			value = VariantUtils.CreateFrom(in _projectileStartPoint);
			return true;
		}
		if (name == PropertyName._projectileEndPoint)
		{
			value = VariantUtils.CreateFrom(in _projectileEndPoint);
			return true;
		}
		if (name == PropertyName._projectileOffset)
		{
			value = VariantUtils.CreateFrom(in _projectileOffset);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.WaitTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._anticipationParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._projectileStartParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._projectileParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._impactParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._modulateParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._anticipationContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._anticipationDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._projectileContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._projectileStartPoint, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._projectileEndPoint, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._projectileOffset, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.WaitTime, Variant.From<float>(WaitTime));
		info.AddProperty(PropertyName._anticipationParticles, Variant.CreateFrom(_anticipationParticles));
		info.AddProperty(PropertyName._projectileStartParticles, Variant.CreateFrom(_projectileStartParticles));
		info.AddProperty(PropertyName._projectileParticles, Variant.CreateFrom(_projectileParticles));
		info.AddProperty(PropertyName._impactParticles, Variant.CreateFrom(_impactParticles));
		info.AddProperty(PropertyName._modulateParticles, Variant.CreateFrom(_modulateParticles));
		info.AddProperty(PropertyName._anticipationContainer, Variant.From(in _anticipationContainer));
		info.AddProperty(PropertyName._anticipationDuration, Variant.From(in _anticipationDuration));
		info.AddProperty(PropertyName._projectileContainer, Variant.From(in _projectileContainer));
		info.AddProperty(PropertyName._projectileStartPoint, Variant.From(in _projectileStartPoint));
		info.AddProperty(PropertyName._projectileEndPoint, Variant.From(in _projectileEndPoint));
		info.AddProperty(PropertyName._projectileOffset, Variant.From(in _projectileOffset));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.WaitTime, out var value))
		{
			WaitTime = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._anticipationParticles, out var value2))
		{
			_anticipationParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._projectileStartParticles, out var value3))
		{
			_projectileStartParticles = value3.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._projectileParticles, out var value4))
		{
			_projectileParticles = value4.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._impactParticles, out var value5))
		{
			_impactParticles = value5.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._modulateParticles, out var value6))
		{
			_modulateParticles = value6.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._anticipationContainer, out var value7))
		{
			_anticipationContainer = value7.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._anticipationDuration, out var value8))
		{
			_anticipationDuration = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._projectileContainer, out var value9))
		{
			_projectileContainer = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._projectileStartPoint, out var value10))
		{
			_projectileStartPoint = value10.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._projectileEndPoint, out var value11))
		{
			_projectileEndPoint = value11.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._projectileOffset, out var value12))
		{
			_projectileOffset = value12.As<float>();
		}
	}
}
