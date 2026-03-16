using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NPower.cs")]
public class NPower : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName Reload = "Reload";

		public static readonly StringName OnPulsingStarted = "OnPulsingStarted";

		public static readonly StringName OnPulsingStopped = "OnPulsingStopped";

		public static readonly StringName RefreshAmount = "RefreshAmount";

		public static readonly StringName OnDisplayAmountChanged = "OnDisplayAmountChanged";

		public static readonly StringName FlashPower = "FlashPower";

		public static readonly StringName OnHovered = "OnHovered";

		public static readonly StringName OnUnhovered = "OnUnhovered";

		public static readonly StringName SubscribeToModelEvents = "SubscribeToModelEvents";

		public static readonly StringName UnsubscribeFromModelEvents = "UnsubscribeFromModelEvents";

		public static readonly StringName OnPowerRemoved = "OnPowerRemoved";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Container = "Container";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _amountLabel = "_amountLabel";

		public static readonly StringName _powerFlash = "_powerFlash";

		public static readonly StringName _animInTween = "_animInTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly StringName _pulse = new StringName("pulse");

	private PowerModel? _model;

	private TextureRect _icon;

	private MegaLabel _amountLabel;

	private CpuParticles2D _powerFlash;

	private Tween? _animInTween;

	public NPowerContainer Container { get; set; }

	private static string ScenePath => SceneHelper.GetScenePath("combat/power");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public PowerModel Model
	{
		get
		{
			return _model ?? throw new InvalidOperationException("Model was accessed before it was set.");
		}
		set
		{
			if (_model != null)
			{
				UnsubscribeFromModelEvents();
			}
			value.AssertMutable();
			_model = value;
			if (_model != null && IsInsideTree())
			{
				SubscribeToModelEvents();
			}
			Reload();
		}
	}

	public static NPower Create(PowerModel power)
	{
		NPower nPower = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NPower>(PackedScene.GenEditState.Disabled);
		nPower.Model = power;
		return nPower;
	}

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
		_amountLabel = GetNode<MegaLabel>("%AmountLabel");
		_powerFlash = GetNode<CpuParticles2D>("%PowerFlash");
		Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
		Reload();
		_animInTween?.Kill();
		_animInTween = CreateTween().SetParallel();
		_animInTween.TweenProperty(_icon, "position:y", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(-24f);
		_animInTween.TweenProperty(this, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public override void _EnterTree()
	{
		SubscribeToModelEvents();
	}

	public override void _ExitTree()
	{
		UnsubscribeFromModelEvents();
	}

	private void Reload()
	{
		if (IsNodeReady())
		{
			_icon.Texture = _model?.Icon;
			_powerFlash.Texture = _model?.BigIcon;
			RefreshAmount();
		}
	}

	private void OnPulsingStarted()
	{
		ShaderMaterial shaderMaterial = (ShaderMaterial)_icon.Material;
		shaderMaterial.SetShaderParameter(_pulse, 1);
	}

	private void OnPulsingStopped()
	{
		ShaderMaterial shaderMaterial = (ShaderMaterial)_icon.Material;
		shaderMaterial.SetShaderParameter(_pulse, 0);
	}

	private void RefreshAmount()
	{
		if (_model != null)
		{
			_amountLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, Model.AmountLabelColor);
			_amountLabel.SetTextAutoSize((Model.StackType == PowerStackType.Counter) ? Model.DisplayAmount.ToString() : string.Empty);
		}
		else
		{
			_amountLabel.SetTextAutoSize(string.Empty);
		}
	}

	private void OnDisplayAmountChanged()
	{
		FlashPower();
		RefreshAmount();
	}

	private void OnPowerFlashed(PowerModel _)
	{
		FlashPower();
	}

	private void FlashPower()
	{
		_powerFlash.Emitting = true;
	}

	private void OnHovered()
	{
		NCombatRoom.Instance?.GetCreatureNode(Model.Owner)?.ShowHoverTips(Model.HoverTips);
		_icon.Scale = Vector2.One * 1.1f;
		CombatManager.Instance.StateTracker.CombatStateChanged += ShowPowerHoverTips;
	}

	private void OnUnhovered()
	{
		NCombatRoom.Instance?.GetCreatureNode(Model.Owner)?.HideHoverTips();
		_icon.Scale = Vector2.One * 1f;
		CombatManager.Instance.StateTracker.CombatStateChanged -= ShowPowerHoverTips;
	}

	private void ShowPowerHoverTips(CombatState _)
	{
		NCombatRoom.Instance?.GetCreatureNode(Model.Owner)?.ShowHoverTips(Model.HoverTips);
	}

	private void SubscribeToModelEvents()
	{
		if (_model != null)
		{
			Model.DisplayAmountChanged += OnDisplayAmountChanged;
			Model.Flashed += OnPowerFlashed;
			Model.Removed += OnPowerRemoved;
			Model.Owner.Died += OnOwnerDied;
			Model.Owner.Revived += OnOwnerRevived;
			Model.PulsingStarted += OnPulsingStarted;
			Model.PulsingStopped += OnPulsingStopped;
		}
	}

	private void UnsubscribeFromModelEvents()
	{
		if (_model != null)
		{
			Model.DisplayAmountChanged -= OnDisplayAmountChanged;
			Model.Flashed -= OnPowerFlashed;
			Model.Removed -= OnPowerRemoved;
			Model.Owner.Died -= OnOwnerDied;
			Model.Owner.Revived -= OnOwnerRevived;
			Model.PulsingStarted -= OnPulsingStarted;
			Model.PulsingStopped -= OnPulsingStopped;
		}
	}

	private void OnPowerRemoved()
	{
		UnsubscribeFromModelEvents();
	}

	private void OnOwnerDied(Creature _)
	{
		if (GodotObject.IsInstanceValid(this) && Model.ShouldPowerBeRemovedAfterOwnerDeath())
		{
			base.MouseFilter = MouseFilterEnum.Ignore;
		}
	}

	private void OnOwnerRevived(Creature _)
	{
		base.MouseFilter = MouseFilterEnum.Stop;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPulsingStarted, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPulsingStopped, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshAmount, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisplayAmountChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.FlashPower, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SubscribeToModelEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnsubscribeFromModelEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPowerRemoved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPulsingStarted && args.Count == 0)
		{
			OnPulsingStarted();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPulsingStopped && args.Count == 0)
		{
			OnPulsingStopped();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshAmount && args.Count == 0)
		{
			RefreshAmount();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisplayAmountChanged && args.Count == 0)
		{
			OnDisplayAmountChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FlashPower && args.Count == 0)
		{
			FlashPower();
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
		if (method == MethodName.SubscribeToModelEvents && args.Count == 0)
		{
			SubscribeToModelEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnsubscribeFromModelEvents && args.Count == 0)
		{
			UnsubscribeFromModelEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPowerRemoved && args.Count == 0)
		{
			OnPowerRemoved();
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
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.OnPulsingStarted)
		{
			return true;
		}
		if (method == MethodName.OnPulsingStopped)
		{
			return true;
		}
		if (method == MethodName.RefreshAmount)
		{
			return true;
		}
		if (method == MethodName.OnDisplayAmountChanged)
		{
			return true;
		}
		if (method == MethodName.FlashPower)
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
		if (method == MethodName.SubscribeToModelEvents)
		{
			return true;
		}
		if (method == MethodName.UnsubscribeFromModelEvents)
		{
			return true;
		}
		if (method == MethodName.OnPowerRemoved)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Container)
		{
			Container = VariantUtils.ConvertTo<NPowerContainer>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._amountLabel)
		{
			_amountLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._powerFlash)
		{
			_powerFlash = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			_animInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Container)
		{
			value = VariantUtils.CreateFrom<NPowerContainer>(Container);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._amountLabel)
		{
			value = VariantUtils.CreateFrom(in _amountLabel);
			return true;
		}
		if (name == PropertyName._powerFlash)
		{
			value = VariantUtils.CreateFrom(in _powerFlash);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			value = VariantUtils.CreateFrom(in _animInTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._amountLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._powerFlash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Container, Variant.From<NPowerContainer>(Container));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._amountLabel, Variant.From(in _amountLabel));
		info.AddProperty(PropertyName._powerFlash, Variant.From(in _powerFlash));
		info.AddProperty(PropertyName._animInTween, Variant.From(in _animInTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Container, out var value))
		{
			Container = value.As<NPowerContainer>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._amountLabel, out var value3))
		{
			_amountLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._powerFlash, out var value4))
		{
			_powerFlash = value4.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._animInTween, out var value5))
		{
			_animInTween = value5.As<Tween>();
		}
	}
}
