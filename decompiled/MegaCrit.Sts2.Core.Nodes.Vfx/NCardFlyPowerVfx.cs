using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardFlyPowerVfx.cs")]
public class NCardFlyPowerVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'GetDuration' method.
		/// </summary>
		public static readonly StringName GetDuration = "GetDuration";

		/// <summary>
		/// Cached name for the 'GetDurationInternal' method.
		/// </summary>
		public static readonly StringName GetDurationInternal = "GetDurationInternal";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the 'CardNode' property.
		/// </summary>
		public static readonly StringName CardNode = "CardNode";

		/// <summary>
		/// Cached name for the '_cardOwnerNode' field.
		/// </summary>
		public static readonly StringName _cardOwnerNode = "_cardOwnerNode";

		/// <summary>
		/// Cached name for the '_vfx' field.
		/// </summary>
		public static readonly StringName _vfx = "_vfx";

		/// <summary>
		/// Cached name for the '_swooshPath' field.
		/// </summary>
		public static readonly StringName _swooshPath = "_swooshPath";

		/// <summary>
		/// Cached name for the '_scaleTween' field.
		/// </summary>
		public static readonly StringName _scaleTween = "_scaleTween";

		/// <summary>
		/// Cached name for the '_scalingOut' field.
		/// </summary>
		public static readonly StringName _scalingOut = "_scalingOut";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private const float _speed = 3000f;

	private const float _scaleOutProportion = 0.9f;

	private const float _initialRotationSpeed = (float)Math.PI;

	private const float _maxRotationSpeed = (float)Math.PI * 50f;

	private NCreature _cardOwnerNode;

	private NCardTrailVfx? _vfx;

	private Path2D _swooshPath;

	private Tween? _scaleTween;

	private bool _scalingOut;

	private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_card_power_fly");

	public NCard CardNode { get; private set; }

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NCardFlyPowerVfx? Create(NCard card)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardFlyPowerVfx nCardFlyPowerVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NCardFlyPowerVfx>(PackedScene.GenEditState.Disabled);
		nCardFlyPowerVfx.CardNode = card;
		return nCardFlyPowerVfx;
	}

	public override void _Ready()
	{
		base.GlobalPosition = CardNode.GlobalPosition;
		Player owner = CardNode.Model.Owner;
		_cardOwnerNode = NCombatRoom.Instance.GetCreatureNode(owner.Creature);
		_vfx = NCardTrailVfx.Create(CardNode, owner.Character.TrailPath);
		if (_vfx != null)
		{
			this.AddChildSafely(_vfx);
		}
		Vector2 vfxSpawnPosition = _cardOwnerNode.VfxSpawnPosition;
		Vector2 position = vfxSpawnPosition - base.GlobalPosition;
		_swooshPath = GetNode<Path2D>("SwooshPath");
		_swooshPath.Curve.SetPointPosition(_swooshPath.Curve.PointCount - 1, position);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_scaleTween?.Kill();
		_cancelToken.Cancel();
	}

	public float GetDuration()
	{
		return GetDurationInternal() + 0.05f;
	}

	private float GetDurationInternal()
	{
		return _swooshPath.Curve.GetBakedLength() / 3000f;
	}

	public async Task PlayAnim()
	{
		SfxCmd.Play("event:/sfx/ui/cards/card_movement_B_power");
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(CardNode, "scale", Vector2.One * 0.1f, 0.30000001192092896);
		float length = _swooshPath.Curve.GetBakedLength();
		double timeAccumulator = 0.0;
		float duration = GetDurationInternal();
		while (timeAccumulator < (double)duration)
		{
			await this.AwaitProcessFrame();
			if (_cancelToken.IsCancellationRequested)
			{
				break;
			}
			double processDeltaTime = GetProcessDeltaTime();
			timeAccumulator += processDeltaTime;
			float num = (float)(timeAccumulator / (double)duration);
			float num2 = Ease.QuadIn(num);
			Transform2D transform2D = _swooshPath.Curve.SampleBakedWithRotation(num2 * length);
			CardNode.GlobalPosition = base.GlobalPosition + transform2D.Origin;
			float s = transform2D.Rotation - CardNode.Rotation;
			float num3 = Mathf.Lerp((float)Math.PI, (float)Math.PI * 50f, num);
			CardNode.Rotation += (float)Mathf.Sign(s) * Mathf.Min(Mathf.Abs(s), (float)((double)num3 * processDeltaTime));
			if (num >= 0.9f && !_scalingOut)
			{
				_scalingOut = true;
				_scaleTween?.Kill();
				_scaleTween = CreateTween();
				_scaleTween.TweenProperty(CardNode, "scale", Vector2.Zero, (float)((double)duration - timeAccumulator));
			}
		}
		NGame.Instance.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
		if (_vfx != null)
		{
			await _vfx.FadeOut();
		}
		CardNode.QueueFreeSafely();
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
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetDuration, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetDurationInternal, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NCardFlyPowerVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0])));
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetDuration && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetDuration());
			return true;
		}
		if (method == MethodName.GetDurationInternal && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetDurationInternal());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NCardFlyPowerVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.GetDuration)
		{
			return true;
		}
		if (method == MethodName.GetDurationInternal)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CardNode)
		{
			CardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._cardOwnerNode)
		{
			_cardOwnerNode = VariantUtils.ConvertTo<NCreature>(in value);
			return true;
		}
		if (name == PropertyName._vfx)
		{
			_vfx = VariantUtils.ConvertTo<NCardTrailVfx>(in value);
			return true;
		}
		if (name == PropertyName._swooshPath)
		{
			_swooshPath = VariantUtils.ConvertTo<Path2D>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._scalingOut)
		{
			_scalingOut = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CardNode)
		{
			value = VariantUtils.CreateFrom<NCard>(CardNode);
			return true;
		}
		if (name == PropertyName._cardOwnerNode)
		{
			value = VariantUtils.CreateFrom(in _cardOwnerNode);
			return true;
		}
		if (name == PropertyName._vfx)
		{
			value = VariantUtils.CreateFrom(in _vfx);
			return true;
		}
		if (name == PropertyName._swooshPath)
		{
			value = VariantUtils.CreateFrom(in _swooshPath);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		if (name == PropertyName._scalingOut)
		{
			value = VariantUtils.CreateFrom(in _scalingOut);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardOwnerNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._vfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._swooshPath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._scalingOut, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CardNode, Variant.From<NCard>(CardNode));
		info.AddProperty(PropertyName._cardOwnerNode, Variant.From(in _cardOwnerNode));
		info.AddProperty(PropertyName._vfx, Variant.From(in _vfx));
		info.AddProperty(PropertyName._swooshPath, Variant.From(in _swooshPath));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._scalingOut, Variant.From(in _scalingOut));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CardNode, out var value))
		{
			CardNode = value.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._cardOwnerNode, out var value2))
		{
			_cardOwnerNode = value2.As<NCreature>();
		}
		if (info.TryGetProperty(PropertyName._vfx, out var value3))
		{
			_vfx = value3.As<NCardTrailVfx>();
		}
		if (info.TryGetProperty(PropertyName._swooshPath, out var value4))
		{
			_swooshPath = value4.As<Path2D>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value5))
		{
			_scaleTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._scalingOut, out var value6))
		{
			_scalingOut = value6.As<bool>();
		}
	}
}
