using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardTrailVfx.cs")]
public class NCardTrailVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName StopParticles = "StopParticles";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _nodeToFollow = "_nodeToFollow";

		public static readonly StringName _sprites = "_sprites";

		public static readonly StringName _updateSprites = "_updateSprites";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private Control _nodeToFollow;

	private Node2D _sprites;

	private bool _updateSprites = true;

	private Tween? _tween;

	public static NCardTrailVfx? Create(Control card, string characterTrailPath)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardTrailVfx nCardTrailVfx = PreloadManager.Cache.GetScene(characterTrailPath).Instantiate<NCardTrailVfx>(PackedScene.GenEditState.Disabled);
		nCardTrailVfx._nodeToFollow = card;
		return nCardTrailVfx;
	}

	public override void _Ready()
	{
		if (NCombatUi.IsDebugHidingPlayContainer)
		{
			base.Visible = false;
		}
		_sprites = GetNode<Node2D>("Sprites");
		_sprites.Modulate = StsColors.transparentWhite;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_sprites, "scale", Vector2.One * 0.5f, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Cubic)
			.SetDelay(0.25);
		_tween.TweenProperty(_nodeToFollow, "modulate:a", 0.75f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_tween.TweenProperty(_sprites, "modulate:a", 1f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public override void _Process(double delta)
	{
		if (_updateSprites)
		{
			base.GlobalPosition = _nodeToFollow.GlobalPosition;
			base.Rotation = _nodeToFollow.Rotation;
		}
	}

	public async Task FadeOut()
	{
		_updateSprites = false;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(this, "modulate:a", 0f, 0.5);
		StopParticles(_tween);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	private void StopParticles(Tween tween)
	{
		foreach (Node child in _sprites.GetChildren())
		{
			if (child is CpuParticles2D cpuParticles2D)
			{
				tween.TweenProperty(cpuParticles2D, "amount", 1, 0.5);
			}
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.String, "characterTrailPath", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tween", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NCardTrailVfx>(Create(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopParticles && args.Count == 1)
		{
			StopParticles(VariantUtils.ConvertTo<Tween>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NCardTrailVfx>(Create(VariantUtils.ConvertTo<Control>(in args[0]), VariantUtils.ConvertTo<string>(in args[1])));
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.StopParticles)
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
		if (name == PropertyName._nodeToFollow)
		{
			_nodeToFollow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._sprites)
		{
			_sprites = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._updateSprites)
		{
			_updateSprites = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._nodeToFollow)
		{
			value = VariantUtils.CreateFrom(in _nodeToFollow);
			return true;
		}
		if (name == PropertyName._sprites)
		{
			value = VariantUtils.CreateFrom(in _sprites);
			return true;
		}
		if (name == PropertyName._updateSprites)
		{
			value = VariantUtils.CreateFrom(in _updateSprites);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nodeToFollow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sprites, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._updateSprites, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._nodeToFollow, Variant.From(in _nodeToFollow));
		info.AddProperty(PropertyName._sprites, Variant.From(in _sprites));
		info.AddProperty(PropertyName._updateSprites, Variant.From(in _updateSprites));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._nodeToFollow, out var value))
		{
			_nodeToFollow = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._sprites, out var value2))
		{
			_sprites = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._updateSprites, out var value3))
		{
			_updateSprites = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value4))
		{
			_tween = value4.As<Tween>();
		}
	}
}
