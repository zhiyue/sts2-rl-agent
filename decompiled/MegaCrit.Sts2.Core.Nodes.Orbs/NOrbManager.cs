using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Orbs;

[ScriptPath("res://src/Core/Nodes/Orbs/NOrbManager.cs")]
public class NOrbManager : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Create = "Create";

		public static readonly StringName RemoveSlotAnim = "RemoveSlotAnim";

		public static readonly StringName AddSlotAnim = "AddSlotAnim";

		public static readonly StringName AddOrbAnim = "AddOrbAnim";

		public static readonly StringName UpdateControllerNavigation = "UpdateControllerNavigation";

		public static readonly StringName TweenLayout = "TweenLayout";

		public static readonly StringName UpdateVisuals = "UpdateVisuals";

		public static readonly StringName ClearOrbs = "ClearOrbs";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName IsLocal = "IsLocal";

		public static readonly StringName DefaultFocusOwner = "DefaultFocusOwner";

		public static readonly StringName _orbContainer = "_orbContainer";

		public static readonly StringName _creatureNode = "_creatureNode";

		public static readonly StringName _curTween = "_curTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _orbContainer;

	private readonly List<NOrb> _orbs = new List<NOrb>();

	private NCreature _creatureNode;

	private const float _minRadius = 225f;

	private const float _maxRadius = 300f;

	private const float _range = 150f;

	private const float _angleOffset = -25f;

	private const float _tweenSpeed = 0.45f;

	private Tween? _curTween;

	private static string ScenePath => SceneHelper.GetScenePath("/orbs/orb_manager");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public bool IsLocal { get; private set; }

	private Player Player => _creatureNode.Entity.Player;

	public Control DefaultFocusOwner
	{
		get
		{
			if (_orbs.Count <= 0)
			{
				return _creatureNode.Hitbox;
			}
			return _orbs.First();
		}
	}

	public override void _Ready()
	{
		_orbContainer = GetNode<Control>("%Orbs");
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		CombatManager.Instance.CombatSetUp += OnCombatSetup;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		CombatManager.Instance.CombatSetUp -= OnCombatSetup;
	}

	public static NOrbManager Create(NCreature creature, bool isLocal)
	{
		if (creature.Entity.Player == null)
		{
			throw new InvalidOperationException("NOrbManager can only be applied to player creatures");
		}
		NOrbManager nOrbManager = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NOrbManager>(PackedScene.GenEditState.Disabled);
		nOrbManager._creatureNode = creature;
		nOrbManager.IsLocal = isLocal;
		return nOrbManager;
	}

	private void OnCombatSetup(CombatState _)
	{
		if (Player.Creature.IsAlive && Player.PlayerCombatState != null)
		{
			AddSlotAnim(Player.PlayerCombatState.OrbQueue.Capacity);
		}
	}

	public void RemoveSlotAnim(int amount)
	{
		if (amount > _orbs.Count)
		{
			throw new InvalidOperationException("There are not enough slots to remove.");
		}
		for (int i = 0; i < amount; i++)
		{
			NOrb nOrb = _orbs.Last();
			nOrb.QueueFreeSafely();
			_orbs.Remove(nOrb);
			if (nOrb.HasFocus())
			{
				_creatureNode.Hitbox.TryGrabFocus();
			}
		}
		TweenLayout();
		UpdateControllerNavigation();
	}

	public void AddSlotAnim(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			NOrb nOrb = NOrb.Create(LocalContext.IsMe(Player));
			_orbContainer.AddChildSafely(nOrb);
			_orbs.Add(nOrb);
			nOrb.Position = Vector2.Zero;
		}
		TweenLayout();
		UpdateControllerNavigation();
	}

	public void ReplaceOrb(OrbModel oldOrb, OrbModel newOrb)
	{
		for (int i = 0; i < _orbs.Count; i++)
		{
			if (_orbs[i].Model == oldOrb)
			{
				_orbs[i].ReplaceOrb(newOrb);
			}
		}
		UpdateControllerNavigation();
	}

	public void AddOrbAnim()
	{
		OrbModel model = Player.PlayerCombatState.OrbQueue.Orbs.LastOrDefault();
		NOrb nOrb = _orbs.FirstOrDefault((NOrb node) => node.Model == null);
		if (nOrb == null)
		{
			EvokeOrbAnim(_orbs.First((NOrb node) => node.Model != null).Model);
			nOrb = (NOrb)_orbContainer.GetChildren().First((Node node) => ((NOrb)node).Model == null);
		}
		NOrb nOrb2 = NOrb.Create(LocalContext.IsMe(Player), model);
		nOrb.AddSibling(nOrb2);
		_orbs.Insert(_orbs.IndexOf(nOrb), nOrb2);
		nOrb2.Position = nOrb.Position;
		_orbContainer.RemoveChildSafely(nOrb);
		_orbs.Remove(nOrb);
		nOrb.QueueFreeSafely();
		TweenLayout();
		UpdateControllerNavigation();
	}

	public void EvokeOrbAnim(OrbModel orb)
	{
		NOrb nOrb = _orbs.Last((NOrb node) => node.Model == orb);
		Tween tween = CreateTween();
		_orbs.Remove(nOrb);
		tween.TweenProperty(nOrb, "modulate:a", 0, 0.25);
		tween.Chain().TweenCallback(Callable.From(nOrb.QueueFreeSafely));
		NOrb nOrb2 = NOrb.Create(LocalContext.IsMe(Player));
		_orbContainer.AddChildSafely(nOrb2);
		_orbs.Add(nOrb2);
		nOrb2.Position = Vector2.Zero;
		if (nOrb.HasFocus())
		{
			_creatureNode.Hitbox.TryGrabFocus();
		}
		TweenLayout();
		UpdateControllerNavigation();
	}

	private void UpdateControllerNavigation()
	{
		for (int i = 0; i < _orbs.Count; i++)
		{
			NOrb nOrb = _orbs[i];
			NodePath path;
			if (i <= 0)
			{
				List<NOrb> orbs = _orbs;
				path = orbs[orbs.Count - 1].GetPath();
			}
			else
			{
				path = _orbs[i - 1].GetPath();
			}
			nOrb.FocusNeighborRight = path;
			_orbs[i].FocusNeighborLeft = ((i < _orbs.Count - 1) ? _orbs[i + 1].GetPath() : _orbs[0].GetPath());
			_orbs[i].FocusNeighborTop = _orbs[i].GetPath();
			_orbs[i].FocusNeighborBottom = _creatureNode.Hitbox.GetPath();
		}
		_creatureNode.UpdateNavigation();
	}

	private void TweenLayout()
	{
		int capacity = Player.PlayerCombatState.OrbQueue.Capacity;
		if (capacity != 0)
		{
			float num = 125f;
			float num2 = num / (float)(capacity - 1);
			float num3 = Mathf.Lerp(225f, 300f, ((float)capacity - 3f) / 7f);
			if (!IsLocal)
			{
				num3 *= 0.75f;
			}
			_curTween?.Kill();
			_curTween = CreateTween().SetParallel();
			for (int i = 0; i < capacity; i++)
			{
				float s = float.DegreesToRadians(-25f - num);
				Vector2 vector = new Vector2(0f - Mathf.Cos(s), Mathf.Sin(s)) * num3;
				_curTween.TweenProperty(_orbs[i], "position", vector, 0.44999998807907104).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
				num -= num2;
			}
		}
	}

	private void OnCombatStateChanged(CombatState _)
	{
		UpdateVisuals(OrbEvokeType.None);
	}

	public void UpdateVisuals(OrbEvokeType evokeType)
	{
		foreach (NOrb orb in _orbs)
		{
			orb.UpdateVisuals(isEvoking: false);
		}
		switch (evokeType)
		{
		case OrbEvokeType.Front:
			_orbs.FirstOrDefault()?.UpdateVisuals(isEvoking: true);
			break;
		case OrbEvokeType.All:
		{
			foreach (NOrb orb2 in _orbs)
			{
				orb2.UpdateVisuals(isEvoking: true);
			}
			break;
		}
		case OrbEvokeType.None:
			break;
		}
	}

	public void ClearOrbs()
	{
		_curTween?.Kill();
		if (_orbs.Count == 0)
		{
			return;
		}
		_curTween = CreateTween();
		foreach (NOrb orb in _orbs)
		{
			_curTween.Parallel().TweenProperty(orb, "position", Vector2.Zero, 1.0).SetEase(Tween.EaseType.InOut)
				.SetTrans(Tween.TransitionType.Sine);
			_curTween.Parallel().TweenProperty(orb, "modulate:a", 0, 0.25);
		}
		foreach (NOrb orb2 in _orbs)
		{
			_curTween.Chain().TweenCallback(Callable.From(orb2.QueueFreeSafely));
		}
		_orbs.Clear();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "creature", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "isLocal", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveSlotAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddSlotAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddOrbAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateControllerNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.TweenLayout, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "evokeType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ClearOrbs, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NOrbManager>(Create(VariantUtils.ConvertTo<NCreature>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		if (method == MethodName.RemoveSlotAnim && args.Count == 1)
		{
			RemoveSlotAnim(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddSlotAnim && args.Count == 1)
		{
			AddSlotAnim(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddOrbAnim && args.Count == 0)
		{
			AddOrbAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateControllerNavigation && args.Count == 0)
		{
			UpdateControllerNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TweenLayout && args.Count == 0)
		{
			TweenLayout();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVisuals && args.Count == 1)
		{
			UpdateVisuals(VariantUtils.ConvertTo<OrbEvokeType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearOrbs && args.Count == 0)
		{
			ClearOrbs();
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
			ret = VariantUtils.CreateFrom<NOrbManager>(Create(VariantUtils.ConvertTo<NCreature>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
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
		if (method == MethodName._EnterTree)
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
		if (method == MethodName.RemoveSlotAnim)
		{
			return true;
		}
		if (method == MethodName.AddSlotAnim)
		{
			return true;
		}
		if (method == MethodName.AddOrbAnim)
		{
			return true;
		}
		if (method == MethodName.UpdateControllerNavigation)
		{
			return true;
		}
		if (method == MethodName.TweenLayout)
		{
			return true;
		}
		if (method == MethodName.UpdateVisuals)
		{
			return true;
		}
		if (method == MethodName.ClearOrbs)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsLocal)
		{
			IsLocal = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._orbContainer)
		{
			_orbContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._creatureNode)
		{
			_creatureNode = VariantUtils.ConvertTo<NCreature>(in value);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			_curTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsLocal)
		{
			value = VariantUtils.CreateFrom<bool>(IsLocal);
			return true;
		}
		if (name == PropertyName.DefaultFocusOwner)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusOwner);
			return true;
		}
		if (name == PropertyName._orbContainer)
		{
			value = VariantUtils.CreateFrom(in _orbContainer);
			return true;
		}
		if (name == PropertyName._creatureNode)
		{
			value = VariantUtils.CreateFrom(in _creatureNode);
			return true;
		}
		if (name == PropertyName._curTween)
		{
			value = VariantUtils.CreateFrom(in _curTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLocal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._orbContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._creatureNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._curTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsLocal, Variant.From<bool>(IsLocal));
		info.AddProperty(PropertyName._orbContainer, Variant.From(in _orbContainer));
		info.AddProperty(PropertyName._creatureNode, Variant.From(in _creatureNode));
		info.AddProperty(PropertyName._curTween, Variant.From(in _curTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsLocal, out var value))
		{
			IsLocal = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._orbContainer, out var value2))
		{
			_orbContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._creatureNode, out var value3))
		{
			_creatureNode = value3.As<NCreature>();
		}
		if (info.TryGetProperty(PropertyName._curTween, out var value4))
		{
			_curTween = value4.As<Tween>();
		}
	}
}
