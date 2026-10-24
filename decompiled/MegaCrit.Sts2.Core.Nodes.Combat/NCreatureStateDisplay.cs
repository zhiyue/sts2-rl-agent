using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCreatureStateDisplay.cs")]
public class NCreatureStateDisplay : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SubscribeToCreatureEvents' method.
		/// </summary>
		public static readonly StringName SubscribeToCreatureEvents = "SubscribeToCreatureEvents";

		/// <summary>
		/// Cached name for the 'DebugToggleVisibility' method.
		/// </summary>
		public static readonly StringName DebugToggleVisibility = "DebugToggleVisibility";

		/// <summary>
		/// Cached name for the 'SetCreatureBounds' method.
		/// </summary>
		public static readonly StringName SetCreatureBounds = "SetCreatureBounds";

		/// <summary>
		/// Cached name for the 'RefreshValues' method.
		/// </summary>
		public static readonly StringName RefreshValues = "RefreshValues";

		/// <summary>
		/// Cached name for the 'OnHovered' method.
		/// </summary>
		public static readonly StringName OnHovered = "OnHovered";

		/// <summary>
		/// Cached name for the 'OnUnhovered' method.
		/// </summary>
		public static readonly StringName OnUnhovered = "OnUnhovered";

		/// <summary>
		/// Cached name for the 'ShowNameplate' method.
		/// </summary>
		public static readonly StringName ShowNameplate = "ShowNameplate";

		/// <summary>
		/// Cached name for the 'HideNameplate' method.
		/// </summary>
		public static readonly StringName HideNameplate = "HideNameplate";

		/// <summary>
		/// Cached name for the 'HideImmediately' method.
		/// </summary>
		public static readonly StringName HideImmediately = "HideImmediately";

		/// <summary>
		/// Cached name for the 'AnimateIn' method.
		/// </summary>
		public static readonly StringName AnimateIn = "AnimateIn";

		/// <summary>
		/// Cached name for the 'AnimateInBlock' method.
		/// </summary>
		public static readonly StringName AnimateInBlock = "AnimateInBlock";

		/// <summary>
		/// Cached name for the 'AnimateOut' method.
		/// </summary>
		public static readonly StringName AnimateOut = "AnimateOut";

		/// <summary>
		/// Cached name for the 'OnBlockTrackingCreatureBlockChanged' method.
		/// </summary>
		public static readonly StringName OnBlockTrackingCreatureBlockChanged = "OnBlockTrackingCreatureBlockChanged";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_powerContainer' field.
		/// </summary>
		public static readonly StringName _powerContainer = "_powerContainer";

		/// <summary>
		/// Cached name for the '_nameplateContainer' field.
		/// </summary>
		public static readonly StringName _nameplateContainer = "_nameplateContainer";

		/// <summary>
		/// Cached name for the '_nameplateLabel' field.
		/// </summary>
		public static readonly StringName _nameplateLabel = "_nameplateLabel";

		/// <summary>
		/// Cached name for the '_healthBar' field.
		/// </summary>
		public static readonly StringName _healthBar = "_healthBar";

		/// <summary>
		/// Cached name for the '_hpBarHitbox' field.
		/// </summary>
		public static readonly StringName _hpBarHitbox = "_hpBarHitbox";

		/// <summary>
		/// Cached name for the '_creatureSize' field.
		/// </summary>
		public static readonly StringName _creatureSize = "_creatureSize";

		/// <summary>
		/// Cached name for the '_showHideTween' field.
		/// </summary>
		public static readonly StringName _showHideTween = "_showHideTween";

		/// <summary>
		/// Cached name for the '_hoverTween' field.
		/// </summary>
		public static readonly StringName _hoverTween = "_hoverTween";

		/// <summary>
		/// Cached name for the '_originalPosition' field.
		/// </summary>
		public static readonly StringName _originalPosition = "_originalPosition";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private NPowerContainer _powerContainer;

	private Control _nameplateContainer;

	private MegaLabel _nameplateLabel;

	private NHealthBar _healthBar;

	private Control _hpBarHitbox;

	private Creature? _creature;

	private Vector2 _creatureSize;

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Nodes.Combat.NCreature.TrackBlockStatus(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> for details.
	/// </summary>
	private Creature? _blockTrackingCreature;

	private Tween? _showHideTween;

	private Tween? _hoverTween;

	private static readonly Vector2 _healthBarAnimOffset = new Vector2(0f, 20f);

	private Vector2 _originalPosition;

	public override void _Ready()
	{
		_powerContainer = GetNode<NPowerContainer>("%PowerContainer");
		_nameplateContainer = GetNode<Control>("%NameplateContainer");
		_nameplateLabel = GetNode<MegaLabel>("%NameplateLabel");
		_healthBar = GetNode<NHealthBar>("%HealthBar");
		_hpBarHitbox = GetNode<Control>("%HpBarHitbox");
		_nameplateContainer.Modulate = StsColors.transparentWhite;
		_originalPosition = base.Position;
		_hpBarHitbox.Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
		_hpBarHitbox.Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		SubscribeToCreatureEvents();
		if (NCombatRoom.Instance != null)
		{
			NCombatRoom.Instance.Ui.DebugToggleHpBar += DebugToggleVisibility;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		if (_creature != null)
		{
			_creature.BlockChanged -= AnimateInBlock;
			_creature.Died -= OnCreatureDied;
			_creature.Revived -= OnCreatureRevived;
		}
		if (_blockTrackingCreature != null)
		{
			_blockTrackingCreature.BlockChanged -= OnBlockTrackingCreatureBlockChanged;
		}
		if (NCombatRoom.Instance != null)
		{
			NCombatRoom.Instance.Ui.DebugToggleHpBar -= DebugToggleVisibility;
		}
	}

	public void SetCreature(Creature creature)
	{
		if (_creature != null)
		{
			throw new InvalidOperationException("Creature was already set.");
		}
		_creature = creature;
		SubscribeToCreatureEvents();
		_nameplateLabel.SetTextAutoSize(creature.Name);
		_powerContainer.SetCreature(_creature);
		_healthBar.SetCreature(_creature);
		RefreshValues();
	}

	private void SubscribeToCreatureEvents()
	{
		if (_creature != null)
		{
			_creature.BlockChanged += AnimateInBlock;
			_creature.Died += OnCreatureDied;
			_creature.Revived += OnCreatureRevived;
		}
	}

	private void DebugToggleVisibility()
	{
		base.Visible = !NCombatUi.IsDebugHidingHpBar;
	}

	public void SetCreatureBounds(Control bounds)
	{
		_healthBar.UpdateLayoutForCreatureBounds(bounds);
		_nameplateContainer.GlobalPosition = new Vector2(bounds.GlobalPosition.X, _nameplateContainer.GlobalPosition.Y);
		_nameplateContainer.Size = new Vector2(bounds.Size.X * bounds.Scale.X, _nameplateContainer.Size.Y);
		_powerContainer.SetCreatureBounds(bounds);
		RefreshValues();
	}

	private void RefreshValues()
	{
		if (_creature != null)
		{
			_nameplateLabel.SetTextAutoSize(_creature.Name);
			_healthBar.RefreshValues();
		}
	}

	private void OnCombatStateChanged(CombatState _)
	{
		RefreshValues();
	}

	private void OnHovered()
	{
		_healthBar.FadeOutHpLabel(0.5f, 0.1f);
		ShowNameplate();
		if (!NTargetManager.Instance.IsInSelection)
		{
			NCombatRoom.Instance?.GetCreatureNode(_creature)?.ShowHoverTips(_creature.HoverTips);
		}
	}

	private void OnUnhovered()
	{
		_healthBar.FadeInHpLabel(0.5f);
		HideNameplate();
		NCombatRoom.Instance.GetCreatureNode(_creature)?.HideHoverTips();
	}

	public void ShowNameplate()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(_powerContainer, "modulate:a", 0.5f, 0.15);
		_hoverTween.TweenProperty(_nameplateContainer, "modulate:a", 1f, 0.15);
	}

	public void HideNameplate()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(_powerContainer, "modulate:a", 1f, 0.2);
		_hoverTween.TweenProperty(_nameplateContainer, "modulate:a", 0f, 0.2);
	}

	public void HideImmediately()
	{
		_showHideTween?.Kill();
		_hoverTween?.Kill();
		Color modulate = base.Modulate;
		modulate.A = 0f;
		base.Modulate = modulate;
	}

	public void AnimateIn(HealthBarAnimMode mode)
	{
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			Color modulate = base.Modulate;
			modulate.A = 1f;
			base.Modulate = modulate;
			base.Visible = true;
			return;
		}
		float num = 0f;
		base.Visible = true;
		base.Modulate = StsColors.transparentWhite;
		base.Position -= _healthBarAnimOffset;
		if (mode == HealthBarAnimMode.SpawnedAtCombatStart)
		{
			num = Rng.Chaotic.NextFloat(1.3f, 1.7f);
		}
		_showHideTween?.Kill();
		_showHideTween = CreateTween().SetParallel();
		_showHideTween.TweenProperty(this, "modulate:a", 1f, (mode == HealthBarAnimMode.FromHidden) ? 0.15 : 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine)
			.SetDelay(num);
		_showHideTween.TweenProperty(this, "position", _originalPosition, (mode == HealthBarAnimMode.FromHidden) ? 0.15 : 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad)
			.SetDelay(num);
	}

	private void AnimateInBlock(int oldBlock, int blockGain)
	{
		if (oldBlock == 0 && blockGain != 0)
		{
			_healthBar.AnimateInBlock(oldBlock, blockGain);
		}
	}

	public void AnimateOut()
	{
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			Color modulate = base.Modulate;
			modulate.A = 0f;
			base.Modulate = modulate;
			base.Visible = false;
			return;
		}
		_showHideTween?.Kill();
		_showHideTween = CreateTween().SetParallel();
		_showHideTween.TweenProperty(this, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_showHideTween.TweenProperty(this, "position", _healthBarAnimOffset, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
		_showHideTween.Chain().TweenCallback(Callable.From(() => base.Visible = false));
	}

	private void OnCreatureDied(Creature _)
	{
		_hpBarHitbox.MouseFilter = MouseFilterEnum.Ignore;
	}

	private void OnCreatureRevived(Creature _)
	{
		_hpBarHitbox.MouseFilter = MouseFilterEnum.Stop;
	}

	public void TrackBlockStatus(Creature creature)
	{
		_blockTrackingCreature = creature;
		_blockTrackingCreature.BlockChanged += OnBlockTrackingCreatureBlockChanged;
		_healthBar.TrackBlockStatus(creature);
	}

	/// <summary>
	/// See <see cref="M:MegaCrit.Sts2.Core.Nodes.Combat.NCreature.TrackBlockStatus(MegaCrit.Sts2.Core.Entities.Creatures.Creature)" /> for details.
	/// </summary>
	private void OnBlockTrackingCreatureBlockChanged(int oldBlock, int blockGain)
	{
		_healthBar.RefreshValues();
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
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SubscribeToCreatureEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCreatureBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "bounds", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshValues, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowNameplate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideNameplate, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "mode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateInBlock, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldBlock", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "blockGain", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBlockTrackingCreatureBlockChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldBlock", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "blockGain", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.SubscribeToCreatureEvents && args.Count == 0)
		{
			SubscribeToCreatureEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugToggleVisibility && args.Count == 0)
		{
			DebugToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCreatureBounds && args.Count == 1)
		{
			SetCreatureBounds(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshValues && args.Count == 0)
		{
			RefreshValues();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnHovered && args.Count == 0)
		{
			OnHovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnhovered && args.Count == 0)
		{
			OnUnhovered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowNameplate && args.Count == 0)
		{
			ShowNameplate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideNameplate && args.Count == 0)
		{
			HideNameplate();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideImmediately && args.Count == 0)
		{
			HideImmediately();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 1)
		{
			AnimateIn(VariantUtils.ConvertTo<HealthBarAnimMode>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateInBlock && args.Count == 2)
		{
			AnimateInBlock(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateOut && args.Count == 0)
		{
			AnimateOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBlockTrackingCreatureBlockChanged && args.Count == 2)
		{
			OnBlockTrackingCreatureBlockChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SubscribeToCreatureEvents)
		{
			return true;
		}
		if (method == MethodName.DebugToggleVisibility)
		{
			return true;
		}
		if (method == MethodName.SetCreatureBounds)
		{
			return true;
		}
		if (method == MethodName.RefreshValues)
		{
			return true;
		}
		if (method == MethodName.OnHovered)
		{
			return true;
		}
		if (method == MethodName.OnUnhovered)
		{
			return true;
		}
		if (method == MethodName.ShowNameplate)
		{
			return true;
		}
		if (method == MethodName.HideNameplate)
		{
			return true;
		}
		if (method == MethodName.HideImmediately)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.AnimateInBlock)
		{
			return true;
		}
		if (method == MethodName.AnimateOut)
		{
			return true;
		}
		if (method == MethodName.OnBlockTrackingCreatureBlockChanged)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._powerContainer)
		{
			_powerContainer = VariantUtils.ConvertTo<NPowerContainer>(in value);
			return true;
		}
		if (name == PropertyName._nameplateContainer)
		{
			_nameplateContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			_nameplateLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._healthBar)
		{
			_healthBar = VariantUtils.ConvertTo<NHealthBar>(in value);
			return true;
		}
		if (name == PropertyName._hpBarHitbox)
		{
			_hpBarHitbox = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._creatureSize)
		{
			_creatureSize = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._showHideTween)
		{
			_showHideTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			_originalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._powerContainer)
		{
			value = VariantUtils.CreateFrom(in _powerContainer);
			return true;
		}
		if (name == PropertyName._nameplateContainer)
		{
			value = VariantUtils.CreateFrom(in _nameplateContainer);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			value = VariantUtils.CreateFrom(in _nameplateLabel);
			return true;
		}
		if (name == PropertyName._healthBar)
		{
			value = VariantUtils.CreateFrom(in _healthBar);
			return true;
		}
		if (name == PropertyName._hpBarHitbox)
		{
			value = VariantUtils.CreateFrom(in _hpBarHitbox);
			return true;
		}
		if (name == PropertyName._creatureSize)
		{
			value = VariantUtils.CreateFrom(in _creatureSize);
			return true;
		}
		if (name == PropertyName._showHideTween)
		{
			value = VariantUtils.CreateFrom(in _showHideTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			value = VariantUtils.CreateFrom(in _originalPosition);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameplateContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameplateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._healthBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpBarHitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._creatureSize, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._showHideTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._powerContainer, Variant.From(in _powerContainer));
		info.AddProperty(PropertyName._nameplateContainer, Variant.From(in _nameplateContainer));
		info.AddProperty(PropertyName._nameplateLabel, Variant.From(in _nameplateLabel));
		info.AddProperty(PropertyName._healthBar, Variant.From(in _healthBar));
		info.AddProperty(PropertyName._hpBarHitbox, Variant.From(in _hpBarHitbox));
		info.AddProperty(PropertyName._creatureSize, Variant.From(in _creatureSize));
		info.AddProperty(PropertyName._showHideTween, Variant.From(in _showHideTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._originalPosition, Variant.From(in _originalPosition));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._powerContainer, out var value))
		{
			_powerContainer = value.As<NPowerContainer>();
		}
		if (info.TryGetProperty(PropertyName._nameplateContainer, out var value2))
		{
			_nameplateContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._nameplateLabel, out var value3))
		{
			_nameplateLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._healthBar, out var value4))
		{
			_healthBar = value4.As<NHealthBar>();
		}
		if (info.TryGetProperty(PropertyName._hpBarHitbox, out var value5))
		{
			_hpBarHitbox = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._creatureSize, out var value6))
		{
			_creatureSize = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._showHideTween, out var value7))
		{
			_showHideTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value8))
		{
			_hoverTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalPosition, out var value9))
		{
			_originalPosition = value9.As<Vector2>();
		}
	}
}
