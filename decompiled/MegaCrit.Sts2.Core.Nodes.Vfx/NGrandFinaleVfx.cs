using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NGrandFinaleVfx.cs")]
public class NGrandFinaleVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'Initialize' method.
		/// </summary>
		public static readonly StringName Initialize = "Initialize";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_spotlight' field.
		/// </summary>
		public static readonly StringName _spotlight = "_spotlight";

		/// <summary>
		/// Cached name for the '_spotlightParticles' field.
		/// </summary>
		public static readonly StringName _spotlightParticles = "_spotlightParticles";

		/// <summary>
		/// Cached name for the '_anticipationParticles' field.
		/// </summary>
		public static readonly StringName _anticipationParticles = "_anticipationParticles";

		/// <summary>
		/// Cached name for the '_slashParticles' field.
		/// </summary>
		public static readonly StringName _slashParticles = "_slashParticles";

		/// <summary>
		/// Cached name for the '_endParticles' field.
		/// </summary>
		public static readonly StringName _endParticles = "_endParticles";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_grand_finale");

	[Export(PropertyHint.None, "")]
	private Node2D? _spotlight;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _spotlightParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _anticipationParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _slashParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _endParticles;

	private CancellationTokenSource? _cts;

	private static readonly float _spotlightDuration = 1.25f;

	private static readonly float _anticipationDuration = 0.25f;

	private static readonly float _slashDuration = 0.125f;

	private static readonly float _hitDuration = 0.0125f;

	public static readonly float totalAnticipationDuration = _spotlightDuration + _anticipationDuration + _slashDuration;

	public static NGrandFinaleVfx? Create(Creature creature)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (nCreature != null)
		{
			return Create(nCreature.VfxSpawnPosition);
		}
		return null;
	}

	public static NGrandFinaleVfx? Create(Vector2 playerPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NGrandFinaleVfx nGrandFinaleVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NGrandFinaleVfx>(PackedScene.GenEditState.Disabled);
		nGrandFinaleVfx.Initialize(playerPosition);
		return nGrandFinaleVfx;
	}

	private void Initialize(Vector2 playerPosition)
	{
		_anticipationParticles.GlobalPosition = playerPosition;
		_slashParticles.GlobalPosition = playerPosition;
		_endParticles.GlobalPosition = playerPosition;
		_spotlight.Modulate = new Color(1f, 1f, 1f, 0f);
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	private async Task PlaySequence()
	{
		_cts = new CancellationTokenSource();
		_spotlightParticles.GlobalPosition = new Vector2(GetViewportRect().Size.X / 2f, 0f);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(_spotlight, "modulate", new Color(1f, 1f, 1f), 1.0);
		_spotlightParticles.Restart();
		await Cmd.Wait(_spotlightDuration, _cts.Token);
		_anticipationParticles.Restart();
		await Cmd.Wait(_anticipationDuration, _cts.Token);
		_slashParticles.Restart();
		await Cmd.Wait(_slashDuration, _cts.Token);
		NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
		await Cmd.Wait(_hitDuration, _cts.Token);
		_endParticles.Restart();
		Tween tween2 = GetTree().CreateTween();
		tween2.TweenProperty(_spotlight, "modulate", new Color(1f, 1f, 1f, 0f), 0.5);
		await Cmd.Wait(2f, _cts.Token);
		this.QueueFreeSafely();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "playerPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "playerPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NGrandFinaleVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<Vector2>(in args[0]));
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
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NGrandFinaleVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.Initialize)
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._spotlight)
		{
			_spotlight = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._spotlightParticles)
		{
			_spotlightParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._anticipationParticles)
		{
			_anticipationParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._slashParticles)
		{
			_slashParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			_endParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._spotlight)
		{
			value = VariantUtils.CreateFrom(in _spotlight);
			return true;
		}
		if (name == PropertyName._spotlightParticles)
		{
			value = VariantUtils.CreateFrom(in _spotlightParticles);
			return true;
		}
		if (name == PropertyName._anticipationParticles)
		{
			value = VariantUtils.CreateFrom(in _anticipationParticles);
			return true;
		}
		if (name == PropertyName._slashParticles)
		{
			value = VariantUtils.CreateFrom(in _slashParticles);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			value = VariantUtils.CreateFrom(in _endParticles);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spotlight, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spotlightParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._anticipationParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._endParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._spotlight, Variant.From(in _spotlight));
		info.AddProperty(PropertyName._spotlightParticles, Variant.From(in _spotlightParticles));
		info.AddProperty(PropertyName._anticipationParticles, Variant.From(in _anticipationParticles));
		info.AddProperty(PropertyName._slashParticles, Variant.From(in _slashParticles));
		info.AddProperty(PropertyName._endParticles, Variant.From(in _endParticles));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._spotlight, out var value))
		{
			_spotlight = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._spotlightParticles, out var value2))
		{
			_spotlightParticles = value2.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._anticipationParticles, out var value3))
		{
			_anticipationParticles = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._slashParticles, out var value4))
		{
			_slashParticles = value4.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._endParticles, out var value5))
		{
			_endParticles = value5.As<NParticlesContainer>();
		}
	}
}
