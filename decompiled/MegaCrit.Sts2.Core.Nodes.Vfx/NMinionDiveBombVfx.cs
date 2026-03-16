using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.Collections;
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

[ScriptPath("res://src/Core/Nodes/Vfx/NMinionDiveBombVfx.cs")]
public class NMinionDiveBombVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public static readonly StringName Initialize = "Initialize";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetMinionVisible = "SetMinionVisible";

		public static readonly StringName UpdateMinionSprite = "UpdateMinionSprite";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName SourceFinalPosition = "SourceFinalPosition";

		public static readonly StringName DestinationFinalPosition = "DestinationFinalPosition";

		public static readonly StringName _minionSprite = "_minionSprite";

		public static readonly StringName _minionTextures = "_minionTextures";

		public static readonly StringName _minionAnimator = "_minionAnimator";

		public static readonly StringName _minionAnimations = "_minionAnimations";

		public static readonly StringName _minionVfx = "_minionVfx";

		public static readonly StringName _fallingTrail = "_fallingTrail";

		public static readonly StringName _fallingVfx = "_fallingVfx";

		public static readonly StringName _impactVfx = "_impactVfx";

		public static readonly StringName _flightTime = "_flightTime";

		public static readonly StringName _fallingVfxEntryTime = "_fallingVfxEntryTime";

		public static readonly StringName _horizontalCurve = "_horizontalCurve";

		public static readonly StringName _verticalCurve = "_verticalCurve";

		public static readonly StringName _textureCurve = "_textureCurve";

		public static readonly StringName _maxHeight = "_maxHeight";

		public static readonly StringName _sourceOffset = "_sourceOffset";

		public static readonly StringName _destinationOffset = "_destinationOffset";

		public static readonly StringName _previousIndex = "_previousIndex";

		public static readonly StringName _sourcePosition = "_sourcePosition";

		public static readonly StringName _destinationPosition = "_destinationPosition";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/vfx_minion_dive_bomb");

	[Export(PropertyHint.None, "")]
	private Sprite2D? _minionSprite;

	[Export(PropertyHint.None, "")]
	private Array<Texture2D>? _minionTextures;

	[Export(PropertyHint.None, "")]
	private AnimationPlayer? _minionAnimator;

	[Export(PropertyHint.None, "")]
	private Array<string>? _minionAnimations;

	[Export(PropertyHint.None, "")]
	private Array<NParticlesContainer>? _minionVfx;

	[Export(PropertyHint.None, "")]
	private Node2D? _fallingTrail;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _fallingVfx;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer? _impactVfx;

	[Export(PropertyHint.None, "")]
	private float _flightTime;

	[Export(PropertyHint.None, "")]
	private float _fallingVfxEntryTime;

	[Export(PropertyHint.None, "")]
	private Curve? _horizontalCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _verticalCurve;

	[Export(PropertyHint.None, "")]
	private Curve? _textureCurve;

	[Export(PropertyHint.None, "")]
	private float _maxHeight;

	[Export(PropertyHint.None, "")]
	private Vector2 _sourceOffset;

	[Export(PropertyHint.None, "")]
	private Vector2 _destinationOffset;

	private int _previousIndex = -1;

	private Vector2 _sourcePosition;

	private Vector2 _destinationPosition;

	private Vector2 SourceFinalPosition => _sourcePosition + _sourceOffset;

	private Vector2 DestinationFinalPosition => _destinationPosition + _destinationOffset;

	public static NMinionDiveBombVfx? Create(Creature owner, Creature target)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(owner);
		NCreature nCreature2 = NCombatRoom.Instance?.GetCreatureNode(target);
		if (nCreature2 != null && nCreature != null)
		{
			return Create(nCreature.VfxSpawnPosition, nCreature2.GetBottomOfHitbox());
		}
		return null;
	}

	public static NMinionDiveBombVfx? Create(Vector2 playerCenterPosition, Vector2 targetFloorPosition)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NMinionDiveBombVfx nMinionDiveBombVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NMinionDiveBombVfx>(PackedScene.GenEditState.Disabled);
		nMinionDiveBombVfx.Initialize(playerCenterPosition, targetFloorPosition);
		return nMinionDiveBombVfx;
	}

	private void Initialize(Vector2 sourcePosition, Vector2 destinationPosition)
	{
		_sourcePosition = sourcePosition;
		_destinationPosition = destinationPosition;
		_fallingTrail.Visible = true;
		base.GlobalPosition = sourcePosition;
		for (int i = 0; i < _minionVfx.Count; i++)
		{
			_minionVfx[i].SetEmitting(emitting: false);
		}
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlaySequence());
	}

	private void SetMinionVisible(bool visible)
	{
		_minionSprite.SelfModulate = (visible ? new Color(1f, 1f, 1f) : new Color(1f, 1f, 1f, 0f));
	}

	private void UpdateMinionSprite(int index)
	{
		if (_previousIndex == index)
		{
			return;
		}
		_previousIndex = index;
		Texture2D texture = _minionTextures[Mathf.Clamp(index, 0, _minionTextures.Count - 1)];
		_minionSprite.Texture = texture;
		string text = _minionAnimations[Mathf.Clamp(index, 0, _minionAnimations.Count - 1)];
		if (!_minionAnimator.CurrentAnimation.Equals(text))
		{
			_minionAnimator.Play(text);
		}
		for (int i = 0; i < _minionVfx.Count; i++)
		{
			if (i == index)
			{
				_minionVfx[i].Restart();
			}
		}
	}

	private async Task PlaySequence()
	{
		Vector2 startPos = SourceFinalPosition;
		Vector2 endPos = DestinationFinalPosition;
		UpdateMinionSprite(0);
		_minionSprite.GlobalPosition = startPos;
		SetMinionVisible(visible: true);
		double timer = 0.0;
		bool isPlayingFallingVfx = false;
		while (timer < (double)_flightTime)
		{
			float offset = (float)timer / _flightTime;
			float weight = _horizontalCurve.Sample(offset);
			float num = _verticalCurve.Sample(offset);
			float s = _textureCurve.Sample(offset);
			UpdateMinionSprite(Mathf.FloorToInt(s));
			Vector2 globalPosition = startPos.Lerp(endPos, weight);
			globalPosition += Vector2.Up * num * _maxHeight;
			_minionSprite.GlobalPosition = globalPosition;
			_fallingVfx.GlobalPosition = globalPosition;
			if (timer >= (double)_fallingVfxEntryTime && !isPlayingFallingVfx)
			{
				_fallingVfx.Restart();
				_fallingTrail.Visible = true;
				isPlayingFallingVfx = true;
			}
			timer += GetProcessDeltaTime();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		SetMinionVisible(visible: false);
		NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
		_impactVfx.GlobalPosition = _destinationPosition;
		_impactVfx.Restart();
		_fallingTrail.Visible = false;
		_fallingVfx.SetEmitting(emitting: false);
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "playerCenterPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "targetFloorPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "sourcePosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "destinationPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetMinionVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "visible", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateMinionSprite, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NMinionDiveBombVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
			return true;
		}
		if (method == MethodName.Initialize && args.Count == 2)
		{
			Initialize(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetMinionVisible && args.Count == 1)
		{
			SetMinionVisible(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateMinionSprite && args.Count == 1)
		{
			UpdateMinionSprite(VariantUtils.ConvertTo<int>(in args[0]));
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
			ret = VariantUtils.CreateFrom<NMinionDiveBombVfx>(Create(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1])));
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
		if (method == MethodName.Initialize)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.SetMinionVisible)
		{
			return true;
		}
		if (method == MethodName.UpdateMinionSprite)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._minionSprite)
		{
			_minionSprite = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._minionTextures)
		{
			_minionTextures = VariantUtils.ConvertToArray<Texture2D>(in value);
			return true;
		}
		if (name == PropertyName._minionAnimator)
		{
			_minionAnimator = VariantUtils.ConvertTo<AnimationPlayer>(in value);
			return true;
		}
		if (name == PropertyName._minionAnimations)
		{
			_minionAnimations = VariantUtils.ConvertToArray<string>(in value);
			return true;
		}
		if (name == PropertyName._minionVfx)
		{
			_minionVfx = VariantUtils.ConvertToArray<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._fallingTrail)
		{
			_fallingTrail = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._fallingVfx)
		{
			_fallingVfx = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._impactVfx)
		{
			_impactVfx = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._flightTime)
		{
			_flightTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._fallingVfxEntryTime)
		{
			_fallingVfxEntryTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._horizontalCurve)
		{
			_horizontalCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._verticalCurve)
		{
			_verticalCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._textureCurve)
		{
			_textureCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._maxHeight)
		{
			_maxHeight = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._sourceOffset)
		{
			_sourceOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._destinationOffset)
		{
			_destinationOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._previousIndex)
		{
			_previousIndex = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._sourcePosition)
		{
			_sourcePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._destinationPosition)
		{
			_destinationPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Vector2 from;
		if (name == PropertyName.SourceFinalPosition)
		{
			from = SourceFinalPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DestinationFinalPosition)
		{
			from = DestinationFinalPosition;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._minionSprite)
		{
			value = VariantUtils.CreateFrom(in _minionSprite);
			return true;
		}
		if (name == PropertyName._minionTextures)
		{
			value = VariantUtils.CreateFromArray(_minionTextures);
			return true;
		}
		if (name == PropertyName._minionAnimator)
		{
			value = VariantUtils.CreateFrom(in _minionAnimator);
			return true;
		}
		if (name == PropertyName._minionAnimations)
		{
			value = VariantUtils.CreateFromArray(_minionAnimations);
			return true;
		}
		if (name == PropertyName._minionVfx)
		{
			value = VariantUtils.CreateFromArray(_minionVfx);
			return true;
		}
		if (name == PropertyName._fallingTrail)
		{
			value = VariantUtils.CreateFrom(in _fallingTrail);
			return true;
		}
		if (name == PropertyName._fallingVfx)
		{
			value = VariantUtils.CreateFrom(in _fallingVfx);
			return true;
		}
		if (name == PropertyName._impactVfx)
		{
			value = VariantUtils.CreateFrom(in _impactVfx);
			return true;
		}
		if (name == PropertyName._flightTime)
		{
			value = VariantUtils.CreateFrom(in _flightTime);
			return true;
		}
		if (name == PropertyName._fallingVfxEntryTime)
		{
			value = VariantUtils.CreateFrom(in _fallingVfxEntryTime);
			return true;
		}
		if (name == PropertyName._horizontalCurve)
		{
			value = VariantUtils.CreateFrom(in _horizontalCurve);
			return true;
		}
		if (name == PropertyName._verticalCurve)
		{
			value = VariantUtils.CreateFrom(in _verticalCurve);
			return true;
		}
		if (name == PropertyName._textureCurve)
		{
			value = VariantUtils.CreateFrom(in _textureCurve);
			return true;
		}
		if (name == PropertyName._maxHeight)
		{
			value = VariantUtils.CreateFrom(in _maxHeight);
			return true;
		}
		if (name == PropertyName._sourceOffset)
		{
			value = VariantUtils.CreateFrom(in _sourceOffset);
			return true;
		}
		if (name == PropertyName._destinationOffset)
		{
			value = VariantUtils.CreateFrom(in _destinationOffset);
			return true;
		}
		if (name == PropertyName._previousIndex)
		{
			value = VariantUtils.CreateFrom(in _previousIndex);
			return true;
		}
		if (name == PropertyName._sourcePosition)
		{
			value = VariantUtils.CreateFrom(in _sourcePosition);
			return true;
		}
		if (name == PropertyName._destinationPosition)
		{
			value = VariantUtils.CreateFrom(in _destinationPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._minionSprite, PropertyHint.NodeType, "Sprite2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._minionTextures, PropertyHint.TypeString, "24/17:Texture2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._minionAnimator, PropertyHint.NodeType, "AnimationPlayer", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._minionAnimations, PropertyHint.TypeString, "4/0:", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Array, PropertyName._minionVfx, PropertyHint.TypeString, "24/34:Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fallingTrail, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fallingVfx, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._impactVfx, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._flightTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._fallingVfxEntryTime, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._horizontalCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._verticalCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._textureCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._maxHeight, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._sourceOffset, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._destinationOffset, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._previousIndex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._sourcePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._destinationPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.SourceFinalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.DestinationFinalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._minionSprite, Variant.From(in _minionSprite));
		info.AddProperty(PropertyName._minionTextures, Variant.CreateFrom(_minionTextures));
		info.AddProperty(PropertyName._minionAnimator, Variant.From(in _minionAnimator));
		info.AddProperty(PropertyName._minionAnimations, Variant.CreateFrom(_minionAnimations));
		info.AddProperty(PropertyName._minionVfx, Variant.CreateFrom(_minionVfx));
		info.AddProperty(PropertyName._fallingTrail, Variant.From(in _fallingTrail));
		info.AddProperty(PropertyName._fallingVfx, Variant.From(in _fallingVfx));
		info.AddProperty(PropertyName._impactVfx, Variant.From(in _impactVfx));
		info.AddProperty(PropertyName._flightTime, Variant.From(in _flightTime));
		info.AddProperty(PropertyName._fallingVfxEntryTime, Variant.From(in _fallingVfxEntryTime));
		info.AddProperty(PropertyName._horizontalCurve, Variant.From(in _horizontalCurve));
		info.AddProperty(PropertyName._verticalCurve, Variant.From(in _verticalCurve));
		info.AddProperty(PropertyName._textureCurve, Variant.From(in _textureCurve));
		info.AddProperty(PropertyName._maxHeight, Variant.From(in _maxHeight));
		info.AddProperty(PropertyName._sourceOffset, Variant.From(in _sourceOffset));
		info.AddProperty(PropertyName._destinationOffset, Variant.From(in _destinationOffset));
		info.AddProperty(PropertyName._previousIndex, Variant.From(in _previousIndex));
		info.AddProperty(PropertyName._sourcePosition, Variant.From(in _sourcePosition));
		info.AddProperty(PropertyName._destinationPosition, Variant.From(in _destinationPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._minionSprite, out var value))
		{
			_minionSprite = value.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._minionTextures, out var value2))
		{
			_minionTextures = value2.AsGodotArray<Texture2D>();
		}
		if (info.TryGetProperty(PropertyName._minionAnimator, out var value3))
		{
			_minionAnimator = value3.As<AnimationPlayer>();
		}
		if (info.TryGetProperty(PropertyName._minionAnimations, out var value4))
		{
			_minionAnimations = value4.AsGodotArray<string>();
		}
		if (info.TryGetProperty(PropertyName._minionVfx, out var value5))
		{
			_minionVfx = value5.AsGodotArray<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._fallingTrail, out var value6))
		{
			_fallingTrail = value6.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._fallingVfx, out var value7))
		{
			_fallingVfx = value7.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._impactVfx, out var value8))
		{
			_impactVfx = value8.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._flightTime, out var value9))
		{
			_flightTime = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._fallingVfxEntryTime, out var value10))
		{
			_fallingVfxEntryTime = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._horizontalCurve, out var value11))
		{
			_horizontalCurve = value11.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._verticalCurve, out var value12))
		{
			_verticalCurve = value12.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._textureCurve, out var value13))
		{
			_textureCurve = value13.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._maxHeight, out var value14))
		{
			_maxHeight = value14.As<float>();
		}
		if (info.TryGetProperty(PropertyName._sourceOffset, out var value15))
		{
			_sourceOffset = value15.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._destinationOffset, out var value16))
		{
			_destinationOffset = value16.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._previousIndex, out var value17))
		{
			_previousIndex = value17.As<int>();
		}
		if (info.TryGetProperty(PropertyName._sourcePosition, out var value18))
		{
			_sourcePosition = value18.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._destinationPosition, out var value19))
		{
			_destinationPosition = value19.As<Vector2>();
		}
	}
}
