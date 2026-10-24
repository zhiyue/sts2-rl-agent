using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

[ScriptPath("res://src/Core/Nodes/Screens/Bestiary/NBestiaryLayoutDecimillipede.cs")]
public class NBestiaryLayoutDecimillipede : NBestiaryLayout
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NBestiaryLayout.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Cleanup' method.
		/// </summary>
		public new static readonly StringName Cleanup = "Cleanup";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NBestiaryLayout.PropertyName
	{
		/// <summary>
		/// Cached name for the '_encounterVisuals' field.
		/// </summary>
		public static readonly StringName _encounterVisuals = "_encounterVisuals";

		/// <summary>
		/// Cached name for the '_creatureContainer' field.
		/// </summary>
		public static readonly StringName _creatureContainer = "_creatureContainer";

		/// <summary>
		/// Cached name for the '_encounterSlots' field.
		/// </summary>
		public static readonly StringName _encounterSlots = "_encounterSlots";

		/// <summary>
		/// Cached name for the '_bestiary' field.
		/// </summary>
		public static readonly StringName _bestiary = "_bestiary";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NBestiaryLayout.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("screens/bestiary/bestiary_layout_decimillipede");

	private Control? _encounterVisuals;

	private Control _creatureContainer;

	private Control _encounterSlots;

	private List<NCreature> _creatures = new List<NCreature>();

	private NBestiary _bestiary;

	public static NBestiaryLayoutDecimillipede? Create(NBestiary bestiary)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NBestiaryLayoutDecimillipede nBestiaryLayoutDecimillipede = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NBestiaryLayoutDecimillipede>(PackedScene.GenEditState.Disabled);
		nBestiaryLayoutDecimillipede._bestiary = bestiary;
		return nBestiaryLayoutDecimillipede;
	}

	public override void _Ready()
	{
		_creatureContainer = GetNode<Control>("%CreatureContainer");
		_encounterSlots = GetNode<Control>("%EncounterSlots");
	}

	public override void Cleanup()
	{
		_encounterVisuals?.QueueFreeSafely();
		_encounterVisuals = null;
	}

	public override List<BestiaryMonsterMove> Setup(BestiaryEntry entry, Tween tween)
	{
		EncounterModel encounterModel = entry.encounterModel.ToMutable();
		_encounterVisuals?.QueueFreeSafely();
		encounterModel.GenerateMonstersWithSlots(NullRunState.Instance);
		foreach (var monstersWithSlot in encounterModel.MonstersWithSlots)
		{
			MonsterModel item = monstersWithSlot.Item1;
			string item2 = monstersWithSlot.Item2;
			item.Rng = Rng.Chaotic;
			item.SetUpForCombat();
			Creature creature = new Creature(item, CombatSide.Enemy, item2)
			{
				CombatState = new NullCombatState()
			};
			NCreature nCreature = NCreature.Create(creature);
			_creatureContainer.AddChildSafely(nCreature);
			_creatures.Add(nCreature);
			nCreature.SetupForBestiary();
			nCreature.GlobalPosition = _encounterSlots.GetNode<Marker2D>(creature.SlotName).GlobalPosition;
		}
		int num = 3;
		List<BestiaryMonsterMove> list = new List<BestiaryMonsterMove>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<BestiaryMonsterMove> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		span[num2] = BestiaryMonsterMove.FromAction(GetBestiaryMoveName("WRITHE"), AnimAttack);
		num2++;
		span[num2] = BestiaryMonsterMove.FromAction(GetBestiaryMoveName("REATTACH"), AnimReattach);
		num2++;
		span[num2] = BestiaryMonsterMove.FromAction(GetBestiaryMoveName("DEAD"), AnimDie);
		return list;
	}

	private LocString GetBestiaryMoveName(string moveId)
	{
		return new LocString("monsters", ModelDb.GetId<DecimillipedeSegment>().Entry + ".moves." + moveId + ".title");
	}

	private async Task AnimAttack()
	{
		foreach (NCreature creature in _creatures)
		{
			((DecimillipedeSegment)creature.Entity.Monster).SegmentAttack();
		}
		Node2D node2D = PreloadManager.Cache.GetScene(DecimillipedeSegment.rocksVfxPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		_bestiary.VfxContainer.AddChildSafely(node2D);
		node2D.GlobalPosition = NGame.Instance.GetViewportRect().Size * 0.5f;
		await Cmd.Wait(0.5f);
	}

	private async Task AnimReattach()
	{
		foreach (NCreature creature in _creatures)
		{
			creature.GetSpecialNode<NDecimillipedeSegmentVfx>("%NDecimillipedeSegmentVfx")?.Regenerate();
			await CreatureCmd.TriggerAnim(creature.Entity, "Revive", 0.15f);
		}
	}

	private async Task AnimDie()
	{
		foreach (NCreature creature in _creatures)
		{
			await CreatureCmd.TriggerAnim(creature.Entity, "Dead", 0.15f);
		}
	}

	public override IEnumerable<NCreature> GetCreatures()
	{
		return _creatures;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "bestiary", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NBestiaryLayoutDecimillipede>(Create(VariantUtils.ConvertTo<NBestiary>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cleanup && args.Count == 0)
		{
			Cleanup();
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
			ret = VariantUtils.CreateFrom<NBestiaryLayoutDecimillipede>(Create(VariantUtils.ConvertTo<NBestiary>(in args[0])));
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Cleanup)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._encounterVisuals)
		{
			_encounterVisuals = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._creatureContainer)
		{
			_creatureContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._encounterSlots)
		{
			_encounterSlots = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bestiary)
		{
			_bestiary = VariantUtils.ConvertTo<NBestiary>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._encounterVisuals)
		{
			value = VariantUtils.CreateFrom(in _encounterVisuals);
			return true;
		}
		if (name == PropertyName._creatureContainer)
		{
			value = VariantUtils.CreateFrom(in _creatureContainer);
			return true;
		}
		if (name == PropertyName._encounterSlots)
		{
			value = VariantUtils.CreateFrom(in _encounterSlots);
			return true;
		}
		if (name == PropertyName._bestiary)
		{
			value = VariantUtils.CreateFrom(in _bestiary);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._encounterVisuals, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._encounterSlots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bestiary, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._encounterVisuals, Variant.From(in _encounterVisuals));
		info.AddProperty(PropertyName._creatureContainer, Variant.From(in _creatureContainer));
		info.AddProperty(PropertyName._encounterSlots, Variant.From(in _encounterSlots));
		info.AddProperty(PropertyName._bestiary, Variant.From(in _bestiary));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._encounterVisuals, out var value))
		{
			_encounterVisuals = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._creatureContainer, out var value2))
		{
			_creatureContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._encounterSlots, out var value3))
		{
			_encounterSlots = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bestiary, out var value4))
		{
			_bestiary = value4.As<NBestiary>();
		}
	}
}
