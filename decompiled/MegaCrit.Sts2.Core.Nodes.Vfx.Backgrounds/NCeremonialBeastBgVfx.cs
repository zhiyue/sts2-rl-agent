using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/Backgrounds/NCeremonialBeastBgVfx.cs")]
public class NCeremonialBeastBgVfx : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName PlayGlow = "PlayGlow";

		public static readonly StringName PlaySkulls = "PlaySkulls";

		public static readonly StringName PlayFlowers = "PlayFlowers";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _isGlowOn = "_isGlowOn";

		public static readonly StringName _areSkullsOn = "_areSkullsOn";

		public static readonly StringName _parent = "_parent";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private bool _isGlowOn;

	private bool _areSkullsOn;

	private Node2D _parent;

	private MegaSprite _animController;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_parent.Visible = false;
		_animController = new MegaSprite(_parent);
	}

	public override void _EnterTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged += UpdateState;
	}

	public override void _ExitTree()
	{
		CombatManager.Instance.StateTracker.CombatStateChanged -= UpdateState;
	}

	private void UpdateState(CombatState combatState)
	{
		UpdateRingingSfx(combatState);
		UpdateVfxAndMusic(combatState);
	}

	private void UpdateRingingSfx(CombatState combatState)
	{
		bool flag = LocalContext.GetMe(combatState).Creature.HasPower<RingingPower>();
		NRunMusicController.Instance?.UpdateMusicParameter("ringing", flag ? 1 : 0);
	}

	private void UpdateVfxAndMusic(CombatState combatState)
	{
		Creature creature = combatState.Creatures.FirstOrDefault((Creature c) => c.Monster is CeremonialBeast);
		if (creature == null)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("ceremonial_beast_progress", 5f);
			PlayFlowers();
			return;
		}
		if ((float)creature.CurrentHp > (float)creature.MaxHp * 0.66f)
		{
			_parent.Visible = false;
			return;
		}
		_parent.Visible = true;
		if ((float)creature.CurrentHp > (float)creature.MaxHp * 0.33f)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("ceremonial_beast_progress", 1f);
			PlayGlow();
		}
		else if (creature.IsAlive)
		{
			NRunMusicController.Instance?.UpdateMusicParameter("ceremonial_beast_progress", 1f);
			PlaySkulls();
		}
		else
		{
			NRunMusicController.Instance?.UpdateMusicParameter("ceremonial_beast_progress", 5f);
			PlayFlowers();
		}
	}

	private void PlayGlow()
	{
		if (!_isGlowOn)
		{
			_isGlowOn = true;
			MegaAnimationState animationState = _animController.GetAnimationState();
			animationState.SetAnimation("glow_spawn");
			animationState.AddAnimation("glow_idle");
		}
	}

	private void PlaySkulls()
	{
		if (!_areSkullsOn)
		{
			_areSkullsOn = true;
			MegaAnimationState animationState = _animController.GetAnimationState();
			animationState.SetAnimation("skulls_spawn");
			animationState.AddAnimation("glow_and_skulls_idle");
		}
	}

	private void PlayFlowers()
	{
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("glow_and_skulls_idle");
		animationState.AddAnimation("plants_spawn", 4.5f, loop: false);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlaySkulls, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayFlowers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PlayGlow && args.Count == 0)
		{
			PlayGlow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlaySkulls && args.Count == 0)
		{
			PlaySkulls();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayFlowers && args.Count == 0)
		{
			PlayFlowers();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
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
		if (method == MethodName.PlayGlow)
		{
			return true;
		}
		if (method == MethodName.PlaySkulls)
		{
			return true;
		}
		if (method == MethodName.PlayFlowers)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._isGlowOn)
		{
			_isGlowOn = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._areSkullsOn)
		{
			_areSkullsOn = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._isGlowOn)
		{
			value = VariantUtils.CreateFrom(in _isGlowOn);
			return true;
		}
		if (name == PropertyName._areSkullsOn)
		{
			value = VariantUtils.CreateFrom(in _areSkullsOn);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isGlowOn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._areSkullsOn, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._isGlowOn, Variant.From(in _isGlowOn));
		info.AddProperty(PropertyName._areSkullsOn, Variant.From(in _areSkullsOn));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._isGlowOn, out var value))
		{
			_isGlowOn = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._areSkullsOn, out var value2))
		{
			_areSkullsOn = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value3))
		{
			_parent = value3.As<Node2D>();
		}
	}
}
