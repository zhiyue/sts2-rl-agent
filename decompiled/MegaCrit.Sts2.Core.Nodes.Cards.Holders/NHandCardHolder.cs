using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Cards.Holders;

[ScriptPath("res://src/Core/Nodes/Cards/Holders/NHandCardHolder.cs")]
public class NHandCardHolder : NCardHolder
{
	[Signal]
	public delegate void HolderFocusedEventHandler(NHandCardHolder cardHolder);

	[Signal]
	public delegate void HolderUnfocusedEventHandler(NHandCardHolder cardHolder);

	[Signal]
	public delegate void HolderMouseClickedEventHandler(NCardHolder cardHolder);

	public new class MethodName : NCardHolder.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName Clear = "Clear";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnMousePressed = "OnMousePressed";

		public new static readonly StringName OnMouseReleased = "OnMouseReleased";

		public new static readonly StringName DoCardHoverEffects = "DoCardHoverEffects";

		public static readonly StringName SetIndexLabel = "SetIndexLabel";

		public static readonly StringName SetTargetAngle = "SetTargetAngle";

		public static readonly StringName SetTargetPosition = "SetTargetPosition";

		public static readonly StringName SetTargetScale = "SetTargetScale";

		public static readonly StringName SetAngleInstantly = "SetAngleInstantly";

		public static readonly StringName SetScaleInstantly = "SetScaleInstantly";

		public static readonly StringName StopAnimations = "StopAnimations";

		public new static readonly StringName SetCard = "SetCard";

		public static readonly StringName UpdateCard = "UpdateCard";

		public static readonly StringName BeginDrag = "BeginDrag";

		public static readonly StringName CancelDrag = "CancelDrag";

		public static readonly StringName SetDefaultTargets = "SetDefaultTargets";

