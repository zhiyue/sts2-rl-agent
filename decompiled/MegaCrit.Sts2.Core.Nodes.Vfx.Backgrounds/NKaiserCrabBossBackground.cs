using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Backgrounds;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/Backgrounds/NKaiserCrabBossBackground.cs")]
public class NKaiserCrabBossBackground : Node
{
	private enum RightArmState
	{
		Default,
		Charging,
		Resting
	}

	public enum ArmSide
	{
		Left,
		Right
	}

	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName PlayHurtAnim = "PlayHurtAnim";

		public static readonly StringName PlayArmDeathAnim = "PlayArmDeathAnim";

		public static readonly StringName PlayBodyDeathAnim = "PlayBodyDeathAnim";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _leftArm = "_leftArm";

		public static readonly StringName _rightArm = "_rightArm";

		public static readonly StringName _rightArmState = "_rightArmState";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const int _bodyTrack = 0;

	private const int _leftArmTrack = 1;

	private const int _rightArmTrack = 2;

	private const int _reactionTrack = 3;

	private MegaSprite _animController;

	private Node2D _leftArm;

	private Node2D _rightArm;

	private RightArmState _rightArmState;

	public override void _Ready()
	{
		_animController = new MegaSprite(GetNode<Node2D>("%Visuals"));
		_leftArm = GetNode<Node2D>("%ArmBoneL");
		_rightArm = GetNode<Node2D>("%ArmBoneR");
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("right/idle_loop", loop: true, 2);
		animationState.SetAnimation("body/idle_loop");
		animationState.SetAnimation("left/idle_loop", loop: true, 1);
	}

	public async Task PlayAttackAnim(ArmSide side, string animation, float duration)
	{
		string text = side.ToString().ToLowerInvariant();
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation(text + "/" + animation, loop: false, (side == ArmSide.Left) ? 1 : 2);
		animationState.SetAnimation("reactions/attack_" + text, loop: false, 3);
		animationState.AddAnimation(text + "/idle_loop", 0f, loop: true, (side == ArmSide.Left) ? 1 : 2);
		await Cmd.Wait(duration);
	}

	public void PlayHurtAnim(ArmSide side)
	{
		string text = side.ToString().ToLowerInvariant();
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("reactions/hurt_" + text, loop: false, 3);
		if (side == ArmSide.Left)
		{
			animationState.SetAnimation(text + "/hurt", loop: false, 1);
			animationState.AddAnimation(text + "/idle_loop", 0f, loop: true, 1);
			return;
		}
		switch (_rightArmState)
		{
		case RightArmState.Default:
			animationState.SetAnimation(text + "/hurt", loop: false, 2);
			animationState.AddAnimation("right/idle_loop", 0f, loop: true, 2);
			break;
		case RightArmState.Charging:
			animationState.SetAnimation(text + "/hurt_charged", loop: false, 2);
			animationState.AddAnimation("right/charged_loop", 0f, loop: true, 2);
			break;
		case RightArmState.Resting:
			animationState.SetAnimation(text + "/hurt_resting", loop: false, 2);
			animationState.AddAnimation("right/rest_loop", 0f, loop: true, 2);
			break;
		}
	}

	public void PlayArmDeathAnim(ArmSide side)
	{
		string text = side.ToString().ToLowerInvariant();
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("reactions/hurt_" + text, loop: false, 3);
		if (side == ArmSide.Left)
		{
			animationState.SetAnimation(text + "/die", loop: false, 1);
			return;
		}
		switch (_rightArmState)
		{
		case RightArmState.Default:
		case RightArmState.Charging:
			animationState.SetAnimation("right/die", loop: false, 2);
			break;
		case RightArmState.Resting:
			animationState.SetAnimation("right/die_resting", loop: false, 2);
			break;
		}
	}

	public async Task PlayRightSideChargeUpAnim(float duration)
	{
		_rightArmState = RightArmState.Charging;
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("right/charge_up", loop: false, 2);
		animationState.AddAnimation("right/charged_loop", 0f, loop: true, 2);
		animationState.SetAnimation("reactions/attack_right", loop: false, 3);
		await Cmd.Wait(duration);
	}

	public async Task PlayRightSideHeavy(float duration)
	{
		_rightArmState = RightArmState.Resting;
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("right/attack_heavy", loop: false, 2);
		animationState.AddAnimation("right/rest_loop", 0f, loop: true, 2);
		animationState.SetAnimation("reactions/attack_right", loop: false, 3);
		await Cmd.Wait(duration);
	}

	public async Task PlayRightRecharge(float duration)
	{
		_rightArmState = RightArmState.Default;
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("right/wake_up", loop: false, 2);
		animationState.AddAnimation("right/idle_loop", 0f, loop: true, 2);
		await Cmd.Wait(duration);
	}

	public void PlayBodyDeathAnim()
	{
		MegaAnimationState animationState = _animController.GetAnimationState();
		animationState.SetAnimation("body/die", loop: false);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayHurtAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "side", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayArmDeathAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "side", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayBodyDeathAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.PlayHurtAnim && args.Count == 1)
		{
			PlayHurtAnim(VariantUtils.ConvertTo<ArmSide>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayArmDeathAnim && args.Count == 1)
		{
			PlayArmDeathAnim(VariantUtils.ConvertTo<ArmSide>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayBodyDeathAnim && args.Count == 0)
		{
			PlayBodyDeathAnim();
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
		if (method == MethodName.PlayHurtAnim)
		{
			return true;
		}
		if (method == MethodName.PlayArmDeathAnim)
		{
			return true;
		}
		if (method == MethodName.PlayBodyDeathAnim)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._leftArm)
		{
			_leftArm = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._rightArm)
		{
			_rightArm = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._rightArmState)
		{
			_rightArmState = VariantUtils.ConvertTo<RightArmState>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._leftArm)
		{
			value = VariantUtils.CreateFrom(in _leftArm);
			return true;
		}
		if (name == PropertyName._rightArm)
		{
			value = VariantUtils.CreateFrom(in _rightArm);
			return true;
		}
		if (name == PropertyName._rightArmState)
		{
			value = VariantUtils.CreateFrom(in _rightArmState);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._leftArm, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rightArm, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._rightArmState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._leftArm, Variant.From(in _leftArm));
		info.AddProperty(PropertyName._rightArm, Variant.From(in _rightArm));
		info.AddProperty(PropertyName._rightArmState, Variant.From(in _rightArmState));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._leftArm, out var value))
		{
			_leftArm = value.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._rightArm, out var value2))
		{
			_rightArm = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._rightArmState, out var value3))
		{
			_rightArmState = value3.As<RightArmState>();
		}
	}
}
