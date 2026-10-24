using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace MegaCrit.Sts2.Core.Nodes.Cards.Holders;

/// <summary>
/// CardHolder is meant to control the animation visuals for cards when they are presented in the PlayerHand,
/// grid screens, card reward screens and so on. Examples include hovering/un-hovering and fancy fanning logic in hand.
/// Clicked/buttonReleased behavior is handled by its parent: CardHolderContainer (ie PlayerHand).
/// </summary>
[ScriptPath("res://src/Core/Nodes/Cards/Holders/NCardHolder.cs")]
public abstract class NCardHolder : Control
{
	[Signal]
	public delegate void PressedEventHandler(NCardHolder cardHolder);

	[Signal]
	public delegate void AltPressedEventHandler(NCardHolder cardHolder);

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
		/// Cached name for the 'SetClickable' method.
		/// </summary>
		public static readonly StringName SetClickable = "SetClickable";

		/// <summary>
		/// Cached name for the 'ConnectSignals' method.
		/// </summary>
		public static readonly StringName ConnectSignals = "ConnectSignals";

		/// <summary>
		/// Cached name for the '_GuiInput' method.
		/// </summary>
		public new static readonly StringName _GuiInput = "_GuiInput";

		/// <summary>
		/// Cached name for the 'EmitPressed' method.
		/// </summary>
		public static readonly StringName EmitPressed = "EmitPressed";

		/// <summary>
		/// Cached name for the 'EmitAltPressed' method.
		/// </summary>
		public static readonly StringName EmitAltPressed = "EmitAltPressed";

		/// <summary>
		/// Cached name for the 'SetCard' method.
		/// </summary>
		public static readonly StringName SetCard = "SetCard";

		/// <summary>
		/// Cached name for the 'OnCardReassigned' method.
		/// </summary>
		public static readonly StringName OnCardReassigned = "OnCardReassigned";

		/// <summary>
		/// Cached name for the 'OnMousePressed' method.
		/// </summary>
		public static readonly StringName OnMousePressed = "OnMousePressed";

		/// <summary>
		/// Cached name for the 'OnMouseReleased' method.
		/// </summary>
		public static readonly StringName OnMouseReleased = "OnMouseReleased";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'CreateHoverTips' method.
		/// </summary>
		public static readonly StringName CreateHoverTips = "CreateHoverTips";

		/// <summary>
		/// Cached name for the 'ClearHoverTips' method.
		/// </summary>
		public static readonly StringName ClearHoverTips = "ClearHoverTips";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'RefreshFocusState' method.
		/// </summary>
		public static readonly StringName RefreshFocusState = "RefreshFocusState";

		/// <summary>
		/// Cached name for the 'DoCardHoverEffects' method.
		/// </summary>
		public static readonly StringName DoCardHoverEffects = "DoCardHoverEffects";

		/// <summary>
		/// Cached name for the 'OnChildExitingTree' method.
		/// </summary>
		public static readonly StringName OnChildExitingTree = "OnChildExitingTree";

		/// <summary>
		/// Cached name for the 'Clear' method.
		/// </summary>
		public static readonly StringName Clear = "Clear";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'HoverScale' property.
		/// </summary>
		public static readonly StringName HoverScale = "HoverScale";

		/// <summary>
		/// Cached name for the 'SmallScale' property.
		/// </summary>
		public static readonly StringName SmallScale = "SmallScale";

		/// <summary>
		/// Cached name for the 'Hitbox' property.
		/// </summary>
		public static readonly StringName Hitbox = "Hitbox";

		/// <summary>
		/// Cached name for the 'CardNode' property.
		/// </summary>
		public static readonly StringName CardNode = "CardNode";

		/// <summary>
		/// Cached name for the 'IsShowingUpgradedCard' property.
		/// </summary>
		public static readonly StringName IsShowingUpgradedCard = "IsShowingUpgradedCard";

		/// <summary>
		/// Cached name for the 'CanBeFocused' property.
		/// </summary>
		public static readonly StringName CanBeFocused = "CanBeFocused";

		/// <summary>
		/// Cached name for the '_hitbox' field.
		/// </summary>
		public static readonly StringName _hitbox = "_hitbox";

		/// <summary>
		/// Cached name for the '_isHovered' field.
		/// </summary>
		public static readonly StringName _isHovered = "_isHovered";

		/// <summary>
		/// Cached name for the '_isFocused' field.
		/// </summary>
		public static readonly StringName _isFocused = "_isFocused";

		/// <summary>
		/// Cached name for the '_hoverTween' field.
		/// </summary>
		public static readonly StringName _hoverTween = "_hoverTween";

