using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NEnergyCounter.cs")]
public class NEnergyCounter : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnHovered = "OnHovered";

		public static readonly StringName OnUnhovered = "OnUnhovered";

		public static readonly StringName RefreshLabel = "RefreshLabel";

		public static readonly StringName OnEnergyChanged = "OnEnergyChanged";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName AnimOut = "AnimOut";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName OutlineColor = "OutlineColor";

		public static readonly StringName _label = "_label";

		public static readonly StringName _layers = "_layers";

		public static readonly StringName _rotationLayers = "_rotationLayers";

		public static readonly StringName _backParticles = "_backParticles";

		public static readonly StringName _frontParticles = "_frontParticles";

		public static readonly StringName _animInTween = "_animInTween";

		public static readonly StringName _animOutTween = "_animOutTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _darkenedMatPath = "res://materials/ui/energy_orb_dark.tres";

	private Player _player;

	private MegaLabel _label;

	private Control _layers;

	private Control _rotationLayers;

	private CpuParticles2D _backParticles;

	private CpuParticles2D _frontParticles;

	private HoverTip _hoverTip;

	private Tween? _animInTween;

	private Tween? _animOutTween;

	private const float _animDuration = 0.6f;

	private static readonly Vector2 _showPosition = Vector2.Zero;

	private static readonly Vector2 _hidePosition = new Vector2(-480f, 128f);

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://materials/ui/energy_orb_dark.tres");

	private Color OutlineColor => _player.Character.EnergyLabelOutlineColor;

	public static NEnergyCounter? Create(Player player)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NEnergyCounter nEnergyCounter = PreloadManager.Cache.GetScene(player.Character.EnergyCounterPath).Instantiate<NEnergyCounter>(PackedScene.GenEditState.Disabled);
		nEnergyCounter._player = player;
		return nEnergyCounter;
	}

	public override void _Ready()
	{
		_label = GetNode<MegaLabel>("Label");
		_layers = GetNode<Control>("%Layers");
		_rotationLayers = GetNode<Control>("%RotationLayers");
		_backParticles = GetNode<CpuParticles2D>("%BurstBack");
		_frontParticles = GetNode<CpuParticles2D>("%BurstFront");
		LocString locString = new LocString("static_hover_tips", "ENERGY_COUNT.description");
		locString.Add("energyPrefix", EnergyIconHelper.GetPrefix(_player.Character.CardPool));
		_hoverTip = new HoverTip(new LocString("static_hover_tips", "ENERGY_COUNT.title"), locString);
		Connect(Control.SignalName.MouseEntered, Callable.From(OnHovered));
		Connect(Control.SignalName.MouseExited, Callable.From(OnUnhovered));
		RefreshLabel();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		CombatManager.Instance.StateTracker.CombatStateChanged += OnCombatStateChanged;
		_player.PlayerCombatState.EnergyChanged += OnEnergyChanged;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		CombatManager.Instance.StateTracker.CombatStateChanged -= OnCombatStateChanged;
		_player.PlayerCombatState.EnergyChanged -= OnEnergyChanged;
	}

	private void OnHovered()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(-70f, -200f);
	}

	private void OnUnhovered()
	{
		NHoverTipSet.Remove(this);
	}

	private void OnCombatStateChanged(CombatState combatState)
	{
		RefreshLabel();
	}

	private void RefreshLabel()
	{
		PlayerCombatState playerCombatState = _player.PlayerCombatState;
		_label.SetTextAutoSize($"{playerCombatState.Energy}/{playerCombatState.MaxEnergy}");
		_label.AddThemeColorOverride(ThemeConstants.Label.fontColor, (playerCombatState.Energy == 0) ? StsColors.red : StsColors.cream);
		_label.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, (playerCombatState.Energy == 0) ? StsColors.unplayableEnergyCostOutline : OutlineColor);
		Material material = ((playerCombatState.Energy == 0) ? PreloadManager.Cache.GetMaterial("res://materials/ui/energy_orb_dark.tres") : null);
		foreach (Control item in _layers.GetChildren().OfType<Control>())
		{
			item.Material = material;
		}
		foreach (Control item2 in _rotationLayers.GetChildren().OfType<Control>())
		{
			item2.Material = material;
		}
		_layers.Modulate = ((playerCombatState.Energy == 0) ? Colors.DarkGray : Colors.White);
	}

	private void OnEnergyChanged(int oldEnergy, int newEnergy)
	{
		if (oldEnergy < newEnergy)
		{
			_frontParticles.Emitting = true;
			_backParticles.Emitting = true;
		}
	}

	public override void _Process(double delta)
	{
		float num = ((_player.PlayerCombatState.Energy == 0) ? 5f : 30f);
		for (int i = 0; i < _rotationLayers.GetChildCount(); i++)
		{
			_rotationLayers.GetChild<Control>(i).RotationDegrees += (float)delta * num * (float)(i + 1);
		}
	}

	public void AnimIn()
	{
		_animOutTween?.Kill();
		_animInTween = CreateTween();
		base.Position = _hidePosition;
		_animInTween.TweenProperty(this, "position", _showPosition, 0.6000000238418579).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void AnimOut()
	{
		_animInTween?.Kill();
		_animOutTween = CreateTween();
		base.Position = _showPosition;
		_animOutTween.TweenProperty(this, "position", _hidePosition, 0.6000000238418579).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnergyChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldEnergy", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "newEnergy", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RefreshLabel && args.Count == 0)
		{
			RefreshLabel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnergyChanged && args.Count == 2)
		{
			OnEnergyChanged(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
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
		if (method == MethodName.OnHovered)
		{
			return true;
		}
		if (method == MethodName.OnUnhovered)
		{
			return true;
		}
		if (method == MethodName.RefreshLabel)
		{
			return true;
		}
		if (method == MethodName.OnEnergyChanged)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._label)
		{
			_label = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._layers)
		{
			_layers = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._rotationLayers)
		{
			_rotationLayers = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backParticles)
		{
			_backParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._frontParticles)
		{
			_frontParticles = VariantUtils.ConvertTo<CpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			_animInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._animOutTween)
		{
			_animOutTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.OutlineColor)
		{
			value = VariantUtils.CreateFrom<Color>(OutlineColor);
			return true;
		}
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._layers)
		{
			value = VariantUtils.CreateFrom(in _layers);
			return true;
		}
		if (name == PropertyName._rotationLayers)
		{
			value = VariantUtils.CreateFrom(in _rotationLayers);
			return true;
		}
		if (name == PropertyName._backParticles)
		{
			value = VariantUtils.CreateFrom(in _backParticles);
			return true;
		}
		if (name == PropertyName._frontParticles)
		{
			value = VariantUtils.CreateFrom(in _frontParticles);
			return true;
		}
		if (name == PropertyName._animInTween)
		{
			value = VariantUtils.CreateFrom(in _animInTween);
			return true;
		}
		if (name == PropertyName._animOutTween)
		{
			value = VariantUtils.CreateFrom(in _animOutTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._layers, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rotationLayers, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._frontParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animOutTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName.OutlineColor, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._layers, Variant.From(in _layers));
		info.AddProperty(PropertyName._rotationLayers, Variant.From(in _rotationLayers));
		info.AddProperty(PropertyName._backParticles, Variant.From(in _backParticles));
		info.AddProperty(PropertyName._frontParticles, Variant.From(in _frontParticles));
		info.AddProperty(PropertyName._animInTween, Variant.From(in _animInTween));
		info.AddProperty(PropertyName._animOutTween, Variant.From(in _animOutTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._layers, out var value2))
		{
			_layers = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._rotationLayers, out var value3))
		{
			_rotationLayers = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backParticles, out var value4))
		{
			_backParticles = value4.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._frontParticles, out var value5))
		{
			_frontParticles = value5.As<CpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._animInTween, out var value6))
		{
			_animInTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._animOutTween, out var value7))
		{
			_animOutTween = value7.As<Tween>();
		}
	}
}
