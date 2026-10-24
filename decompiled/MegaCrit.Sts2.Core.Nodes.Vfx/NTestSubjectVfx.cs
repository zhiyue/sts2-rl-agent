using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[GlobalClass]
[ScriptPath("res://src/Core/Nodes/Vfx/NTestSubjectVfx.cs")]
public class NTestSubjectVfx : Node
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnAnimationEvent' method.
		/// </summary>
		public static readonly StringName OnAnimationEvent = "OnAnimationEvent";

		/// <summary>
		/// Cached name for the 'PlayAnim1' method.
		/// </summary>
		public static readonly StringName PlayAnim1 = "PlayAnim1";

		/// <summary>
		/// Cached name for the 'SquirtNeck' method.
		/// </summary>
		public static readonly StringName SquirtNeck = "SquirtNeck";

		/// <summary>
		/// Cached name for the 'StartDizzies' method.
		/// </summary>
		public static readonly StringName StartDizzies = "StartDizzies";

		/// <summary>
		/// Cached name for the 'EndDizzies' method.
		/// </summary>
		public static readonly StringName EndDizzies = "EndDizzies";

		/// <summary>
		/// Cached name for the 'StartEmbers' method.
		/// </summary>
		public static readonly StringName StartEmbers = "StartEmbers";

		/// <summary>
		/// Cached name for the 'StartFlames' method.
		/// </summary>
		public static readonly StringName StartFlames = "StartFlames";

		/// <summary>
		/// Cached name for the 'EndFlames' method.
		/// </summary>
		public static readonly StringName EndFlames = "EndFlames";

		/// <summary>
		/// Cached name for the 'StartBurnVfx' method.
		/// </summary>
		public static readonly StringName StartBurnVfx = "StartBurnVfx";

		/// <summary>
		/// Cached name for the 'EndBurnVfx' method.
		/// </summary>
		public static readonly StringName EndBurnVfx = "EndBurnVfx";

		/// <summary>
		/// Cached name for the 'StartCeilingSparks' method.
		/// </summary>
		public static readonly StringName StartCeilingSparks = "StartCeilingSparks";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node.PropertyName
	{
		/// <summary>
		/// Cached name for the '_neckParticles' field.
		/// </summary>
		public static readonly StringName _neckParticles = "_neckParticles";

		/// <summary>
		/// Cached name for the '_dizzyParticles' field.
		/// </summary>
		public static readonly StringName _dizzyParticles = "_dizzyParticles";

		/// <summary>
		/// Cached name for the '_emberParticles' field.
		/// </summary>
		public static readonly StringName _emberParticles = "_emberParticles";

		/// <summary>
		/// Cached name for the '_flameParticles' field.
		/// </summary>
		public static readonly StringName _flameParticles = "_flameParticles";

		/// <summary>
		/// Cached name for the '_burnParticles' field.
		/// </summary>
		public static readonly StringName _burnParticles = "_burnParticles";

		/// <summary>
		/// Cached name for the '_targetedBurnParticle' field.
		/// </summary>
		public static readonly StringName _targetedBurnParticle = "_targetedBurnParticle";

		/// <summary>
		/// Cached name for the '_burnParticleFountain' field.
		/// </summary>
		public static readonly StringName _burnParticleFountain = "_burnParticleFountain";

		/// <summary>
		/// Cached name for the '_ceilingParticles' field.
		/// </summary>
		public static readonly StringName _ceilingParticles = "_ceilingParticles";

		/// <summary>
		/// Cached name for the '_parent' field.
		/// </summary>
		public static readonly StringName _parent = "_parent";

		/// <summary>
		/// Cached name for the '_keyDown' field.
		/// </summary>
		public static readonly StringName _keyDown = "_keyDown";

		/// <summary>
		/// Cached name for the '_doingThing' field.
		/// </summary>
		public static readonly StringName _doingThing = "_doingThing";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node.SignalName
	{
	}

	private GpuParticles2D _neckParticles;

	private GpuParticles2D _dizzyParticles;

	private GpuParticles2D _emberParticles;

	private GpuParticles2D _flameParticles;

	private GpuParticles2D _burnParticles;

	private GpuParticles2D _targetedBurnParticle;

	private GpuParticles2D _burnParticleFountain;

	private GpuParticles2D _ceilingParticles;

	private Node2D _parent;

	private MegaSprite _animController;

	private MegaSprite _frontBurnVfxController;

	private MegaSprite _backBurnVfxController;

	private bool _keyDown;

	private bool _doingThing;

	public override void _Ready()
	{
		_parent = GetParent<Node2D>();
		_animController = new MegaSprite(_parent);
		_frontBurnVfxController = new MegaSprite(GetNode("../FrontBurnVfxSlot/FrontBurnVfx"));
		_backBurnVfxController = new MegaSprite(GetNode("../BackBurnVfxSlot/BackBurnVfx"));
		_animController.ConnectAnimationEvent(Callable.From<GodotObject, GodotObject, GodotObject, GodotObject>(OnAnimationEvent));
		_neckParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/NeckParticles");
		_dizzyParticles = _parent.GetNode<GpuParticles2D>("NeckParticlesSlot/DizzyPaticles");
		_emberParticles = _parent.GetNode<GpuParticles2D>("../../EmberParticles");
		_flameParticles = _parent.GetNode<GpuParticles2D>("../../FlameParticles");
		_burnParticles = _parent.GetNode<GpuParticles2D>("../../BurnParticles");
		_targetedBurnParticle = _parent.GetNode<GpuParticles2D>("../../TargetedBurnParticle");
		_burnParticleFountain = _parent.GetNode<GpuParticles2D>("../../BurnParticleFountain");
		_ceilingParticles = _parent.GetNode<GpuParticles2D>("../../CeilingSparks");
		_neckParticles.OneShot = true;
		_neckParticles.Emitting = false;
		_dizzyParticles.Emitting = false;
		_emberParticles.OneShot = true;
		_emberParticles.Emitting = false;
		_flameParticles.Emitting = false;
		_burnParticles.Emitting = false;
		_targetedBurnParticle.Emitting = false;
		_burnParticleFountain.Emitting = false;
		_ceilingParticles.OneShot = true;
		_ceilingParticles.Emitting = false;
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("idle_loop3");
		});
		this.RunWhenSpineReady(_frontBurnVfxController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("empty");
		});
		this.RunWhenSpineReady(_backBurnVfxController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("empty");
		});
	}

	private void OnAnimationEvent(GodotObject _, GodotObject __, GodotObject ___, GodotObject spineEvent)
	{
		string eventName = new MegaEvent(spineEvent).GetData().GetEventName();
		if (eventName == null)
		{
			return;
		}
		switch (eventName.Length)
		{
		case 12:
			switch (eventName[6])
			{
			case 'x':
				if (eventName == "neck_explode")
				{
					SquirtNeck();
				}
				break;
			case 'e':
				if (eventName == "start_embers")
				{
					StartEmbers();
				}
				break;
			case 'f':
				if (eventName == "start_flames")
				{
					StartFlames();
				}
				break;
			case 'r':
				if (eventName == "end_burn_vfx")
				{
					EndBurnVfx();
				}
				break;
			}
			break;
		case 13:
			if (eventName == "start_dizzies")
			{
				StartDizzies();
			}
			break;
		case 11:
			if (eventName == "end_dizzies")
			{
				EndDizzies();
			}
			break;
		case 10:
			if (eventName == "end_flames")
			{
				EndFlames();
			}
			break;
		case 14:
			if (eventName == "start_burn_vfx")
			{
				StartBurnVfx();
			}
			break;
		case 20:
			if (eventName == "start_ceiling_sparks")
			{
				StartCeilingSparks();
			}
			break;
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
			break;
		}
	}

	private void PlayAnim1()
	{
		_animController.GetAnimationState().SetAnimation("die3", loop: false);
		_animController.GetAnimationState().AddAnimation("idle_loop3");
	}

	private void SquirtNeck()
	{
		_neckParticles.Restart();
	}

	private void StartDizzies()
	{
		if (!_dizzyParticles.Emitting)
		{
			_dizzyParticles.Emitting = true;
		}
	}

	private void EndDizzies()
	{
		_dizzyParticles.Emitting = false;
	}

	private void StartEmbers()
	{
		_emberParticles.Restart();
	}

	private void StartFlames()
	{
		_flameParticles.Emitting = true;
	}

	private void EndFlames()
	{
		_flameParticles.Emitting = false;
	}

	private void StartBurnVfx()
	{
		_frontBurnVfxController.GetAnimationState().SetAnimation("burn", loop: false);
		_backBurnVfxController.GetAnimationState().SetAnimation("burn", loop: false);
		_burnParticles.Restart();
		_targetedBurnParticle.Emitting = true;
		_burnParticleFountain.Restart();
	}

	private void EndBurnVfx()
	{
		_burnParticles.Emitting = false;
		_targetedBurnParticle.Emitting = false;
		_burnParticleFountain.Emitting = false;
	}

	private void StartCeilingSparks()
	{
		_ceilingParticles.Restart();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAnimationEvent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "__", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "___", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false),
			new PropertyInfo(Variant.Type.Object, "spineEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Object"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayAnim1, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SquirtNeck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndDizzies, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartEmbers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartBurnVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndBurnVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartCeilingSparks, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnAnimationEvent && args.Count == 4)
		{
			OnAnimationEvent(VariantUtils.ConvertTo<GodotObject>(in args[0]), VariantUtils.ConvertTo<GodotObject>(in args[1]), VariantUtils.ConvertTo<GodotObject>(in args[2]), VariantUtils.ConvertTo<GodotObject>(in args[3]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayAnim1 && args.Count == 0)
		{
			PlayAnim1();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SquirtNeck && args.Count == 0)
		{
			SquirtNeck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartDizzies && args.Count == 0)
		{
			StartDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndDizzies && args.Count == 0)
		{
			EndDizzies();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartEmbers && args.Count == 0)
		{
			StartEmbers();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartFlames && args.Count == 0)
		{
			StartFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndFlames && args.Count == 0)
		{
			EndFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartBurnVfx && args.Count == 0)
		{
			StartBurnVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndBurnVfx && args.Count == 0)
		{
			EndBurnVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartCeilingSparks && args.Count == 0)
		{
			StartCeilingSparks();
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
		if (method == MethodName.OnAnimationEvent)
		{
			return true;
		}
		if (method == MethodName.PlayAnim1)
		{
			return true;
		}
		if (method == MethodName.SquirtNeck)
		{
			return true;
		}
		if (method == MethodName.StartDizzies)
		{
			return true;
		}
		if (method == MethodName.EndDizzies)
		{
			return true;
		}
		if (method == MethodName.StartEmbers)
		{
			return true;
		}
		if (method == MethodName.StartFlames)
		{
			return true;
		}
		if (method == MethodName.EndFlames)
		{
			return true;
		}
		if (method == MethodName.StartBurnVfx)
		{
			return true;
		}
		if (method == MethodName.EndBurnVfx)
		{
			return true;
		}
		if (method == MethodName.StartCeilingSparks)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._neckParticles)
		{
			_neckParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			_dizzyParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			_emberParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			_flameParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._burnParticles)
		{
			_burnParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._targetedBurnParticle)
		{
			_targetedBurnParticle = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._burnParticleFountain)
		{
			_burnParticleFountain = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._ceilingParticles)
		{
			_ceilingParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._parent)
		{
			_parent = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			_keyDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			_doingThing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._neckParticles)
		{
			value = VariantUtils.CreateFrom(in _neckParticles);
			return true;
		}
		if (name == PropertyName._dizzyParticles)
		{
			value = VariantUtils.CreateFrom(in _dizzyParticles);
			return true;
		}
		if (name == PropertyName._emberParticles)
		{
			value = VariantUtils.CreateFrom(in _emberParticles);
			return true;
		}
		if (name == PropertyName._flameParticles)
		{
			value = VariantUtils.CreateFrom(in _flameParticles);
			return true;
		}
		if (name == PropertyName._burnParticles)
		{
			value = VariantUtils.CreateFrom(in _burnParticles);
			return true;
		}
		if (name == PropertyName._targetedBurnParticle)
		{
			value = VariantUtils.CreateFrom(in _targetedBurnParticle);
			return true;
		}
		if (name == PropertyName._burnParticleFountain)
		{
			value = VariantUtils.CreateFrom(in _burnParticleFountain);
			return true;
		}
		if (name == PropertyName._ceilingParticles)
		{
			value = VariantUtils.CreateFrom(in _ceilingParticles);
			return true;
		}
		if (name == PropertyName._parent)
		{
			value = VariantUtils.CreateFrom(in _parent);
			return true;
		}
		if (name == PropertyName._keyDown)
		{
			value = VariantUtils.CreateFrom(in _keyDown);
			return true;
		}
		if (name == PropertyName._doingThing)
		{
			value = VariantUtils.CreateFrom(in _doingThing);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._neckParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dizzyParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emberParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flameParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._targetedBurnParticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._burnParticleFountain, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ceilingParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._keyDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._doingThing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._neckParticles, Variant.From(in _neckParticles));
		info.AddProperty(PropertyName._dizzyParticles, Variant.From(in _dizzyParticles));
		info.AddProperty(PropertyName._emberParticles, Variant.From(in _emberParticles));
		info.AddProperty(PropertyName._flameParticles, Variant.From(in _flameParticles));
		info.AddProperty(PropertyName._burnParticles, Variant.From(in _burnParticles));
		info.AddProperty(PropertyName._targetedBurnParticle, Variant.From(in _targetedBurnParticle));
		info.AddProperty(PropertyName._burnParticleFountain, Variant.From(in _burnParticleFountain));
		info.AddProperty(PropertyName._ceilingParticles, Variant.From(in _ceilingParticles));
		info.AddProperty(PropertyName._parent, Variant.From(in _parent));
		info.AddProperty(PropertyName._keyDown, Variant.From(in _keyDown));
		info.AddProperty(PropertyName._doingThing, Variant.From(in _doingThing));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._neckParticles, out var value))
		{
			_neckParticles = value.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._dizzyParticles, out var value2))
		{
			_dizzyParticles = value2.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._emberParticles, out var value3))
		{
			_emberParticles = value3.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._flameParticles, out var value4))
		{
			_flameParticles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._burnParticles, out var value5))
		{
			_burnParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._targetedBurnParticle, out var value6))
		{
			_targetedBurnParticle = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._burnParticleFountain, out var value7))
		{
			_burnParticleFountain = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._ceilingParticles, out var value8))
		{
			_ceilingParticles = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._parent, out var value9))
		{
			_parent = value9.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._keyDown, out var value10))
		{
			_keyDown = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._doingThing, out var value11))
		{
			_doingThing = value11.As<bool>();
		}
	}
}
