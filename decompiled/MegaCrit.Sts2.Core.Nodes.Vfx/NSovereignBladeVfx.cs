using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NSovereignBladeVfx.cs")]
public class NSovereignBladeVfx : Node2D
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
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'Forge' method.
		/// </summary>
		public static readonly StringName Forge = "Forge";

		/// <summary>
		/// Cached name for the 'Attack' method.
		/// </summary>
		public static readonly StringName Attack = "Attack";

		/// <summary>
		/// Cached name for the 'OnTargetingBegan' method.
		/// </summary>
		public static readonly StringName OnTargetingBegan = "OnTargetingBegan";

		/// <summary>
		/// Cached name for the 'OnTargetingEnded' method.
		/// </summary>
		public static readonly StringName OnTargetingEnded = "OnTargetingEnded";

		/// <summary>
		/// Cached name for the 'OnFocused' method.
		/// </summary>
		public static readonly StringName OnFocused = "OnFocused";

		/// <summary>
		/// Cached name for the 'OnUnfocused' method.
		/// </summary>
		public static readonly StringName OnUnfocused = "OnUnfocused";

		/// <summary>
		/// Cached name for the 'UpdateHoverTip' method.
		/// </summary>
		public static readonly StringName UpdateHoverTip = "UpdateHoverTip";

		/// <summary>
		/// Cached name for the 'FireSparks' method.
		/// </summary>
		public static readonly StringName FireSparks = "FireSparks";

		/// <summary>
		/// Cached name for the 'FireFlames' method.
		/// </summary>
		public static readonly StringName FireFlames = "FireFlames";

		/// <summary>
		/// Cached name for the 'EndSlash' method.
		/// </summary>
		public static readonly StringName EndSlash = "EndSlash";

		/// <summary>
		/// Cached name for the 'CleanupForge' method.
		/// </summary>
		public static readonly StringName CleanupForge = "CleanupForge";

		/// <summary>
		/// Cached name for the 'CleanupAttack' method.
		/// </summary>
		public static readonly StringName CleanupAttack = "CleanupAttack";

		/// <summary>
		/// Cached name for the 'RemoveSovereignBlade' method.
		/// </summary>
		public static readonly StringName RemoveSovereignBlade = "RemoveSovereignBlade";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the 'OrbitProgress' property.
		/// </summary>
		public static readonly StringName OrbitProgress = "OrbitProgress";

		/// <summary>
		/// Cached name for the '_spineNode' field.
		/// </summary>
		public static readonly StringName _spineNode = "_spineNode";

		/// <summary>
		/// Cached name for the '_bladeGlow' field.
		/// </summary>
		public static readonly StringName _bladeGlow = "_bladeGlow";

		/// <summary>
		/// Cached name for the '_forgeSparks' field.
		/// </summary>
		public static readonly StringName _forgeSparks = "_forgeSparks";

		/// <summary>
		/// Cached name for the '_spawnFlames' field.
		/// </summary>
		public static readonly StringName _spawnFlames = "_spawnFlames";

		/// <summary>
		/// Cached name for the '_spawnFlamesBack' field.
		/// </summary>
		public static readonly StringName _spawnFlamesBack = "_spawnFlamesBack";

		/// <summary>
		/// Cached name for the '_slashParticles' field.
		/// </summary>
		public static readonly StringName _slashParticles = "_slashParticles";

		/// <summary>
		/// Cached name for the '_chargeParticles' field.
		/// </summary>
		public static readonly StringName _chargeParticles = "_chargeParticles";

		/// <summary>
		/// Cached name for the '_spikeParticles' field.
		/// </summary>
		public static readonly StringName _spikeParticles = "_spikeParticles";

		/// <summary>
		/// Cached name for the '_spikeParticles2' field.
		/// </summary>
		public static readonly StringName _spikeParticles2 = "_spikeParticles2";

		/// <summary>
		/// Cached name for the '_spikeCircle' field.
		/// </summary>
		public static readonly StringName _spikeCircle = "_spikeCircle";

		/// <summary>
		/// Cached name for the '_spikeCircle2' field.
		/// </summary>
		public static readonly StringName _spikeCircle2 = "_spikeCircle2";

		/// <summary>
		/// Cached name for the '_hilt' field.
		/// </summary>
		public static readonly StringName _hilt = "_hilt";

		/// <summary>
		/// Cached name for the '_hilt2' field.
		/// </summary>
		public static readonly StringName _hilt2 = "_hilt2";

		/// <summary>
		/// Cached name for the '_detail' field.
		/// </summary>
		public static readonly StringName _detail = "_detail";

		/// <summary>
		/// Cached name for the '_trail' field.
		/// </summary>
		public static readonly StringName _trail = "_trail";

		/// <summary>
		/// Cached name for the '_orbitPath' field.
		/// </summary>
		public static readonly StringName _orbitPath = "_orbitPath";

		/// <summary>
		/// Cached name for the '_hitbox' field.
		/// </summary>
		public static readonly StringName _hitbox = "_hitbox";

		/// <summary>
		/// Cached name for the '_selectionReticle' field.
		/// </summary>
		public static readonly StringName _selectionReticle = "_selectionReticle";

		/// <summary>
		/// Cached name for the '_attackTween' field.
		/// </summary>
		public static readonly StringName _attackTween = "_attackTween";

		/// <summary>
		/// Cached name for the '_scaleTween' field.
		/// </summary>
		public static readonly StringName _scaleTween = "_scaleTween";

		/// <summary>
		/// Cached name for the '_sparkDelay' field.
		/// </summary>
		public static readonly StringName _sparkDelay = "_sparkDelay";

		/// <summary>
		/// Cached name for the '_glowTween' field.
		/// </summary>
		public static readonly StringName _glowTween = "_glowTween";

		/// <summary>
		/// Cached name for the '_trailFadeTween' field.
		/// </summary>
		public static readonly StringName _trailFadeTween = "_trailFadeTween";

		/// <summary>
		/// Cached name for the '_trailStart' field.
		/// </summary>
		public static readonly StringName _trailStart = "_trailStart";

		/// <summary>
		/// Cached name for the '_bladeSize' field.
		/// </summary>
		public static readonly StringName _bladeSize = "_bladeSize";

		/// <summary>
		/// Cached name for the '_targetOrbitPosition' field.
		/// </summary>
		public static readonly StringName _targetOrbitPosition = "_targetOrbitPosition";

		/// <summary>
		/// Cached name for the '_isBehindCharacter' field.
		/// </summary>
		public static readonly StringName _isBehindCharacter = "_isBehindCharacter";

		/// <summary>
		/// Cached name for the '_isFocused' field.
		/// </summary>
		public static readonly StringName _isFocused = "_isFocused";

		/// <summary>
		/// Cached name for the '_hoverTip' field.
		/// </summary>
		public static readonly StringName _hoverTip = "_hoverTip";

		/// <summary>
		/// Cached name for the '_isForging' field.
		/// </summary>
		public static readonly StringName _isForging = "_isForging";

		/// <summary>
		/// Cached name for the '_isAttacking' field.
		/// </summary>
		public static readonly StringName _isAttacking = "_isAttacking";

		/// <summary>
		/// Cached name for the '_isKeyPressed' field.
		/// </summary>
		public static readonly StringName _isKeyPressed = "_isKeyPressed";

		/// <summary>
		/// Cached name for the '_testCharge' field.
		/// </summary>
		public static readonly StringName _testCharge = "_testCharge";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/sovereign_blade");

	private Player _owner;

	private Node2D _spineNode;

	private MegaSprite _animController;

	private Node2D _bladeGlow;

	private GpuParticles2D _forgeSparks;

	private GpuParticles2D _spawnFlames;

	private GpuParticles2D _spawnFlamesBack;

	private GpuParticles2D _slashParticles;

	private GpuParticles2D _chargeParticles;

	private GpuParticles2D _spikeParticles;

	private GpuParticles2D _spikeParticles2;

	private GpuParticles2D _spikeCircle;

	private GpuParticles2D _spikeCircle2;

	private TextureRect _hilt;

	private TextureRect _hilt2;

	private TextureRect _detail;

	private Line2D _trail;

	private Path2D _orbitPath;

	private Control _hitbox;

	private NSelectionReticle _selectionReticle;

	private Tween? _attackTween;

	private Tween? _scaleTween;

	private Tween? _sparkDelay;

	private Tween? _glowTween;

	private Tween? _trailFadeTween;

	private Vector2 _trailStart;

	private float _bladeSize;

	private const float _orbitSpeed = 60f;

	private Vector2 _targetOrbitPosition;

	private bool _isBehindCharacter;

	private const float _hiltThreshold = 0.3f;

	private const float _detailThreshold = 0.66f;

	private bool _isFocused;

	private NHoverTipSet? _hoverTip;

	private bool _isForging;

	private bool _isAttacking;

	private bool _isKeyPressed;

	private float _testCharge;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public CardModel Card { get; private set; }

	public double OrbitProgress { get; set; }

	public override void _Ready()
	{
		_spineNode = GetNode<Node2D>("SpineSword");
		_animController = new MegaSprite(_spineNode);
		_bladeGlow = GetNode<Node2D>("SpineSword/SwordBone/ScaleContainer/BladeGlow");
		_forgeSparks = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/ForgeSparks");
		_spawnFlames = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/SpawnFlames");
		_spawnFlamesBack = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/SpawnFlamesBack");
		_slashParticles = GetNode<GpuParticles2D>("SpineSword/SlashParticles");
		_chargeParticles = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/ChargeParticles");
		_spikeParticles = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/Spikes");
		_spikeParticles2 = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/Spikes2");
		_spikeCircle = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/SpikeCircle");
		_spikeCircle2 = GetNode<GpuParticles2D>("SpineSword/SwordBone/ScaleContainer/SpikeCircle2");
		_hilt = GetNode<TextureRect>("SpineSword/SwordBone/ScaleContainer/Hilt");
		_hilt2 = GetNode<TextureRect>("SpineSword/SwordBone/ScaleContainer/Hilt2");
		_detail = GetNode<TextureRect>("SpineSword/SwordBone/ScaleContainer/Detail");
		_trail = GetNode<Line2D>("Trail");
		_orbitPath = GetNode<Path2D>("%Path");
		_hitbox = GetNode<Control>("%Hitbox");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_hitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnFocused));
		_hitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnfocused));
		_hitbox.Connect(Control.SignalName.FocusEntered, Callable.From(OnFocused));
		_hitbox.Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocused));
		_forgeSparks.Emitting = false;
		_forgeSparks.OneShot = true;
		_spawnFlames.Emitting = false;
		_spawnFlames.OneShot = true;
		_spawnFlamesBack.Emitting = false;
		_spawnFlamesBack.OneShot = true;
		_slashParticles.Emitting = false;
		_slashParticles.OneShot = true;
		_chargeParticles.Emitting = false;
		_spikeParticles2.Emitting = false;
		_spikeCircle2.Emitting = false;
		_bladeGlow.Modulate = Colors.Transparent;
		_bladeGlow.Visible = false;
		_trail.GlobalPosition = Vector2.Zero;
		_trail.ClearPoints();
		this.RunWhenSpineReady(_animController, delegate(MegaAnimationState animState)
		{
			animState.SetAnimation("idle_loop");
		});
		_spineNode.Scale = Vector2.Zero;
		_spineNode.Visible = true;
		NTargetManager.Instance.Connect(NTargetManager.SignalName.TargetingBegan, Callable.From(OnTargetingBegan));
		NTargetManager.Instance.Connect(NTargetManager.SignalName.TargetingEnded, Callable.From(OnTargetingEnded));
		_owner = Card.Owner;
		_owner.Creature.Died += OnOwnerDied;
	}

	public override void _ExitTree()
	{
		_attackTween?.Kill();
		_scaleTween?.Kill();
		_sparkDelay?.Kill();
		_glowTween?.Kill();
		_trailFadeTween?.Kill();
		_owner.Creature.Died -= OnOwnerDied;
	}

	public static NSovereignBladeVfx? Create(CardModel card)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NSovereignBladeVfx nSovereignBladeVfx = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NSovereignBladeVfx>(PackedScene.GenEditState.Disabled);
		nSovereignBladeVfx.Card = card;
		return nSovereignBladeVfx;
	}

	public override void _Process(double delta)
	{
		float bakedLength = _orbitPath.Curve.GetBakedLength();
		if (_hoverTip == null)
		{
			OrbitProgress += 60.0 * delta / (double)bakedLength;
		}
		double num = OrbitProgress % 1.0;
		bool flag = num > 0.25 && num < 0.7799999713897705;
		if (flag != _isBehindCharacter && _bladeSize < 0.6f)
		{
			_isBehindCharacter = !_isBehindCharacter;
			GetParent().MoveChildSafely(this, (!flag) ? (GetParent().GetChildCount() - 1) : 0);
		}
		Transform2D transform2D = _orbitPath.Curve.SampleBakedWithRotation((float)(OrbitProgress % 1.0) * bakedLength);
		Vector2 vector = _orbitPath.GlobalTransform * transform2D.Origin;
		vector.X = Mathf.Lerp(vector.X, base.GlobalPosition.X + 200f, Mathf.Clamp(_bladeSize / 1.25f, 0f, 1f));
		_targetOrbitPosition = vector + Vector2.Up * (_spineNode.Scale.Y - 1f) * 100f;
		if (!_isAttacking)
		{
			_spineNode.GlobalPosition = _spineNode.GlobalPosition.Lerp(_targetOrbitPosition, (float)delta * 7f);
		}
	}

	public void Forge(float bladeDamage = 0f, bool showFlames = false)
	{
		if (_isForging)
		{
			CleanupForge();
		}
		_bladeSize = Mathf.Clamp(Mathf.Lerp(0f, 1f, bladeDamage / 200f), 0f, 1f);
		_isForging = true;
		int num = (int)(_bladeSize * 30f);
		if (num > 0)
		{
			_chargeParticles.Amount = num;
			_chargeParticles.Emitting = true;
		}
		else
		{
			_chargeParticles.Emitting = false;
		}
		_hilt.Visible = _bladeSize < 0.3f;
		_hilt2.Visible = !_hilt.Visible;
		GpuParticles2D spikeParticles = _spikeParticles;
		bool emitting = (_spikeParticles.Visible = _hilt.Visible);
		spikeParticles.Emitting = emitting;
		GpuParticles2D spikeParticles2 = _spikeParticles2;
		emitting = (_spikeParticles2.Visible = !_hilt.Visible);
		spikeParticles2.Emitting = emitting;
		GpuParticles2D spikeCircle = _spikeCircle;
		emitting = (_spikeCircle.Visible = _hilt.Visible);
		spikeCircle.Emitting = emitting;
		GpuParticles2D spikeCircle2 = _spikeCircle2;
		emitting = (_spikeCircle2.Visible = !_hilt.Visible);
		spikeCircle2.Emitting = emitting;
		_detail.Visible = bladeDamage >= 0.66f;
		_bladeGlow.Visible = true;
		Color color = Color.FromHtml("#ff7300");
		Color color2 = color;
		color2.A = 0f;
		_glowTween = CreateTween();
		if (showFlames)
		{
			FireFlames();
		}
		_glowTween.TweenProperty(_bladeGlow, "modulate", color, 0.05).SetEase(Tween.EaseType.Out);
		_glowTween.Chain().TweenProperty(_bladeGlow, "modulate", color2, 0.5).SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Cubic);
		_glowTween.Chain().TweenCallback(Callable.From(CleanupForge));
		Vector2 vector = Vector2.One * Mathf.Lerp(0.9f, 2f, _bladeSize);
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(_spineNode, "scale", vector * 1.2f, 0.05000000074505806).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_scaleTween.Chain().TweenCallback(Callable.From(FireSparks));
		_scaleTween.Chain().TweenProperty(_spineNode, "scale", vector, 0.30000001192092896).SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Cubic);
	}

	public void Attack(Vector2 targetPos)
	{
		if (_isAttacking)
		{
			CleanupAttack();
		}
		_isAttacking = true;
		_animController.GetAnimationState().SetAnimation("attack", loop: false);
		_attackTween = CreateTween();
		Vector2 vector = new Vector2(_spineNode.GlobalPosition.X - 50f, _spineNode.GlobalPosition.Y);
		_trail.Visible = true;
		_trailStart = vector;
		_attackTween.TweenProperty(_spineNode, "rotation", _spineNode.GetAngleTo(targetPos), 0.05000000074505806);
		_attackTween.Parallel().TweenProperty(_spineNode, "global_position", vector, 0.07999999821186066).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Expo);
		_attackTween.Chain().TweenProperty(_spineNode, "rotation", _spineNode.GetAngleTo(targetPos), 0.0);
		_attackTween.Parallel().TweenProperty(_spineNode, "global_position", targetPos, 0.05000000074505806).SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Expo);
		_attackTween.Chain().TweenCallback(Callable.From(EndSlash));
		_attackTween.TweenInterval(0.25);
		_attackTween.Chain().TweenCallback(Callable.From(FireSparks)).SetDelay(0.30000001192092896);
		_attackTween.Chain().TweenCallback(Callable.From(CleanupAttack));
		UpdateHoverTip();
	}

	private void OnTargetingBegan()
	{
		_hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		UpdateHoverTip();
	}

	private void OnTargetingEnded()
	{
		_hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
		UpdateHoverTip();
	}

	private void OnFocused()
	{
		_isFocused = true;
		if (!NCombatRoom.Instance.Ui.Hand.InCardPlay)
		{
			UpdateHoverTip();
		}
	}

	private void OnUnfocused()
	{
		_isFocused = false;
		UpdateHoverTip();
	}

	private void UpdateHoverTip()
	{
		bool flag = _isFocused && !_isAttacking && !NTargetManager.Instance.IsInSelection && _hitbox.MouseFilter != Control.MouseFilterEnum.Ignore;
		if (flag)
		{
			_selectionReticle.OnSelect();
			if (_hoverTip == null)
			{
				_hoverTip = NHoverTipSet.CreateAndShow(_hitbox, HoverTipFactory.FromCard(Card));
				_hoverTip?.SetGlobalPosition(_hitbox.GlobalPosition + Vector2.Right * _hitbox.Size.X);
			}
		}
		else if (!flag)
		{
			_selectionReticle.OnDeselect();
			if (_hoverTip != null)
			{
				NHoverTipSet.Remove(_hitbox);
				_hoverTip = null;
			}
		}
	}

	private void FireSparks()
	{
		_forgeSparks.Restart();
	}

	private void FireFlames()
	{
		_spawnFlames.Restart();
		_spawnFlamesBack.Restart();
	}

	private void EndSlash()
	{
		_chargeParticles.Emitting = false;
		_chargeParticles.Restart();
		_slashParticles.Rotation = _spineNode.GetAngleTo(_trailStart) - 1.5708f;
		_slashParticles.Restart();
		_trail.AddPoint(_trailStart);
		_trail.AddPoint(GetNode<Node2D>("SpineSword/SwordBone/ScaleContainer/SpikeCircle").GlobalPosition);
		_trail.Modulate = Colors.White;
		_trailFadeTween = CreateTween();
		_trailFadeTween.TweenProperty(_trail, "modulate:a", 0f, 0.20000000298023224);
	}

	private void CleanupForge()
	{
		_isForging = false;
		_scaleTween?.Kill();
		_glowTween?.Kill();
	}

	private void CleanupAttack()
	{
		_isAttacking = false;
		_attackTween?.Kill();
		_animController.GetAnimationState().SetAnimation("idle_loop");
		_spineNode.Rotation = 0f;
		_trail.ClearPoints();
	}

	public void RemoveSovereignBlade()
	{
		_scaleTween?.Kill();
		_scaleTween = CreateTween();
		_scaleTween.TweenProperty(_spineNode, "scale", Vector2.Zero, 0.20000000298023224).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_scaleTween.Chain().TweenCallback(Callable.From(this.QueueFreeSafely));
	}

	private void OnOwnerDied(Creature creature)
	{
		_hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		UpdateHoverTip();
		RemoveSovereignBlade();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Forge, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "bladeDamage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "showFlames", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Attack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "targetPos", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnTargetingBegan, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnTargetingEnded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHoverTip, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FireSparks, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FireFlames, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EndSlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CleanupForge, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CleanupAttack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveSovereignBlade, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Forge && args.Count == 2)
		{
			Forge(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Attack && args.Count == 1)
		{
			Attack(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTargetingBegan && args.Count == 0)
		{
			OnTargetingBegan();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnTargetingEnded && args.Count == 0)
		{
			OnTargetingEnded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocused && args.Count == 0)
		{
			OnFocused();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocused && args.Count == 0)
		{
			OnUnfocused();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHoverTip && args.Count == 0)
		{
			UpdateHoverTip();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FireSparks && args.Count == 0)
		{
			FireSparks();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FireFlames && args.Count == 0)
		{
			FireFlames();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EndSlash && args.Count == 0)
		{
			EndSlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CleanupForge && args.Count == 0)
		{
			CleanupForge();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CleanupAttack && args.Count == 0)
		{
			CleanupAttack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveSovereignBlade && args.Count == 0)
		{
			RemoveSovereignBlade();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.Forge)
		{
			return true;
		}
		if (method == MethodName.Attack)
		{
			return true;
		}
		if (method == MethodName.OnTargetingBegan)
		{
			return true;
		}
		if (method == MethodName.OnTargetingEnded)
		{
			return true;
		}
		if (method == MethodName.OnFocused)
		{
			return true;
		}
		if (method == MethodName.OnUnfocused)
		{
			return true;
		}
		if (method == MethodName.UpdateHoverTip)
		{
			return true;
		}
		if (method == MethodName.FireSparks)
		{
			return true;
		}
		if (method == MethodName.FireFlames)
		{
			return true;
		}
		if (method == MethodName.EndSlash)
		{
			return true;
		}
		if (method == MethodName.CleanupForge)
		{
			return true;
		}
		if (method == MethodName.CleanupAttack)
		{
			return true;
		}
		if (method == MethodName.RemoveSovereignBlade)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.OrbitProgress)
		{
			OrbitProgress = VariantUtils.ConvertTo<double>(in value);
			return true;
		}
		if (name == PropertyName._spineNode)
		{
			_spineNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._bladeGlow)
		{
			_bladeGlow = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._forgeSparks)
		{
			_forgeSparks = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spawnFlames)
		{
			_spawnFlames = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spawnFlamesBack)
		{
			_spawnFlamesBack = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._slashParticles)
		{
			_slashParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._chargeParticles)
		{
			_chargeParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spikeParticles)
		{
			_spikeParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spikeParticles2)
		{
			_spikeParticles2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spikeCircle)
		{
			_spikeCircle = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._spikeCircle2)
		{
			_spikeCircle2 = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._hilt)
		{
			_hilt = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hilt2)
		{
			_hilt2 = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._detail)
		{
			_detail = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._trail)
		{
			_trail = VariantUtils.ConvertTo<Line2D>(in value);
			return true;
		}
		if (name == PropertyName._orbitPath)
		{
			_orbitPath = VariantUtils.ConvertTo<Path2D>(in value);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			_hitbox = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._attackTween)
		{
			_attackTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._sparkDelay)
		{
			_sparkDelay = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			_glowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._trailFadeTween)
		{
			_trailFadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._trailStart)
		{
			_trailStart = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._bladeSize)
		{
			_bladeSize = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetOrbitPosition)
		{
			_targetOrbitPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._isBehindCharacter)
		{
			_isBehindCharacter = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			_isFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hoverTip)
		{
			_hoverTip = VariantUtils.ConvertTo<NHoverTipSet>(in value);
			return true;
		}
		if (name == PropertyName._isForging)
		{
			_isForging = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isAttacking)
		{
			_isAttacking = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isKeyPressed)
		{
			_isKeyPressed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._testCharge)
		{
			_testCharge = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.OrbitProgress)
		{
			value = VariantUtils.CreateFrom<double>(OrbitProgress);
			return true;
		}
		if (name == PropertyName._spineNode)
		{
			value = VariantUtils.CreateFrom(in _spineNode);
			return true;
		}
		if (name == PropertyName._bladeGlow)
		{
			value = VariantUtils.CreateFrom(in _bladeGlow);
			return true;
		}
		if (name == PropertyName._forgeSparks)
		{
			value = VariantUtils.CreateFrom(in _forgeSparks);
			return true;
		}
		if (name == PropertyName._spawnFlames)
		{
			value = VariantUtils.CreateFrom(in _spawnFlames);
			return true;
		}
		if (name == PropertyName._spawnFlamesBack)
		{
			value = VariantUtils.CreateFrom(in _spawnFlamesBack);
			return true;
		}
		if (name == PropertyName._slashParticles)
		{
			value = VariantUtils.CreateFrom(in _slashParticles);
			return true;
		}
		if (name == PropertyName._chargeParticles)
		{
			value = VariantUtils.CreateFrom(in _chargeParticles);
			return true;
		}
		if (name == PropertyName._spikeParticles)
		{
			value = VariantUtils.CreateFrom(in _spikeParticles);
			return true;
		}
		if (name == PropertyName._spikeParticles2)
		{
			value = VariantUtils.CreateFrom(in _spikeParticles2);
			return true;
		}
		if (name == PropertyName._spikeCircle)
		{
			value = VariantUtils.CreateFrom(in _spikeCircle);
			return true;
		}
		if (name == PropertyName._spikeCircle2)
		{
			value = VariantUtils.CreateFrom(in _spikeCircle2);
			return true;
		}
		if (name == PropertyName._hilt)
		{
			value = VariantUtils.CreateFrom(in _hilt);
			return true;
		}
		if (name == PropertyName._hilt2)
		{
			value = VariantUtils.CreateFrom(in _hilt2);
			return true;
		}
		if (name == PropertyName._detail)
		{
			value = VariantUtils.CreateFrom(in _detail);
			return true;
		}
		if (name == PropertyName._trail)
		{
			value = VariantUtils.CreateFrom(in _trail);
			return true;
		}
		if (name == PropertyName._orbitPath)
		{
			value = VariantUtils.CreateFrom(in _orbitPath);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			value = VariantUtils.CreateFrom(in _hitbox);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._attackTween)
		{
			value = VariantUtils.CreateFrom(in _attackTween);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		if (name == PropertyName._sparkDelay)
		{
			value = VariantUtils.CreateFrom(in _sparkDelay);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			value = VariantUtils.CreateFrom(in _glowTween);
			return true;
		}
		if (name == PropertyName._trailFadeTween)
		{
			value = VariantUtils.CreateFrom(in _trailFadeTween);
			return true;
		}
		if (name == PropertyName._trailStart)
		{
			value = VariantUtils.CreateFrom(in _trailStart);
			return true;
		}
		if (name == PropertyName._bladeSize)
		{
			value = VariantUtils.CreateFrom(in _bladeSize);
			return true;
		}
		if (name == PropertyName._targetOrbitPosition)
		{
			value = VariantUtils.CreateFrom(in _targetOrbitPosition);
			return true;
		}
		if (name == PropertyName._isBehindCharacter)
		{
			value = VariantUtils.CreateFrom(in _isBehindCharacter);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			value = VariantUtils.CreateFrom(in _isFocused);
			return true;
		}
		if (name == PropertyName._hoverTip)
		{
			value = VariantUtils.CreateFrom(in _hoverTip);
			return true;
		}
		if (name == PropertyName._isForging)
		{
			value = VariantUtils.CreateFrom(in _isForging);
			return true;
		}
		if (name == PropertyName._isAttacking)
		{
			value = VariantUtils.CreateFrom(in _isAttacking);
			return true;
		}
		if (name == PropertyName._isKeyPressed)
		{
			value = VariantUtils.CreateFrom(in _isKeyPressed);
			return true;
		}
		if (name == PropertyName._testCharge)
		{
			value = VariantUtils.CreateFrom(in _testCharge);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spineNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bladeGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._forgeSparks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spawnFlames, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spawnFlamesBack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slashParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chargeParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spikeParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spikeParticles2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spikeCircle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spikeCircle2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hilt, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hilt2, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._detail, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trail, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._orbitPath, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._attackTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sparkDelay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._trailFadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._trailStart, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._bladeSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.OrbitProgress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetOrbitPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isBehindCharacter, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTip, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isForging, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isAttacking, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isKeyPressed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._testCharge, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.OrbitProgress, Variant.From<double>(OrbitProgress));
		info.AddProperty(PropertyName._spineNode, Variant.From(in _spineNode));
		info.AddProperty(PropertyName._bladeGlow, Variant.From(in _bladeGlow));
		info.AddProperty(PropertyName._forgeSparks, Variant.From(in _forgeSparks));
		info.AddProperty(PropertyName._spawnFlames, Variant.From(in _spawnFlames));
		info.AddProperty(PropertyName._spawnFlamesBack, Variant.From(in _spawnFlamesBack));
		info.AddProperty(PropertyName._slashParticles, Variant.From(in _slashParticles));
		info.AddProperty(PropertyName._chargeParticles, Variant.From(in _chargeParticles));
		info.AddProperty(PropertyName._spikeParticles, Variant.From(in _spikeParticles));
		info.AddProperty(PropertyName._spikeParticles2, Variant.From(in _spikeParticles2));
		info.AddProperty(PropertyName._spikeCircle, Variant.From(in _spikeCircle));
		info.AddProperty(PropertyName._spikeCircle2, Variant.From(in _spikeCircle2));
		info.AddProperty(PropertyName._hilt, Variant.From(in _hilt));
		info.AddProperty(PropertyName._hilt2, Variant.From(in _hilt2));
		info.AddProperty(PropertyName._detail, Variant.From(in _detail));
		info.AddProperty(PropertyName._trail, Variant.From(in _trail));
		info.AddProperty(PropertyName._orbitPath, Variant.From(in _orbitPath));
		info.AddProperty(PropertyName._hitbox, Variant.From(in _hitbox));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._attackTween, Variant.From(in _attackTween));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._sparkDelay, Variant.From(in _sparkDelay));
		info.AddProperty(PropertyName._glowTween, Variant.From(in _glowTween));
		info.AddProperty(PropertyName._trailFadeTween, Variant.From(in _trailFadeTween));
		info.AddProperty(PropertyName._trailStart, Variant.From(in _trailStart));
		info.AddProperty(PropertyName._bladeSize, Variant.From(in _bladeSize));
		info.AddProperty(PropertyName._targetOrbitPosition, Variant.From(in _targetOrbitPosition));
		info.AddProperty(PropertyName._isBehindCharacter, Variant.From(in _isBehindCharacter));
		info.AddProperty(PropertyName._isFocused, Variant.From(in _isFocused));
		info.AddProperty(PropertyName._hoverTip, Variant.From(in _hoverTip));
		info.AddProperty(PropertyName._isForging, Variant.From(in _isForging));
		info.AddProperty(PropertyName._isAttacking, Variant.From(in _isAttacking));
		info.AddProperty(PropertyName._isKeyPressed, Variant.From(in _isKeyPressed));
		info.AddProperty(PropertyName._testCharge, Variant.From(in _testCharge));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.OrbitProgress, out var value))
		{
			OrbitProgress = value.As<double>();
		}
		if (info.TryGetProperty(PropertyName._spineNode, out var value2))
		{
			_spineNode = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._bladeGlow, out var value3))
		{
			_bladeGlow = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._forgeSparks, out var value4))
		{
			_forgeSparks = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spawnFlames, out var value5))
		{
			_spawnFlames = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spawnFlamesBack, out var value6))
		{
			_spawnFlamesBack = value6.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._slashParticles, out var value7))
		{
			_slashParticles = value7.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._chargeParticles, out var value8))
		{
			_chargeParticles = value8.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spikeParticles, out var value9))
		{
			_spikeParticles = value9.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spikeParticles2, out var value10))
		{
			_spikeParticles2 = value10.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spikeCircle, out var value11))
		{
			_spikeCircle = value11.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._spikeCircle2, out var value12))
		{
			_spikeCircle2 = value12.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._hilt, out var value13))
		{
			_hilt = value13.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hilt2, out var value14))
		{
			_hilt2 = value14.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._detail, out var value15))
		{
			_detail = value15.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._trail, out var value16))
		{
			_trail = value16.As<Line2D>();
		}
		if (info.TryGetProperty(PropertyName._orbitPath, out var value17))
		{
			_orbitPath = value17.As<Path2D>();
		}
		if (info.TryGetProperty(PropertyName._hitbox, out var value18))
		{
			_hitbox = value18.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value19))
		{
			_selectionReticle = value19.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._attackTween, out var value20))
		{
			_attackTween = value20.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value21))
		{
			_scaleTween = value21.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._sparkDelay, out var value22))
		{
			_sparkDelay = value22.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._glowTween, out var value23))
		{
			_glowTween = value23.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._trailFadeTween, out var value24))
		{
			_trailFadeTween = value24.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._trailStart, out var value25))
		{
			_trailStart = value25.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._bladeSize, out var value26))
		{
			_bladeSize = value26.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetOrbitPosition, out var value27))
		{
			_targetOrbitPosition = value27.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._isBehindCharacter, out var value28))
		{
			_isBehindCharacter = value28.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isFocused, out var value29))
		{
			_isFocused = value29.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hoverTip, out var value30))
		{
			_hoverTip = value30.As<NHoverTipSet>();
		}
		if (info.TryGetProperty(PropertyName._isForging, out var value31))
		{
			_isForging = value31.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isAttacking, out var value32))
		{
			_isAttacking = value32.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isKeyPressed, out var value33))
		{
			_isKeyPressed = value33.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._testCharge, out var value34))
		{
			_testCharge = value34.As<float>();
		}
	}
}
