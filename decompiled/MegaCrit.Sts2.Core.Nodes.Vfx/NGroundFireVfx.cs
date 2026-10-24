using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NGroundFireVfx.cs")]
public class NGroundFireVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ApplyColor' method.
		/// </summary>
		public static readonly StringName ApplyColor = "ApplyColor";

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
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_mainFire' field.
		/// </summary>
		public static readonly StringName _mainFire = "_mainFire";

		/// <summary>
		/// Cached name for the '_ember' field.
		/// </summary>
		public static readonly StringName _ember = "_ember";

		/// <summary>
		/// Cached name for the '_flameSprites' field.
		/// </summary>
		public static readonly StringName _flameSprites = "_flameSprites";

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

	private static readonly StringName _outerColor = new StringName("OuterColor");

	private static readonly StringName _innerColor = new StringName("InnerColor");

	private Tween? _tween;

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/fires/vfx_ground_fire");

	private Node2D _mainFire;

	private GpuParticles2D _ember;

	private GpuParticles2D _flameSprites;

	private VfxColor _vfxColor;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NGroundFireVfx? Create(Creature target, VfxColor color = VfxColor.Red)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature creatureNode = NCombatRoom.Instance.GetCreatureNode(target);
		if (creatureNode == null)
		{
			return null;
		}
		NGroundFireVfx nGroundFireVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NGroundFireVfx>(PackedScene.GenEditState.Disabled);
		nGroundFireVfx._vfxColor = color;
		nGroundFireVfx.GlobalPosition = creatureNode.GetBottomOfHitbox();
		return nGroundFireVfx;
	}

	public override void _Ready()
	{
		_mainFire = GetNode<Node2D>("MainFire");
		_ember = GetNode<GpuParticles2D>("Ember");
		_flameSprites = GetNode<GpuParticles2D>("FlameSprites");
		ApplyColor();
		TaskHelper.RunSafely(AnimateIn());
	}

	private void ApplyColor()
	{
		if (_vfxColor != VfxColor.Red)
		{
			Color color = Colors.White;
			Color color2 = Colors.White;
			Color color3 = new Color("541b00");
			switch (_vfxColor)
			{
			case VfxColor.Green:
				color = new Color("2fa800");
				color2 = new Color("06a000");
				color3 = new Color("541b00");
				break;
			case VfxColor.Blue:
				color = new Color("0099cd");
				color2 = new Color("00a3bf");
				color3 = Colors.Black;
				break;
			case VfxColor.Purple:
				color = new Color("7821ff");
				color2 = new Color("3f21ff");
				color3 = new Color("541b00");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case VfxColor.Black:
			case VfxColor.White:
				break;
			}
			Node node = _mainFire.GetNode("VfxAdditiveStepFire");
			ShaderMaterial shaderMaterial = (ShaderMaterial)node.GetNode<Node2D>("SteppedFireMix").Material;
			shaderMaterial.SetShaderParameter(_outerColor, color);
			shaderMaterial.SetShaderParameter(_innerColor, color2);
			shaderMaterial = (ShaderMaterial)node.GetNode<Node2D>("SteppedFireAdd").Material;
			shaderMaterial.SetShaderParameter(_outerColor, color3);
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task AnimateIn()
	{
		_mainFire.Modulate = Colors.Transparent;
		_mainFire.Scale = Vector2.Zero;
		_ember.Emitting = true;
		Task emberDone = _ember.AwaitSignal(GpuParticles2D.SignalName.Finished, this);
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_mainFire, "scale", Vector2.One * 4f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_tween.TweenProperty(_mainFire, "modulate:a", 0.9f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		if (await _tween.AwaitFinished(this))
		{
			_flameSprites.Emitting = true;
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(_mainFire, "modulate:a", 0f, 0.5);
			_tween.TweenProperty(_flameSprites, "modulate:a", 0f, 0.5);
			_tween.TweenProperty(_mainFire, "scale", Vector2.One * 2f, 2.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
			if (await _tween.AwaitFinished(this))
			{
				_flameSprites.Emitting = false;
				await emberDone;
				this.QueueFreeSafely();
			}
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ApplyColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyColor && args.Count == 0)
		{
			ApplyColor();
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ApplyColor)
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
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._mainFire)
		{
			_mainFire = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._ember)
		{
			_ember = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._flameSprites)
		{
			_flameSprites = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._mainFire)
		{
			value = VariantUtils.CreateFrom(in _mainFire);
			return true;
		}
		if (name == PropertyName._ember)
		{
			value = VariantUtils.CreateFrom(in _ember);
			return true;
		}
		if (name == PropertyName._flameSprites)
		{
			value = VariantUtils.CreateFrom(in _flameSprites);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mainFire, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ember, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameSprites, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._vfxColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._mainFire, Variant.From(in _mainFire));
		info.AddProperty(PropertyName._ember, Variant.From(in _ember));
		info.AddProperty(PropertyName._flameSprites, Variant.From(in _flameSprites));
		info.AddProperty(PropertyName._vfxColor, Variant.From(in _vfxColor));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._mainFire, out var value2))
		{
			_mainFire = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._ember, out var value3))
		{
			_ember = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._flameSprites, out var value4))
		{
			_flameSprites = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._vfxColor, out var value5))
		{
			_vfxColor = value5.As<VfxColor>();
		}
	}
}
