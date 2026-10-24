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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

/// <summary>
/// Vfx for Hellraiser card.
/// Spawns many swords and tints the screen red.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NHellraiserVfx.cs")]
public class NHellraiserVfx : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
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
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_duration' field.
		/// </summary>
		public static readonly StringName _duration = "_duration";

		/// <summary>
		/// Cached name for the '_swordAmount' field.
		/// </summary>
		public static readonly StringName _swordAmount = "_swordAmount";

		/// <summary>
		/// Cached name for the '_spawnPosition' field.
		/// </summary>
		public static readonly StringName _spawnPosition = "_spawnPosition";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private float _duration = 1f;

	private int _swordAmount = 10;

	private Vector2 _spawnPosition;

	private static readonly Vector2 _vfxOffset = new Vector2(-100f, -200f);

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/cards/vfx_hellraiser/hellraiser_vfx");

	private const string _hellraiserSfxPath = "event:/sfx/characters/ironclad/ironclad_hellraiser";

	private CancellationTokenSource? _cts;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public override void _ExitTree()
	{
		_cts?.Cancel();
	}

	public static NHellraiserVfx? Create(Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NHellraiserVfx nHellraiserVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NHellraiserVfx>(PackedScene.GenEditState.Disabled);
		nHellraiserVfx._spawnPosition = NCombatRoom.Instance.GetCreatureNode(target).GetBottomOfHitbox() + _vfxOffset;
		return nHellraiserVfx;
	}

	public override void _Ready()
	{
		List<float> list = new List<float>();
		for (int i = 0; i < _swordAmount; i++)
		{
			list.Add(Rng.Chaotic.NextFloat(10f, 50f));
		}
		list.Sort();
		foreach (float item in list)
		{
			NHellraiserSwordVfx nHellraiserSwordVfx = NHellraiserSwordVfx.Create();
			nHellraiserSwordVfx.GlobalPosition = _spawnPosition;
			nHellraiserSwordVfx.posY = item;
			float num = MathHelper.Remap(item, 10f, 50f, 0.8f, 1f);
			nHellraiserSwordVfx.targetColor = new Color(num, num, num);
			NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(nHellraiserSwordVfx);
		}
		list = new List<float>();
		for (int j = 0; j < _swordAmount; j++)
		{
			list.Add(Rng.Chaotic.NextFloat(-50f, -10f));
		}
		SfxCmd.Play("event:/sfx/characters/ironclad/ironclad_hellraiser");
		list.Sort();
		foreach (float item2 in list)
		{
			NHellraiserSwordVfx nHellraiserSwordVfx2 = NHellraiserSwordVfx.Create();
			nHellraiserSwordVfx2.GlobalPosition = _spawnPosition;
			nHellraiserSwordVfx2.posY = item2;
			float num2 = MathHelper.Remap(item2, -10f, -50f, 0.7f, 0.4f);
			nHellraiserSwordVfx2.targetColor = new Color(num2, num2, num2);
			NCombatRoom.Instance.BackCombatVfxContainer.AddChildSafely(nHellraiserSwordVfx2);
		}
		NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(NAdditiveOverlayVfx.Create());
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
		if (name == PropertyName._swordAmount)
		{
			_swordAmount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			_spawnPosition = VariantUtils.ConvertTo<Vector2>(in value);
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
		if (name == PropertyName._swordAmount)
		{
			value = VariantUtils.CreateFrom(in _swordAmount);
			return true;
		}
		if (name == PropertyName._spawnPosition)
		{
			value = VariantUtils.CreateFrom(in _spawnPosition);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._swordAmount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._spawnPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._duration, Variant.From(in _duration));
		info.AddProperty(PropertyName._swordAmount, Variant.From(in _swordAmount));
		info.AddProperty(PropertyName._spawnPosition, Variant.From(in _spawnPosition));
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
		if (info.TryGetProperty(PropertyName._swordAmount, out var value2))
		{
			_swordAmount = value2.As<int>();
		}
		if (info.TryGetProperty(PropertyName._spawnPosition, out var value3))
		{
			_spawnPosition = value3.As<Vector2>();
		}
	}
}
