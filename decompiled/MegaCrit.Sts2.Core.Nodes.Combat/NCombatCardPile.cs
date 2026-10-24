using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

/// <summary>
/// Node script for DrawPile, DiscardPile, and ExhaustPile.
/// Don't extend to more piles! Make bespoke scripts.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Combat/NCombatCardPile.cs")]
public abstract class NCombatCardPile : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'ConnectSignals' method.
		/// </summary>
		public new static readonly StringName ConnectSignals = "ConnectSignals";

		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'SetAnimInOutPositions' method.
		/// </summary>
		public static readonly StringName SetAnimInOutPositions = "SetAnimInOutPositions";

		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";

		/// <summary>
		/// Cached name for the 'OnPress' method.
		/// </summary>
		public new static readonly StringName OnPress = "OnPress";

		/// <summary>
		/// Cached name for the 'AddCard' method.
		/// </summary>
		public static readonly StringName AddCard = "AddCard";

		/// <summary>
		/// Cached name for the 'RemoveCard' method.
		/// </summary>
		public static readonly StringName RemoveCard = "RemoveCard";

		/// <summary>
		/// Cached name for the 'AnimIn' method.
		/// </summary>
		public static readonly StringName AnimIn = "AnimIn";

		/// <summary>
		/// Cached name for the 'AnimOut' method.
		/// </summary>
		public static readonly StringName AnimOut = "AnimOut";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Pile' property.
		/// </summary>
		public static readonly StringName Pile = "Pile";

		/// <summary>
		/// Cached name for the '_countLabel' field.
		/// </summary>
		public static readonly StringName _countLabel = "_countLabel";

		/// <summary>
		/// Cached name for the '_icon' field.
		/// </summary>
		public static readonly StringName _icon = "_icon";

		/// <summary>
		/// Cached name for the '_bumpTween' field.
		/// </summary>
		public static readonly StringName _bumpTween = "_bumpTween";

		/// <summary>
		/// Cached name for the '_currentCount' field.
		/// </summary>
		public static readonly StringName _currentCount = "_currentCount";

		/// <summary>
		/// Cached name for the '_positionTween' field.
		/// </summary>
		public static readonly StringName _positionTween = "_positionTween";

		/// <summary>
		/// Cached name for the '_showPosition' field.
		/// </summary>
		public static readonly StringName _showPosition = "_showPosition";

		/// <summary>
		/// Cached name for the '_hidePosition' field.
		/// </summary>
		public static readonly StringName _hidePosition = "_hidePosition";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private CardPile? _pile;

	private Player? _localPlayer;

	private MegaLabel _countLabel;

	private Control _icon;

	protected LocString _emptyPileMessage;

	private Tween? _bumpTween;

	private int _currentCount;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.25f;

	private const double _unhoverAnimDur = 0.5;

	private const double _pressDownDur = 0.25;

	private static readonly Color _downColor = Colors.DarkGray;

	private Tween? _positionTween;

	private const double _animDuration = 0.5;

	protected Vector2 _showPosition = new Vector2(100f, 828f);

	protected Vector2 _hidePosition = new Vector2(-160f, 860f);

	protected abstract PileType Pile { get; }

	public override void _Ready()
	{
		if (GetType() != typeof(NCombatCardPile))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected override void ConnectSignals()
	{
		base.ConnectSignals();
		_icon = GetNode<Control>("Icon");
		_countLabel = GetNode<MegaLabel>("CountContainer/Count");
		SetAnimInOutPositions();
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (_pile != null)
		{
			_pile.CardAddFinished -= AddCard;
			_pile.CardAddFinished += AddCard;
			_pile.CardRemoveFinished -= RemoveCard;
			_pile.CardRemoveFinished += RemoveCard;
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_pile != null)
		{
			_pile.CardAddFinished -= AddCard;
			_pile.CardRemoveFinished -= RemoveCard;
		}
		_positionTween?.Kill();
		_bumpTween?.Kill();
	}

	public virtual void Initialize(Player player)
	{
		_localPlayer = player;
		_pile = Pile.GetPile(_localPlayer);
		_pile.CardAddFinished += AddCard;
		_pile.CardRemoveFinished += RemoveCard;
		_currentCount = _pile.Cards.Count;
		_countLabel.SetTextAutoSize(_currentCount.ToString());
	}

	protected virtual void SetAnimInOutPositions()
	{
	}

	protected override void OnRelease()
	{
		_bumpTween?.Kill();
		_bumpTween = CreateTween();
		_bumpTween.TweenProperty(_icon, "scale", base.IsFocused ? _hoverScale : Vector2.One, 0.05);
		_bumpTween.TweenProperty(_icon, "modulate", Colors.White, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		if (_pile == null || _localPlayer == null || !CombatManager.Instance.IsInProgress)
		{
			return;
		}
		if (NTargetManager.Instance.IsInSelection)
		{
			NTargetManager.Instance.CancelTargeting();
		}
		if (_pile.IsEmpty)
		{
			NCapstoneContainer? instance = NCapstoneContainer.Instance;
			if (instance != null && instance.InUse)
			{
				NCapstoneContainer.Instance.Close();
			}
			NThoughtBubbleVfx child = NThoughtBubbleVfx.Create(_emptyPileMessage.GetFormattedText(), _localPlayer.Creature, 2.0);
			NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
		}
		else if (NCapstoneContainer.Instance?.CurrentCapstoneScreen is NCardPileScreen nCardPileScreen && nCardPileScreen.Pile == _pile)
		{
			NCapstoneContainer.Instance.Close();
		}
		else
		{
			NCardPileScreen.ShowScreen(_pile, Hotkeys);
		}
	}

	protected override void OnFocus()
	{
		if (_pile != null)
		{
			LocString locString;
			LocString description;
			switch (_pile.Type)
			{
			case PileType.Draw:
				locString = new LocString("static_hover_tips", "DRAW_PILE.title");
				description = new LocString("static_hover_tips", "DRAW_PILE.description");
				break;
			case PileType.Discard:
				locString = new LocString("static_hover_tips", "DISCARD_PILE.title");
				description = new LocString("static_hover_tips", "DISCARD_PILE.description");
				break;
			case PileType.Exhaust:
				locString = new LocString("static_hover_tips", "EXHAUST_PILE.title");
				description = new LocString("static_hover_tips", "EXHAUST_PILE.description");
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			string text = Hotkeys.FirstOrDefault();
			Key? key = null;
			if (text != null)
			{
				key = NInputManager.Instance.GetShortcutKey(text);
			}
			if (key.HasValue)
			{
				locString.Add("Hotkey", key.Value.ToString());
			}
			else
			{
				locString.Add("Hotkey", "None");
			}
			HoverTip hoverTip = new HoverTip(locString, description);
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTip);
			if (nHoverTipSet != null)
			{
				NHoverTipSet nHoverTipSet2 = nHoverTipSet;
				nHoverTipSet2.GlobalPosition = _pile.Type switch
				{
					PileType.Draw => base.GlobalPosition + new Vector2(14f, -375f), 
					PileType.Discard => base.GlobalPosition + new Vector2(-320f, -370f), 
					PileType.Exhaust => base.GlobalPosition + new Vector2(-320f, -125f), 
					_ => nHoverTipSet.GlobalPosition, 
				};
			}
			_bumpTween?.Kill();
			_bumpTween = CreateTween();
			_bumpTween.TweenProperty(_icon, "scale", _hoverScale, 0.05);
		}
	}

	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(this);
		_bumpTween?.Kill();
		_bumpTween = CreateTween().SetParallel();
		_bumpTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_bumpTween.TweenProperty(_icon, "modulate", Colors.White, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	protected override void OnPress()
	{
		_bumpTween?.Kill();
		_bumpTween = CreateTween().SetParallel();
		_bumpTween.TweenProperty(_icon, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_bumpTween.TweenProperty(_icon, "modulate", _downColor, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	protected virtual void AddCard()
	{
		if (_pile != null)
		{
			_currentCount = Math.Min(_currentCount + 1, _pile.Cards.Count);
			_countLabel.SetTextAutoSize(_currentCount.ToString());
			_countLabel.PivotOffset = _countLabel.Size * 0.5f;
			_bumpTween?.Kill();
			_bumpTween = CreateTween().SetParallel();
			_icon.Scale = _hoverScale;
			_bumpTween.TweenProperty(_icon, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_countLabel.Scale = _hoverScale;
			_bumpTween.TweenProperty(_countLabel, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
	}

	private void RemoveCard()
	{
		if (_pile != null)
		{
			_currentCount = Math.Max(_currentCount - 1, _pile.Cards.Count);
			_countLabel.SetTextAutoSize(_currentCount.ToString());
			_countLabel.PivotOffset = _countLabel.Size * 0.5f;
		}
	}

	public virtual void AnimIn()
	{
		base.Position = _hidePosition;
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", _showPosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void AnimOut()
	{
		base.Position = _showPosition;
		_positionTween?.Kill();
		_positionTween = CreateTween();
		_positionTween.TweenProperty(this, "position", _hidePosition, 0.5).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Back);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAnimInOutPositions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
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
		if (method == MethodName.SetAnimInOutPositions && args.Count == 0)
		{
			SetAnimInOutPositions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddCard && args.Count == 0)
		{
			AddCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveCard && args.Count == 0)
		{
			RemoveCard();
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.ConnectSignals)
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
		if (method == MethodName.SetAnimInOutPositions)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
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
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.AddCard)
		{
			return true;
		}
		if (method == MethodName.RemoveCard)
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._countLabel)
		{
			_countLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bumpTween)
		{
			_bumpTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._currentCount)
		{
			_currentCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			_positionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._showPosition)
		{
			_showPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._hidePosition)
		{
			_hidePosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Pile)
		{
			value = VariantUtils.CreateFrom<PileType>(Pile);
			return true;
		}
		if (name == PropertyName._countLabel)
		{
			value = VariantUtils.CreateFrom(in _countLabel);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._bumpTween)
		{
			value = VariantUtils.CreateFrom(in _bumpTween);
			return true;
		}
		if (name == PropertyName._currentCount)
		{
			value = VariantUtils.CreateFrom(in _currentCount);
			return true;
		}
		if (name == PropertyName._positionTween)
		{
			value = VariantUtils.CreateFrom(in _positionTween);
			return true;
		}
		if (name == PropertyName._showPosition)
		{
			value = VariantUtils.CreateFrom(in _showPosition);
			return true;
		}
		if (name == PropertyName._hidePosition)
		{
			value = VariantUtils.CreateFrom(in _hidePosition);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Pile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._countLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bumpTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._positionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._showPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hidePosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._countLabel, Variant.From(in _countLabel));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._bumpTween, Variant.From(in _bumpTween));
		info.AddProperty(PropertyName._currentCount, Variant.From(in _currentCount));
		info.AddProperty(PropertyName._positionTween, Variant.From(in _positionTween));
		info.AddProperty(PropertyName._showPosition, Variant.From(in _showPosition));
		info.AddProperty(PropertyName._hidePosition, Variant.From(in _hidePosition));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._countLabel, out var value))
		{
			_countLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bumpTween, out var value3))
		{
			_bumpTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._currentCount, out var value4))
		{
			_currentCount = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._positionTween, out var value5))
		{
			_positionTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._showPosition, out var value6))
		{
			_showPosition = value6.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._hidePosition, out var value7))
		{
			_hidePosition = value7.As<Vector2>();
		}
	}
}
