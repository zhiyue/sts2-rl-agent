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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NSpikeSplashVfx.cs")]
public class NSpikeSplashVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_duration' field.
		/// </summary>
		public static readonly StringName _duration = "_duration";

		/// <summary>
		/// Cached name for the '_spikeAmount' field.
		/// </summary>
		public static readonly StringName _spikeAmount = "_spikeAmount";

		/// <summary>
		/// Cached name for the '_spawnPosition' field.
		/// </summary>
		public static readonly StringName _spawnPosition = "_spawnPosition";

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

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/spike_splash_vfx");

	private float _duration = 1f;

	private int _spikeAmount = 6;

	private Vector2 _spawnPosition;

	private VfxColor _vfxColor;

	private CancellationTokenSource? _cts;

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	public static NSpikeSplashVfx? Create(Creature target, VfxColor vfxColor = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSpikeSplashVfx nSpikeSplashVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSpikeSplashVfx>(PackedScene.GenEditState.Disabled);
		nSpikeSplashVfx._spawnPosition = NCombatRoom.Instance.GetCreatureNode(target).GetBottomOfHitbox();
		nSpikeSplashVfx._vfxColor = vfxColor;
		return nSpikeSplashVfx;
	}

	public override void _Ready()
	{
		for (int i = 0; i < _spikeAmount; i++)
		{
			NFgGroundSpikeVfx child = NFgGroundSpikeVfx.Create(_spawnPosition, movingRight: true, _vfxColor);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
			child = NFgGroundSpikeVfx.Create(_spawnPosition, movingRight: false, _vfxColor);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
		}
		for (int j = 0; j < _spikeAmount; j++)
		{
			NBgGroundSpikeVfx child2 = NBgGroundSpikeVfx.Create(_spawnPosition, movingRight: true, _vfxColor);
			NCombatRoom.Instance.BackCombatVfxContainer.AddChildSafely(child2);
			child2 = NBgGroundSpikeVfx.Create(_spawnPosition, movingRight: false, _vfxColor);
			NCombatRoom.Instance.BackCombatVfxContainer.AddChildSafely(child2);
		}
		TaskHelper.RunSafely(SelfDestruct());
	}

	private async Task SelfDestruct()
	{
		_cts = new CancellationTokenSource();
		await Task.Delay(2000, _cts.Token);
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
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Ready)
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
		if (name == PropertyName._spikeAmount)
		{
			_spikeAmount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			_spawnPosition = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._duration)
		{
			value = VariantUtils.CreateFrom(in _duration);
			return true;
		}
		if (name == PropertyName._spikeAmount)
		{
			value = VariantUtils.CreateFrom(in _spikeAmount);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			value = VariantUtils.CreateFrom(in _spawnPosition);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._duration, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._spikeAmount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._spawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._spikeAmount, Variant.From(in _spikeAmount));
		info.AddProperty(PropertyName._spawnPosition, Variant.From(in _spawnPosition));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
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
		if (info.TryGetProperty(PropertyName._spikeAmount, out var value2))
		{
			_spikeAmount = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._spawnPosition, out var value3))
		{
			_spawnPosition = value3.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value4))
		{
			_vfxColor = value4.As<VfxColor>();
		}
	}
}
