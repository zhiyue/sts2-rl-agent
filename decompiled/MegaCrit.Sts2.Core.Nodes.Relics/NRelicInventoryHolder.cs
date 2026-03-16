using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Relics;

[ScriptPath("res://src/Core/Nodes/Relics/NRelicInventoryHolder.cs")]
public class NRelicInventoryHolder : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName RefreshAmount = "RefreshAmount";

		public static readonly StringName RefreshStatus = "RefreshStatus";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName DoFlash = "DoFlash";

		public static readonly StringName OnDisplayAmountChanged = "OnDisplayAmountChanged";

		public static readonly StringName OnStatusChanged = "OnStatusChanged";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName Relic = "Relic";

		public static readonly StringName Inventory = "Inventory";

		public static readonly StringName _relic = "_relic";

		public static readonly StringName _amountLabel = "_amountLabel";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _obtainedTween = "_obtainedTween";

		public static readonly StringName _originalIconPosition = "_originalIconPosition";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("relics/relic_inventory_holder");

	private static readonly string _flashPath = SceneHelper.GetScenePath("vfx/relic_inventory_flash_vfx");

	private const float _newlyAcquiredPopDuration = 0.35f;

	private const float _newlyAcquiredFadeInDuration = 0.1f;

	private const float _newlyAcquiredPopDistance = 40f;

	private NRelic _relic;

	private RelicModel? _subscribedRelic;

	private MegaLabel _amountLabel;

	private Tween? _hoverTween;

	private Tween? _obtainedTween;

	private CancellationTokenSource? _cancellationTokenSource;

	private Vector2 _originalIconPosition;

	private RelicModel _model;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _scenePath, _flashPath });

	public NRelic Relic => _relic;

	public NRelicInventory Inventory { get; set; }

	public static NRelicInventoryHolder? Create(RelicModel relic)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRelicInventoryHolder nRelicInventoryHolder = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRelicInventoryHolder>(PackedScene.GenEditState.Disabled);
		nRelicInventoryHolder.Name = $"NRelicContainerHolder-{relic.Id}";
		nRelicInventoryHolder._model = relic;
		return nRelicInventoryHolder;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_relic = GetNode<NRelic>("%Relic");
		_amountLabel = GetNode<MegaLabel>("%AmountLabel");
		_originalIconPosition = _relic.Icon.Position;
		_relic.ModelChanged += OnModelChanged;
		_relic.Model = _model;
	}

	public override void _ExitTree()
	{
		_hoverTween?.Kill();
		if (_subscribedRelic != null)
		{
			_subscribedRelic.DisplayAmountChanged -= OnDisplayAmountChanged;
			_subscribedRelic.StatusChanged -= OnStatusChanged;
			_subscribedRelic.Flashed -= OnRelicFlashed;
		}
		_subscribedRelic = null;
		_relic.ModelChanged -= OnModelChanged;
	}

	private void OnModelChanged(RelicModel? oldModel, RelicModel? newModel)
	{
		if (oldModel != null)
		{
			oldModel.DisplayAmountChanged -= OnDisplayAmountChanged;
			oldModel.StatusChanged -= OnStatusChanged;
			oldModel.Flashed -= OnRelicFlashed;
		}
		if (newModel != null)
		{
			newModel.DisplayAmountChanged += OnDisplayAmountChanged;
			newModel.StatusChanged += OnStatusChanged;
			newModel.Flashed += OnRelicFlashed;
		}
		RefreshAmount();
		RefreshStatus();
		_subscribedRelic = newModel;
	}

	private void RefreshAmount()
	{
		if (_relic.Model.ShowCounter && RunManager.Instance.IsInProgress)
		{
			_amountLabel.Visible = true;
			_amountLabel.SetTextAutoSize(_relic.Model.DisplayAmount.ToString());
		}
		else
		{
			_amountLabel.Visible = false;
		}
	}

	private void RefreshStatus()
	{
		if (!RunManager.Instance.IsInProgress)
		{
			_relic.Icon.Modulate = Colors.White;
			return;
		}
		_relic.Model.UpdateTexture(_relic.Icon);
		TextureRect icon = _relic.Icon;
		Color modulate;
		switch (_relic.Model.Status)
		{
		case RelicStatus.Normal:
		case RelicStatus.Active:
			modulate = Colors.White;
			break;
		case RelicStatus.Disabled:
			modulate = new Color("#808080");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		icon.Modulate = modulate;
	}

	public async Task PlayNewlyAcquiredAnimation(Vector2? startLocation, Vector2? startScale)
	{
		if (_cancellationTokenSource != null)
		{
			await _cancellationTokenSource.CancelAsync();
		}
		CancellationTokenSource cancelTokenSource = (_cancellationTokenSource = new CancellationTokenSource());
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (!cancelTokenSource.IsCancellationRequested)
		{
			_obtainedTween?.Kill();
			if (!startLocation.HasValue)
			{
				TextureRect icon = _relic.Icon;
				Vector2 position = _relic.Icon.Position;
				position.Y = _relic.Icon.Position.Y + 40f;
				icon.Position = position;
				TextureRect icon2 = _relic.Icon;
				Color modulate = _relic.Icon.Modulate;
				modulate.A = 0f;
				icon2.Modulate = modulate;
				_obtainedTween = GetTree().CreateTween();
				_obtainedTween.TweenProperty(_relic.Icon, "modulate:a", 1f, 0.10000000149011612);
				_obtainedTween.Parallel();
				_obtainedTween.SetEase(Tween.EaseType.Out);
				_obtainedTween.SetTrans(Tween.TransitionType.Back);
				_obtainedTween.TweenProperty(_relic.Icon, "position:y", _originalIconPosition.Y, 0.3499999940395355);
				_obtainedTween.TweenCallback(Callable.From(DoFlash));
			}
			else
			{
				_relic.Icon.GlobalPosition = startLocation.Value;
				_relic.Icon.Scale = startScale ?? Vector2.One;
				TextureRect icon3 = _relic.Icon;
				Color modulate = _relic.Icon.Modulate;
				modulate.A = 1f;
				icon3.Modulate = modulate;
				_obtainedTween = GetTree().CreateTween();
				_obtainedTween.SetEase(Tween.EaseType.Out);
				_obtainedTween.SetTrans(Tween.TransitionType.Sine);
				_obtainedTween.TweenProperty(_relic.Icon, "position", _originalIconPosition, 0.3499999940395355);
				_obtainedTween.Parallel().TweenProperty(_relic.Icon, "scale", Vector2.One, 0.3499999940395355);
				_obtainedTween.TweenCallback(Callable.From(DoFlash));
			}
		}
	}

	protected override void OnFocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relic.Icon, "scale", Vector2.One * 1.25f, 0.05);
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _relic.Model.HoverTips);
		nHoverTipSet.SetAlignmentForRelic(_relic);
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(_relic.Icon, "scale", Vector2.One, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
	}

	private void OnRelicFlashed(RelicModel _, IEnumerable<Creature> __)
	{
		DoFlash();
	}

	private void DoFlash()
	{
		Node2D node2D = PreloadManager.Cache.GetScene(_flashPath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		Node aboveTopBarVfxContainer = NRun.Instance.GlobalUi.AboveTopBarVfxContainer;
		node2D.GetNode<GpuParticles2D>("Particles").Texture = _relic.Model.Icon;
		node2D.GlobalPosition = base.GlobalPosition + base.Size * 0.5f;
		aboveTopBarVfxContainer.AddChildSafely(node2D);
	}

	private void OnDisplayAmountChanged()
	{
		RefreshAmount();
	}

	private void OnStatusChanged()
	{
		RefreshStatus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshAmount, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoFlash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnDisplayAmountChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnStatusChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshAmount && args.Count == 0)
		{
			RefreshAmount();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshStatus && args.Count == 0)
		{
			RefreshStatus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoFlash && args.Count == 0)
		{
			DoFlash();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnDisplayAmountChanged && args.Count == 0)
		{
			OnDisplayAmountChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnStatusChanged && args.Count == 0)
		{
			OnStatusChanged();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.RefreshAmount)
		{
			return true;
		}
		if (method == MethodName.RefreshStatus)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.DoFlash)
		{
			return true;
		}
		if (method == MethodName.OnDisplayAmountChanged)
		{
			return true;
		}
		if (method == MethodName.OnStatusChanged)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Inventory)
		{
			Inventory = VariantUtils.ConvertTo<NRelicInventory>(in value);
			return true;
		}
		if (name == PropertyName._relic)
		{
			_relic = VariantUtils.ConvertTo<NRelic>(in value);
			return true;
		}
		if (name == PropertyName._amountLabel)
		{
			_amountLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._obtainedTween)
		{
			_obtainedTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalIconPosition)
		{
			_originalIconPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Relic)
		{
			value = VariantUtils.CreateFrom<NRelic>(Relic);
			return true;
		}
		if (name == PropertyName.Inventory)
		{
			value = VariantUtils.CreateFrom<NRelicInventory>(Inventory);
			return true;
		}
		if (name == PropertyName._relic)
		{
			value = VariantUtils.CreateFrom(in _relic);
			return true;
		}
		if (name == PropertyName._amountLabel)
		{
			value = VariantUtils.CreateFrom(in _amountLabel);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._obtainedTween)
		{
			value = VariantUtils.CreateFrom(in _obtainedTween);
			return true;
		}
		if (name == PropertyName._originalIconPosition)
		{
			value = VariantUtils.CreateFrom(in _originalIconPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relic, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._amountLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._obtainedTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalIconPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Relic, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Inventory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Inventory, Variant.From<NRelicInventory>(Inventory));
		info.AddProperty(PropertyName._relic, Variant.From(in _relic));
		info.AddProperty(PropertyName._amountLabel, Variant.From(in _amountLabel));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._obtainedTween, Variant.From(in _obtainedTween));
		info.AddProperty(PropertyName._originalIconPosition, Variant.From(in _originalIconPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Inventory, out var value))
		{
			Inventory = value.As<NRelicInventory>();
		}
		if (info.TryGetProperty(PropertyName._relic, out var value2))
		{
			_relic = value2.As<NRelic>();
		}
		if (info.TryGetProperty(PropertyName._amountLabel, out var value3))
		{
			_amountLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value4))
		{
			_hoverTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._obtainedTween, out var value5))
		{
			_obtainedTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalIconPosition, out var value6))
		{
			_originalIconPosition = value6.As<Vector2>();
		}
	}
}