		public static readonly StringName Flash = "Flash";
	}

	public new class PropertyName : NCardHolder.PropertyName
	{
		public static readonly StringName InSelectMode = "InSelectMode";

		public static readonly StringName TargetPosition = "TargetPosition";

		public static readonly StringName TargetAngle = "TargetAngle";

		public static readonly StringName ShouldGlowGold = "ShouldGlowGold";

		public static readonly StringName ShouldGlowRed = "ShouldGlowRed";

		public static readonly StringName _flash = "_flash";

		public static readonly StringName _flashTween = "_flashTween";

		public static readonly StringName _handIndexLabel = "_handIndexLabel";

		public static readonly StringName _targetPosition = "_targetPosition";

		public static readonly StringName _targetAngle = "_targetAngle";

		public static readonly StringName _targetScale = "_targetScale";

		public static readonly StringName _hand = "_hand";
	}

	public new class SignalName : NCardHolder.SignalName
	{
		public static readonly StringName HolderFocused = "HolderFocused";

		public static readonly StringName HolderUnfocused = "HolderUnfocused";

		public static readonly StringName HolderMouseClicked = "HolderMouseClicked";
	}

	private Control _flash;

	private Tween? _flashTween;

	private MegaLabel _handIndexLabel;

	private const float _rotateSpeed = 10f;

	private const float _angleSnapThreshold = 0.1f;

	private const float _scaleSpeed = 8f;

	private const float _scaleSnapThreshold = 0.002f;

	private const float _moveSpeed = 7f;

	private const float _positionSnapThreshold = 1f;

	private const float _reenableHitboxThreshold = 200f;

	private Vector2 _targetPosition;

	private float _targetAngle;

	private Vector2 _targetScale;

	private CancellationTokenSource? _angleCancelToken;

	private CancellationTokenSource? _positionCancelToken;

	private CancellationTokenSource? _scaleCancelToken;

	private NPlayerHand _hand;

	private HolderFocusedEventHandler backing_HolderFocused;

	private HolderUnfocusedEventHandler backing_HolderUnfocused;

	private HolderMouseClickedEventHandler backing_HolderMouseClicked;

	public bool InSelectMode { get; set; }

	public Vector2 TargetPosition => _targetPosition;

	public float TargetAngle => _targetAngle;

	private static string ScenePath => SceneHelper.GetScenePath("cards/holders/hand_card_holder");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	private bool ShouldGlowGold
	{
		get
		{
			CardModel cardModel = base.CardNode?.Model;
			if (cardModel == null)
			{
				return false;
			}
			if (_hand.SelectModeGoldGlowOverride != null)
			{
				return _hand.SelectModeGoldGlowOverride(cardModel);
			}
			if (CombatManager.Instance.IsPlayPhase && cardModel.CanPlay())
			{
				return cardModel.ShouldGlowGold;
			}
			return false;
		}
	}

	private bool ShouldGlowRed
	{
		get
		{
			CardModel cardModel = base.CardNode?.Model;
			if (cardModel == null)
			{
				return false;
			}
			if (CombatManager.Instance.IsPlayPhase)
			{
				return cardModel.ShouldGlowRed;
			}
			return false;
		}
	}

	public event HolderFocusedEventHandler HolderFocused
	{
		add
		{
			backing_HolderFocused = (HolderFocusedEventHandler)Delegate.Combine(backing_HolderFocused, value);
		}
		remove
		{
			backing_HolderFocused = (HolderFocusedEventHandler)Delegate.Remove(backing_HolderFocused, value);
		}
	}

	public event HolderUnfocusedEventHandler HolderUnfocused
	{
		add
		{
			backing_HolderUnfocused = (HolderUnfocusedEventHandler)Delegate.Combine(backing_HolderUnfocused, value);
		}
		remove
		{
			backing_HolderUnfocused = (HolderUnfocusedEventHandler)Delegate.Remove(backing_HolderUnfocused, value);
		}
	}

	public event HolderMouseClickedEventHandler HolderMouseClicked
	{
		add
		{
			backing_HolderMouseClicked = (HolderMouseClickedEventHandler)Delegate.Combine(backing_HolderMouseClicked, value);
		}
		remove
		{
			backing_HolderMouseClicked = (HolderMouseClickedEventHandler)Delegate.Remove(backing_HolderMouseClicked, value);
		}
	}

	public static NHandCardHolder Create(NCard card, NPlayerHand hand)
	{
		NHandCardHolder nHandCardHolder = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NHandCardHolder>(PackedScene.GenEditState.Disabled);
		nHandCardHolder.Name = $"{nHandCardHolder.GetType().Name}-{card.Model.Id}";
		nHandCardHolder.SetCard(card);
		nHandCardHolder._hand = hand;
		return nHandCardHolder;
	}

	public override void _Ready()
	{
		ConnectSignals();
		_flash = GetNode<Control>("Flash");
		_flash.Modulate = new Color(_flash.Modulate.R, _flash.Modulate.G, _flash.Modulate.B, 0f);
		_handIndexLabel = GetNode<MegaLabel>("%HandIndex");
		UpdateCard();
		base.Hitbox.SetEnabled(enabled: false);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		UnsubscribeFromEvents(base.CardNode?.Model);
		StopAnimations();
	}

	public override void Clear()
	{
		UnsubscribeFromEvents(base.CardNode?.Model);
		base.Clear();
		StopAnimations();
	}

	protected override void OnFocus()
	{
		EmitSignal(SignalName.HolderFocused, this);
		base.OnFocus();
	}

	protected override void OnUnfocus()
	{
		EmitSignal(SignalName.HolderUnfocused, this);
		base.OnUnfocus();
	}

	protected override void OnMousePressed(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left && _isClickable)
		{
			SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
			EmitSignal(SignalName.HolderMouseClicked, this);
		}
	}

	protected override void OnMouseReleased(InputEvent inputEvent)
	{
	}

	protected override void DoCardHoverEffects(bool isHovered)
	{
		base.ZIndex = (isHovered ? 1 : 0);
		if (isHovered)
		{
			CreateHoverTips();
		}
		else
		{
			ClearHoverTips();
		}
	}

	public void SetIndexLabel(int i)
	{
		_handIndexLabel.SetTextAutoSize(i.ToString());
		_handIndexLabel.Visible = i > 0 && SaveManager.Instance.PrefsSave.ShowCardIndices;
	}

	public void SetTargetAngle(float angle)
	{
		_targetAngle = angle;
		_angleCancelToken?.Cancel();
		_angleCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimAngle(_angleCancelToken));
	}

	public void SetTargetPosition(Vector2 position)
	{
		_targetPosition = position;
		_positionCancelToken?.Cancel();
		_positionCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimPosition(_positionCancelToken));
	}

	public void SetTargetScale(Vector2 scale)
	{
		_targetScale = scale;
		_scaleCancelToken?.Cancel();
		_scaleCancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(AnimScale(_scaleCancelToken));
	}

	public void SetAngleInstantly(float setAngle)
	{
		_angleCancelToken?.Cancel();
		base.RotationDegrees = setAngle;
	}

	public void SetScaleInstantly(Vector2 setScale)
	{
		_scaleCancelToken?.Cancel();
		base.Scale = setScale;
	}

	private void StopAnimations()
	{
		_angleCancelToken?.Cancel();
		_positionCancelToken?.Cancel();
		_scaleCancelToken?.Cancel();
	}

	private async Task AnimAngle(CancellationTokenSource cancelToken)
	{
		while (!cancelToken.IsCancellationRequested)
		{
			base.RotationDegrees = Mathf.Lerp(base.RotationDegrees, _targetAngle, (float)GetProcessDeltaTime() * 10f);
			if (Mathf.Abs(base.RotationDegrees - _targetAngle) < 0.1f)
			{
				base.RotationDegrees = _targetAngle;
				break;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task AnimScale(CancellationTokenSource cancelToken)
	{
		while (!cancelToken.IsCancellationRequested)
		{
			base.Scale = base.Scale.Lerp(_targetScale, (float)GetProcessDeltaTime() * 8f);
			if (Mathf.Abs(_targetScale.X - base.Scale.X) < 0.002f)
			{
				base.Scale = _targetScale;
				break;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private async Task AnimPosition(CancellationTokenSource cancelToken)
	{
		while (!cancelToken.IsCancellationRequested)
		{
			base.Position = base.Position.Lerp(_targetPosition, (float)GetProcessDeltaTime() * 7f);
			float num = Mathf.Abs(base.Position.X - _targetPosition.X);
			if (!base.Hitbox.IsEnabled && num < 200f)
			{
				base.Hitbox.SetEnabled(enabled: true);
			}
			if (base.Position.DistanceSquaredTo(_targetPosition) < 1f)
			{
				base.Position = _targetPosition;
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		if (!base.Hitbox.IsEnabled && base.Position.DistanceSquaredTo(_targetPosition) < 200f)
		{
			base.Hitbox.SetEnabled(enabled: true);
		}
	}

	protected override void SetCard(NCard node)
	{
		if (base.CardNode != null)
		{
			base.CardNode.ModelChanged -= OnModelChanged;
		}
		UnsubscribeFromEvents(base.CardNode?.Model);
		base.SetCard(node);
		UpdateCard();
		SubscribeToEvents(base.CardNode?.Model);
		if (base.CardNode != null)
		{
			base.CardNode.ModelChanged += OnModelChanged;
		}
		if (node.Scale != Vector2.One)
		{
			node.CreateTween().TweenProperty(node, "scale", Vector2.One, 0.25);
		}
	}

	public void UpdateCard()
	{
		if (!IsNodeReady() || base.CardNode == null)
		{
			return;
		}
		base.CardNode.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
		if (!CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (base.CardNode.Model.CanPlay() || ShouldGlowRed || ShouldGlowGold)
		{
			base.CardNode.CardHighlight.AnimShow();
			base.CardNode.CardHighlight.Modulate = NCardHighlight.playableColor;
			if (ShouldGlowRed)
			{
				base.CardNode.CardHighlight.Modulate = NCardHighlight.red;
			}
			else if (ShouldGlowGold)
			{
				base.CardNode.CardHighlight.Modulate = NCardHighlight.gold;
			}
		}
		else
		{
			base.CardNode.CardHighlight.AnimHide();
		}
	}

	public void BeginDrag()
	{
		SetAngleInstantly(0f);
		SetScaleInstantly(HoverScale);
	}

	public void CancelDrag()
	{
		base.ZIndex = 0;
		SetAngleInstantly(0f);
		SetScaleInstantly(Vector2.One);
	}

	public void SetDefaultTargets()
	{
		base.ZIndex = 0;
		IReadOnlyList<NHandCardHolder> activeHolders = _hand.ActiveHolders;
		int num = activeHolders.IndexOf(this);
		int count = activeHolders.Count;
		if (num >= 0)
		{
			SetTargetPosition(HandPosHelper.GetPosition(count, num));
			SetTargetAngle(HandPosHelper.GetAngle(count, num));
			SetTargetScale(HandPosHelper.GetScale(count));
		}
	}

	public void Flash()
	{
		if (GodotObject.IsInstanceValid(_flash))
		{
			_flash.Scale = Vector2.One;
			_flash.Modulate = NCardHighlight.playableColor;
			if (ShouldGlowGold)
			{
				_flash.Modulate = NCardHighlight.gold;
			}
			else if (ShouldGlowRed)
			{
				_flash.Modulate = NCardHighlight.red;
			}
			_flashTween?.Kill();
			_flashTween = CreateTween();
			_flashTween.TweenProperty(_flash, "modulate:a", 0.6, 0.15);
			_flashTween.TweenProperty(_flash, "modulate:a", 0, 0.3);
		}
	}

	private void SubscribeToEvents(CardModel? card)
	{
		if (card != null && IsInsideTree())
		{
			card.Upgraded += Flash;
			card.KeywordsChanged += Flash;
			card.ReplayCountChanged += Flash;
			card.AfflictionChanged += Flash;
			card.EnergyCostChanged += Flash;
			card.StarCostChanged += Flash;
		}
	}

	private void UnsubscribeFromEvents(CardModel? card)
	{
		if (card != null)
		{
			card.Upgraded -= Flash;
			card.KeywordsChanged -= Flash;
			card.ReplayCountChanged -= Flash;
			card.AfflictionChanged -= Flash;
			card.EnergyCostChanged -= Flash;
			card.StarCostChanged -= Flash;
		}
	}

	private void OnModelChanged(CardModel? oldModel)
	{
		UnsubscribeFromEvents(oldModel);
		SubscribeToEvents(base.CardNode?.Model);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(22);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Object, "hand", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Clear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMousePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoCardHoverEffects, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isHovered", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetIndexLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "i", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTargetAngle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "angle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTargetPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTargetScale, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "scale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetAngleInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "setAngle", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetScaleInstantly, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "setScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.StopAnimations, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BeginDrag, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelDrag, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetDefaultTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Flash, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NHandCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<NPlayerHand>(in args[1])));
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
		if (method == MethodName.Clear && args.Count == 0)
		{
			Clear();
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
		if (method == MethodName.OnMousePressed && args.Count == 1)
		{
			OnMousePressed(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseReleased && args.Count == 1)
		{
			OnMouseReleased(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoCardHoverEffects && args.Count == 1)
		{
			DoCardHoverEffects(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetIndexLabel && args.Count == 1)
		{
			SetIndexLabel(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTargetAngle && args.Count == 1)
		{
			SetTargetAngle(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTargetPosition && args.Count == 1)
		{
			SetTargetPosition(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTargetScale && args.Count == 1)
		{
			SetTargetScale(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAngleInstantly && args.Count == 1)
		{
			SetAngleInstantly(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScaleInstantly && args.Count == 1)
		{
			SetScaleInstantly(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StopAnimations && args.Count == 0)
		{
			StopAnimations();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCard && args.Count == 1)
		{
			SetCard(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCard && args.Count == 0)
		{
			UpdateCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BeginDrag && args.Count == 0)
		{
			BeginDrag();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelDrag && args.Count == 0)
		{
			CancelDrag();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDefaultTargets && args.Count == 0)
		{
			SetDefaultTargets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Flash && args.Count == 0)
		{
			Flash();
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
			ret = VariantUtils.CreateFrom<NHandCardHolder>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<NPlayerHand>(in args[1])));
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Clear)
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
		if (method == MethodName.OnMousePressed)
		{
			return true;
		}
		if (method == MethodName.OnMouseReleased)
		{
			return true;
		}
		if (method == MethodName.DoCardHoverEffects)
		{
			return true;
		}
		if (method == MethodName.SetIndexLabel)
		{
			return true;
		}
		if (method == MethodName.SetTargetAngle)
		{
			return true;
		}
		if (method == MethodName.SetTargetPosition)
		{
			return true;
		}
		if (method == MethodName.SetTargetScale)
		{
			return true;
		}
		if (method == MethodName.SetAngleInstantly)
		{
			return true;
		}
		if (method == MethodName.SetScaleInstantly)
		{
			return true;
		}
		if (method == MethodName.StopAnimations)
		{
			return true;
		}
		if (method == MethodName.SetCard)
		{
			return true;
		}
		if (method == MethodName.UpdateCard)
		{
			return true;
		}
		if (method == MethodName.BeginDrag)
		{
			return true;
		}
		if (method == MethodName.CancelDrag)
		{
			return true;
		}
		if (method == MethodName.SetDefaultTargets)
		{
			return true;
		}
		if (method == MethodName.Flash)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InSelectMode)
		{
			InSelectMode = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._flash)
		{
			_flash = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._flashTween)
		{
			_flashTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._handIndexLabel)
		{
			_handIndexLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			_targetPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._targetAngle)
		{
			_targetAngle = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			_targetScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hand)
		{
			_hand = VariantUtils.ConvertTo<NPlayerHand>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.InSelectMode)
		{
			from = InSelectMode;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.TargetPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(TargetPosition);
			return true;
		}
		if (name == PropertyName.TargetAngle)
		{
			value = VariantUtils.CreateFrom<float>(TargetAngle);
			return true;
		}
		if (name == PropertyName.ShouldGlowGold)
		{
			from = ShouldGlowGold;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.ShouldGlowRed)
		{
			from = ShouldGlowRed;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._flash)
		{
			value = VariantUtils.CreateFrom(in _flash);
			return true;
		}
		if (name == PropertyName._flashTween)
		{
			value = VariantUtils.CreateFrom(in _flashTween);
			return true;
		}
		if (name == PropertyName._handIndexLabel)
		{
			value = VariantUtils.CreateFrom(in _handIndexLabel);
			return true;
		}
		if (name == PropertyName._targetPosition)
		{
			value = VariantUtils.CreateFrom(in _targetPosition);
			return true;
		}
		if (name == PropertyName._targetAngle)
		{
			value = VariantUtils.CreateFrom(in _targetAngle);
			return true;
		}
		if (name == PropertyName._targetScale)
		{
			value = VariantUtils.CreateFrom(in _targetScale);
			return true;
		}
		if (name == PropertyName._hand)
		{
			value = VariantUtils.CreateFrom(in _hand);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flash, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._flashTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._handIndexLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetAngle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._targetScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hand, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InSelectMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.TargetPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.TargetAngle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldGlowGold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldGlowRed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.InSelectMode, Variant.From<bool>(InSelectMode));
		info.AddProperty(PropertyName._flash, Variant.From(in _flash));
		info.AddProperty(PropertyName._flashTween, Variant.From(in _flashTween));
		info.AddProperty(PropertyName._handIndexLabel, Variant.From(in _handIndexLabel));
		info.AddProperty(PropertyName._targetPosition, Variant.From(in _targetPosition));
		info.AddProperty(PropertyName._targetAngle, Variant.From(in _targetAngle));
		info.AddProperty(PropertyName._targetScale, Variant.From(in _targetScale));
		info.AddProperty(PropertyName._hand, Variant.From(in _hand));
		info.AddSignalEventDelegate(SignalName.HolderFocused, backing_HolderFocused);
		info.AddSignalEventDelegate(SignalName.HolderUnfocused, backing_HolderUnfocused);
		info.AddSignalEventDelegate(SignalName.HolderMouseClicked, backing_HolderMouseClicked);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InSelectMode, out var value))
		{
			InSelectMode = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._flash, out var value2))
		{
			_flash = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._flashTween, out var value3))
		{
			_flashTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._handIndexLabel, out var value4))
		{
			_handIndexLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._targetPosition, out var value5))
		{
			_targetPosition = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._targetAngle, out var value6))
		{
			_targetAngle = value6.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetScale, out var value7))
		{
			_targetScale = value7.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hand, out var value8))
		{
			_hand = value8.As<NPlayerHand>();
		}
		if (info.TryGetSignalEventDelegate<HolderFocusedEventHandler>(SignalName.HolderFocused, out var value9))
		{
			backing_HolderFocused = value9;
		}
		if (info.TryGetSignalEventDelegate<HolderUnfocusedEventHandler>(SignalName.HolderUnfocused, out var value10))
		{
			backing_HolderUnfocused = value10;
		}
		if (info.TryGetSignalEventDelegate<HolderMouseClickedEventHandler>(SignalName.HolderMouseClicked, out var value11))
		{
			backing_HolderMouseClicked = value11;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(SignalName.HolderFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.HolderUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.HolderMouseClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHolderFocused(NHandCardHolder cardHolder)
	{
		EmitSignal(SignalName.HolderFocused, cardHolder);
	}

	protected void EmitSignalHolderUnfocused(NHandCardHolder cardHolder)
	{
		EmitSignal(SignalName.HolderUnfocused, cardHolder);
	}

	protected void EmitSignalHolderMouseClicked(NCardHolder cardHolder)
	{
		EmitSignal(SignalName.HolderMouseClicked, cardHolder);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.HolderFocused && args.Count == 1)
		{
			backing_HolderFocused?.Invoke(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
		}
		else if (signal == SignalName.HolderUnfocused && args.Count == 1)
		{
			backing_HolderUnfocused?.Invoke(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]));
		}
		else if (signal == SignalName.HolderMouseClicked && args.Count == 1)
		{
			backing_HolderMouseClicked?.Invoke(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.HolderFocused)
		{
			return true;
		}
		if (signal == SignalName.HolderUnfocused)
		{
			return true;
		}
		if (signal == SignalName.HolderMouseClicked)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