		/// <summary>
		/// Cached name for the '_currentPressedAction' field.
		/// </summary>
		public static readonly StringName _currentPressedAction = "_currentPressedAction";

		/// <summary>
		/// Cached name for the '_isClickable' field.
		/// </summary>
		public static readonly StringName _isClickable = "_isClickable";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
		/// <summary>
		/// Cached name for the 'Pressed' signal.
		/// </summary>
		public static readonly StringName Pressed = "Pressed";

		/// <summary>
		/// Cached name for the 'AltPressed' signal.
		/// </summary>
		public static readonly StringName AltPressed = "AltPressed";
	}

	public static readonly Vector2 smallScale = Vector2.One * 0.8f;

	protected NClickableControl _hitbox;

	protected bool _isHovered;

	protected bool _isFocused;

	protected Tween? _hoverTween;

	private InputEventMouseButton? _currentPressedAction;

	protected bool _isClickable = true;

	private PressedEventHandler backing_Pressed;

	private AltPressedEventHandler backing_AltPressed;

	protected virtual Vector2 HoverScale => Vector2.One;

	public virtual Vector2 SmallScale => smallScale;

	public NClickableControl Hitbox => _hitbox;

	public NCard? CardNode { get; protected set; }

	/// <summary>
	/// The CardModel that this holder's CardNode is displaying.
	/// Null if the holder is empty.
	/// </summary>
	public virtual CardModel? CardModel => CardNode?.Model;

	public virtual bool IsShowingUpgradedCard => CardModel?.IsUpgraded ?? false;

	protected bool CanBeFocused => _isHovered;

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Cards.Holders.NCardHolder.PressedEventHandler" />
	public event PressedEventHandler Pressed
	{
		add
		{
			backing_Pressed = (PressedEventHandler)Delegate.Combine(backing_Pressed, value);
		}
		remove
		{
			backing_Pressed = (PressedEventHandler)Delegate.Remove(backing_Pressed, value);
		}
	}

	/// <inheritdoc cref="T:MegaCrit.Sts2.Core.Nodes.Cards.Holders.NCardHolder.AltPressedEventHandler" />
	public event AltPressedEventHandler AltPressed
	{
		add
		{
			backing_AltPressed = (AltPressedEventHandler)Delegate.Combine(backing_AltPressed, value);
		}
		remove
		{
			backing_AltPressed = (AltPressedEventHandler)Delegate.Remove(backing_AltPressed, value);
		}
	}

	public override void _Ready()
	{
		if (GetType() != typeof(NCardHolder))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	/// <summary>
	/// Set whether the card holder may be selected (e.g. clicked by mouse or confirmed by controller).
	/// Different than calling _hitbox.Disable, as the card holder can still be focused.
	/// </summary>
	public void SetClickable(bool isClickable)
	{
		_isClickable = isClickable;
	}

	protected void ConnectSignals()
	{
		if (CardNode != null)
		{
			CardNode.Position = Vector2.Zero;
		}
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
		Connect(Control.SignalName.FocusExited, Callable.From(OnUnfocus));
		_hitbox = GetNode<NClickableControl>("%Hitbox");
		_hitbox.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(delegate
		{
			OnFocus();
		}));
		_hitbox.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(delegate
		{
			OnUnfocus();
		}));
		_hitbox.Connect(NClickableControl.SignalName.MousePressed, Callable.From<InputEvent>(OnMousePressed));
		_hitbox.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(OnMouseReleased));
		Connect(Node.SignalName.ChildExitingTree, Callable.From<Node>(OnChildExitingTree));
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		base._GuiInput(inputEvent);
		if (_isClickable && CardNode != null)
		{
			if (inputEvent.IsActionPressed(MegaInput.select))
			{
				SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
				CallDeferred(MethodName.EmitPressed);
			}
			else if (inputEvent.IsActionPressed(MegaInput.accept))
			{
				SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
				CallDeferred(MethodName.EmitAltPressed);
			}
		}
	}

	private void EmitPressed()
	{
		EmitSignal(SignalName.Pressed, this);
	}

	private void EmitAltPressed()
	{
		EmitSignal(SignalName.AltPressed, this);
	}

	protected virtual void SetCard(NCard node)
	{
		if (CardNode != null)
		{
			throw new InvalidOperationException("Cannot set a card node on a holder that already has one.");
		}
		CardNode = node;
		if (CardNode.GetParent() == null)
		{
			this.AddChildSafely(node);
		}
		else
		{
			node.Reparent(this);
		}
	}

	/// <summary>
	/// This is used for reducing the number of unnecessary costly instancing and godot tree operations.
	/// </summary>
	public void ReassignToCard(CardModel cardModel, PileType pileType, Creature? target, ModelVisibility visibility)
	{
		CardNode.Visibility = visibility;
		CardNode.Model = cardModel;
		CardNode.SetPreviewTarget(target);
		CardNode.UpdateVisuals(pileType, CardPreviewMode.Normal);
		OnCardReassigned();
	}

	protected virtual void OnCardReassigned()
	{
	}

	protected virtual void OnMousePressed(InputEvent inputEvent)
	{
		if (_currentPressedAction == null && inputEvent is InputEventMouseButton inputEventMouseButton && _isClickable)
		{
			MouseButton buttonIndex = inputEventMouseButton.ButtonIndex;
			if (((ulong)(buttonIndex - 1) <= 1uL) ? true : false)
			{
				SfxCmd.Play("event:/sfx/ui/clicks/ui_click");
			}
			_currentPressedAction = inputEventMouseButton;
		}
	}

	protected virtual void OnMouseReleased(InputEvent inputEvent)
	{
		if (CardNode == null || !_isHovered || _currentPressedAction == null)
		{
			return;
		}
		if (inputEvent is InputEventMouseButton inputEventMouseButton && _isClickable)
		{
			if (inputEventMouseButton.ButtonIndex != _currentPressedAction.ButtonIndex)
			{
				return;
			}
			if (inputEventMouseButton.ButtonIndex == MouseButton.Left)
			{
				EmitSignal(SignalName.Pressed, this);
			}
			else
			{
				EmitSignal(SignalName.AltPressed, this);
			}
		}
		_currentPressedAction = null;
	}

	protected virtual void OnFocus()
	{
		_isHovered = true;
		RefreshFocusState();
	}

	protected virtual void CreateHoverTips()
	{
		if (CardNode != null)
		{
			NHoverTipSet.CreateAndShow(this, CardNode.Model.HoverTips)?.SetAlignmentForCardHolder(this);
		}
	}

	protected void ClearHoverTips()
	{
		NHoverTipSet.Remove(this);
	}

	protected virtual void OnUnfocus()
	{
		_isHovered = false;
		_currentPressedAction = null;
		RefreshFocusState();
	}

	protected void RefreshFocusState()
	{
		if (_isFocused != CanBeFocused)
		{
			_isFocused = CanBeFocused;
			DoCardHoverEffects(_isFocused);
		}
	}

	protected virtual void DoCardHoverEffects(bool isHovered)
	{
		if (isHovered)
		{
			_hoverTween?.Kill();
			base.Scale = HoverScale;
			if (CardNode.Visibility == ModelVisibility.Visible)
			{
				CreateHoverTips();
			}
		}
		else if (!isHovered)
		{
			_hoverTween?.Kill();
			_hoverTween = CreateTween();
			_hoverTween.TweenProperty(this, "scale", SmallScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			ClearHoverTips();
		}
	}

	/// <summary>
	/// Clean up the CardHolder if its NCard is removed.
	/// We can get rid of this if we start wanting to reuse CardHolders.
	/// </summary>
	private void OnChildExitingTree(Node node)
	{
		if (node == CardNode && node.GetParent() != this)
		{
			ClearHoverTips();
			CardNode = null;
		}
	}

	public virtual void Clear()
	{
		if (CardNode != null)
		{
			if (CardNode.GetParent() == this)
			{
				this.RemoveChildSafely(CardNode);
			}
			CardNode = null;
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
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetClickable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isClickable", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._GuiInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EmitPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EmitAltPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnCardReassigned, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMousePressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMouseReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearHoverTips, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshFocusState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoCardHoverEffects, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isHovered", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnChildExitingTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Clear, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetClickable && args.Count == 1)
		{
			SetClickable(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._GuiInput && args.Count == 1)
		{
			_GuiInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EmitPressed && args.Count == 0)
		{
			EmitPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EmitAltPressed && args.Count == 0)
		{
			EmitAltPressed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCard && args.Count == 1)
		{
			SetCard(VariantUtils.ConvertTo<NCard>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCardReassigned && args.Count == 0)
		{
			OnCardReassigned();
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateHoverTips && args.Count == 0)
		{
			CreateHoverTips();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearHoverTips && args.Count == 0)
		{
			ClearHoverTips();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshFocusState && args.Count == 0)
		{
			RefreshFocusState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoCardHoverEffects && args.Count == 1)
		{
			DoCardHoverEffects(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChildExitingTree && args.Count == 1)
		{
			OnChildExitingTree(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Clear && args.Count == 0)
		{
			Clear();
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
		if (method == MethodName.SetClickable)
		{
			return true;
		}
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName._GuiInput)
		{
			return true;
		}
		if (method == MethodName.EmitPressed)
		{
			return true;
		}
		if (method == MethodName.EmitAltPressed)
		{
			return true;
		}
		if (method == MethodName.SetCard)
		{
			return true;
		}
		if (method == MethodName.OnCardReassigned)
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.CreateHoverTips)
		{
			return true;
		}
		if (method == MethodName.ClearHoverTips)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.RefreshFocusState)
		{
			return true;
		}
		if (method == MethodName.DoCardHoverEffects)
		{
			return true;
		}
		if (method == MethodName.OnChildExitingTree)
		{
			return true;
		}
		if (method == MethodName.Clear)
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
		if (name == PropertyName._hitbox)
		{
			_hitbox = VariantUtils.ConvertTo<NClickableControl>(in value);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			_isFocused = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._currentPressedAction)
		{
			_currentPressedAction = VariantUtils.ConvertTo<InputEventMouseButton>(in value);
			return true;
		}
		if (name == PropertyName._isClickable)
		{
			_isClickable = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Vector2 from;
		if (name == PropertyName.HoverScale)
		{
			from = HoverScale;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.SmallScale)
		{
			from = SmallScale;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Hitbox)
		{
			value = VariantUtils.CreateFrom<NClickableControl>(Hitbox);
			return true;
		}
		if (name == PropertyName.CardNode)
		{
			value = VariantUtils.CreateFrom<NCard>(CardNode);
			return true;
		}
		bool from2;
		if (name == PropertyName.IsShowingUpgradedCard)
		{
			from2 = IsShowingUpgradedCard;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.CanBeFocused)
		{
			from2 = CanBeFocused;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName._hitbox)
		{
			value = VariantUtils.CreateFrom(in _hitbox);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._isFocused)
		{
			value = VariantUtils.CreateFrom(in _isFocused);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._currentPressedAction)
		{
			value = VariantUtils.CreateFrom(in _currentPressedAction);
			return true;
		}
		if (name == PropertyName._isClickable)
		{
			value = VariantUtils.CreateFrom(in _isClickable);
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
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.HoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.SmallScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShowingUpgradedCard, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.CanBeFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentPressedAction, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isClickable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CardNode, Variant.From<NCard>(CardNode));
		info.AddProperty(PropertyName._hitbox, Variant.From(in _hitbox));
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._isFocused, Variant.From(in _isFocused));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._currentPressedAction, Variant.From(in _currentPressedAction));
		info.AddProperty(PropertyName._isClickable, Variant.From(in _isClickable));
		info.AddSignalEventDelegate(SignalName.Pressed, backing_Pressed);
		info.AddSignalEventDelegate(SignalName.AltPressed, backing_AltPressed);
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
		if (info.TryGetProperty(PropertyName._hitbox, out var value2))
		{
			_hitbox = value2.As<NClickableControl>();
		}
		if (info.TryGetProperty(PropertyName._isHovered, out var value3))
		{
			_isHovered = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isFocused, out var value4))
		{
			_isFocused = value4.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value5))
		{
			_hoverTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._currentPressedAction, out var value6))
		{
			_currentPressedAction = value6.As<InputEventMouseButton>();
		}
		if (info.TryGetProperty(PropertyName._isClickable, out var value7))
		{
			_isClickable = value7.As<bool>();
		}
		if (info.TryGetSignalEventDelegate<PressedEventHandler>(SignalName.Pressed, out var value8))
		{
			backing_Pressed = value8;
		}
		if (info.TryGetSignalEventDelegate<AltPressedEventHandler>(SignalName.AltPressed, out var value9))
		{
			backing_AltPressed = value9;
		}
	}

	/// <summary>
	/// Get the signal information for all the signals declared in this class.
	/// This method is used by Godot to register the available signals in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Pressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.AltPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalPressed(NCardHolder cardHolder)
	{
		EmitSignal(SignalName.Pressed, cardHolder);
	}

	protected void EmitSignalAltPressed(NCardHolder cardHolder)
	{
		EmitSignal(SignalName.AltPressed, cardHolder);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Pressed && args.Count == 1)
		{
			backing_Pressed?.Invoke(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
		}
		else if (signal == SignalName.AltPressed && args.Count == 1)
		{
			backing_AltPressed?.Invoke(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Pressed)
		{
			return true;
		}
		if (signal == SignalName.AltPressed)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
