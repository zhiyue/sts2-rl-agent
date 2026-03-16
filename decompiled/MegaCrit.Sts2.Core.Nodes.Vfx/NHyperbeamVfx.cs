using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NHyperbeamVfx.cs")]
public class NHyperbeamVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName ApplyRotation = "ApplyRotation";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ShowLaser = "ShowLaser";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _anticipationParticles = "_anticipationParticles";

		public static readonly StringName _laserParticles = "_laserParticles";

		public static readonly StringName _laserEndParticles = "_laserEndParticles";

		public static readonly StringName _laserLine = "_laserLine";

		public static readonly StringName _laserContainer = "_laserContainer";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_hyperbeam");

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _anticipationParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _laserParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Array<GpuParticles2D> _laserEndParticles = new Array<GpuParticles2D>();

	[Export(PropertyHint.None, "")]
	private Line2D? _laserLine;

	[Export(PropertyHint.None, "")]
	private Node2D? _laserContainer;

	public static readonly float hyperbeamAnticipationDuration = 0.525f;

	public static readonly float hyperbeamLaserDuration = 0.5f;

	public static NHyperbeamVfx? Create(Creature owner, Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		NCreature nCreature2 = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature2 != null && nCreature != null)
		{
			Vector2 vfxSpawnPosition = nCreature.VfxSpawnPosition;
			Player player = owner.Player;
			if (player != null && player.Character is Defect defect)
			{
				vfxSpawnPosition += defect.EyelineOffset;
			}
			return Create(vfxSpawnPosition, nCreature2.VfxSpawnPosition);
		}
		return null;
	}

	public static NHyperbeamVfx? Create(Vector2 defectEyePosition, Vector2 mainTargetCenterPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NHyperbeamVfx nHyperbeamVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NHyperbeamVfx>(PackedScene.GenEditState.Disabled);
		nHyperbeamVfx.GlobalPosition = defectEyePosition;
		nHyperbeamVfx.ApplyRotation(defectEyePosition, mainTargetCenterPosition);
		return nHyperbeamVfx;
	}

	public void ApplyRotation(Vector2 sourcePosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - sourcePosition;
		float rotationDegrees = Mathf.RadToDeg(Mathf.Atan2(vector.Y, vector.X));
		base.RotationDegrees = rotationDegrees;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private void ShowLaser(bool showing)
	{
		for (int i = 0; i < _laserParticles.Count; i++)
		{
			_laserParticles[i].Visible = showing;
			if (showing)
			{
				_laserParticles[i].Restart();
			}
		}
		_laserLine.Visible = showing;
		_laserContainer.Visible = showing;
	}

	private async Task PlaySequence()
	{
		ShowLaser(showing: false);
		for (int i = 0; i < _anticipationParticles.Count; i++)
		{
			_anticipationParticles[i].Restart();
		}
		await Cmd.Wait(hyperbeamAnticipationDuration);
		ShowLaser(showing: true);
		NGame.Instance?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Normal);
		await Cmd.Wait(hyperbeamLaserDuration);
		ShowLaser(showing: false);
		for (int j = 0; j < _laserEndParticles.Count; j++)
		{
			_laserEndParticles[j].Restart();
		}
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "defectEyePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "mainTargetCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyRotation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "sourcePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowLaser, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "showing", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NHyperbeamVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName.ApplyRotation && args.Count == 2)
		{
			ApplyRotation(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowLaser && args.Count == 1)
		{
			ShowLaser(VariantUtils.ConvertTo<bool>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NHyperbeamVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
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
		if (method == MethodName.ApplyRotation)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ShowLaser)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._anticipationParticles)
		{
			_anticipationParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._laserParticles)
		{
			_laserParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._laserEndParticles)
		{
			_laserEndParticles = VariantUtils.ConvertToArray<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._laserLine)
		{
			_laserLine = VariantUtils.ConvertTo<Line2D>(in value);
			return true;
		}
		if (name == PropertyName._laserContainer)
		{
			_laserContainer = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._anticipationParticles)
		{
			value = VariantUtils.CreateFromArray(_anticipationParticles);
			return true;
		}
		if (name == PropertyName._laserParticles)
		{
			value = VariantUtils.CreateFromArray(_laserParticles);
			return true;
		}
		if (name == PropertyName._laserEndParticles)
		{
			value = VariantUtils.CreateFromArray(_laserEndParticles);
			return true;
		}
		if (name == PropertyName._laserLine)
		{
			value = VariantUtils.CreateFrom(in _laserLine);
			return true;
		}
		if (name == PropertyName._laserContainer)
		{
			value = VariantUtils.CreateFrom(in _laserContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._anticipationParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._laserParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._laserEndParticles, PropertyHint.TypeString, "24/34:GPUParticles2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._laserLine, PropertyHint.NodeType, "Line2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._laserContainer, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._anticipationParticles, Variant.CreateFrom(_anticipationParticles));
		info.AddProperty(PropertyName._laserParticles, Variant.CreateFrom(_laserParticles));
		info.AddProperty(PropertyName._laserEndParticles, Variant.CreateFrom(_laserEndParticles));
		info.AddProperty(PropertyName._laserLine, Variant.From(in _laserLine));
		info.AddProperty(PropertyName._laserContainer, Variant.From(in _laserContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._anticipationParticles, out var value))
		{
			_anticipationParticles = value.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._laserParticles, out var value2))
		{
			_laserParticles = value2.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._laserEndParticles, out var value3))
		{
			_laserEndParticles = value3.AsGodotArray<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._laserLine, out var value4))
		{
			_laserLine = value4.As<Line2D>();
		}
		if (info.TryGetProperty(PropertyName._laserContainer, out var value5))
		{
			_laserContainer = value5.As<Node2D>();
		}
	}
}
