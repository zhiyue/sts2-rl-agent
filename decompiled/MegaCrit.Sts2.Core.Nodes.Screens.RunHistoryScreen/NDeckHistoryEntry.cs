using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NDeckHistoryEntry.cs")]
public class NDeckHistoryEntry : NButton
{
	[Signal]
	public delegate void ClickedEventHandler(NDeckHistoryEntry entry);

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Reload = "Reload";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnRelease = "OnRelease";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _cardImage = "_cardImage";

		public static readonly StringName _enchantmentImage = "_enchantmentImage";

		public static readonly StringName _labelContainer = "_labelContainer";

		public static readonly StringName _scaleTween = "_scaleTween";

		public static readonly StringName _amount = "_amount";
	}

	public new class SignalName : NButton.SignalName
	{
		public static readonly StringName Clicked = "Clicked";
	}

	private MegaLabel _titleLabel;

	private NTinyCard _cardImage;

	private TextureRect _enchantmentImage;

	private MarginContainer _labelContainer;

	private Tween? _scaleTween;

	private int _amount;

	private ClickedEventHandler backing_Clicked;

	private static string ScenePath => SceneHelper.GetScenePath("screens/run_history_screen/deck_history_entry");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public IEnumerable<int> FloorsAddedToDeck { get; private set; }

	public CardModel Card { get; private set; }

	public event ClickedEventHandler Clicked
	{
		add
		{
			backing_Clicked = (ClickedEventHandler)Delegate.Combine(backing_Clicked, value);
		}
		remove
		{
			backing_Clicked = (ClickedEventHandler)Delegate.Remove(backing_Clicked, value);
		}
	}

	public override void _Ready()
	{
		ConnectSignals();
		_titleLabel = GetNode<MegaLabel>("%Label");
		_labelContainer = _titleLabel.GetParent<MarginContainer>();
		_cardImage = GetNode<NTinyCard>("%Card");
		_enchantmentImage = GetNode<TextureRect>("%Enchantment");
		_cardImage.PivotOffset = _cardImage.Size * 0.5f;
		Reload();
	}

	public static NDeckHistoryEntry Create(CardModel card, int amount)
	{
		return Create(card, amount, Array.Empty<int>());
	}

	public static NDeckHistoryEntry Create(CardModel card, int amount, IEnumerable<int> floorsAdded)
	{
		NDeckHistoryEntry nDeckHistoryEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NDeckHistoryEntry>(PackedScene.GenEditState.Disabled);
		nDeckHistoryEntry.Card = card;
		nDeckHistoryEntry._amount = amount;
		nDeckHistoryEntry.FloorsAddedToDeck = floorsAdded;
		return nDeckHistoryEntry;
	}

	private void Reload()
	{
		_titleLabel.SetTextAutoSize(Card.Title);
		bool flag = Card.CurrentUpgradeLevel >= 1;
		bool flag2 = Card.Enchantment != null;
		string text = Card.Title;
		if (_amount > 1)
		{
			text = $"{_amount}x {text}";
		}
		_titleLabel.SetTextAutoSize(text);
		if (flag2)
		{
			_titleLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, StsColors.purple);
		}
		else if (flag)
		{
			_titleLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, StsColors.green);
		}
		_cardImage.SetCard(Card);
		if (Card.Enchantment != null)
		{
			_enchantmentImage.Texture = Card.Enchantment.Icon;
		}
		_enchantmentImage.Visible = Card.Enchantment != null;
		base.Size = new Vector2(_cardImage.Size.X + _titleLabel.Size.X + 10f, base.Size.Y);
	}

	protected override void OnFocus()
	{
		_scaleTween?.FastForwardToCompletion();
		_scaleTween = CreateTween().SetParallel();
		_scaleTween.TweenProperty(_cardImage, "scale", Vector2.One * 1.5f, 0.05);
		_scaleTween.TweenProperty(_labelContainer, "position:x", _labelContainer.Position.X + 8f, 0.05);
	}

	protected override void OnUnfocus()
	{
		_scaleTween?.FastForwardToCompletion();
		_scaleTween = CreateTween().SetParallel();
		_scaleTween.TweenProperty(_cardImage, "scale", Vector2.One, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
		_scaleTween.TweenProperty(_labelContainer, "position:x", _labelContainer.Position.X - 8f, 0.5).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
	}

	protected override void OnRelease()
	{
		EmitSignal(SignalName.Clicked, this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
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
		if (method == MethodName.Reload)
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._cardImage)
		{
			_cardImage = VariantUtils.ConvertTo<NTinyCard>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentImage)
		{
			_enchantmentImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			_labelContainer = VariantUtils.ConvertTo<MarginContainer>(in value);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			_scaleTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._amount)
		{
			_amount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
		if (name == PropertyName._cardImage)
		{
			value = VariantUtils.CreateFrom(in _cardImage);
			return true;
		}
		if (name == PropertyName._enchantmentImage)
		{
			value = VariantUtils.CreateFrom(in _enchantmentImage);
			return true;
		}
		if (name == PropertyName._labelContainer)
		{
			value = VariantUtils.CreateFrom(in _labelContainer);
			return true;
		}
		if (name == PropertyName._scaleTween)
		{
			value = VariantUtils.CreateFrom(in _scaleTween);
			return true;
		}
		if (name == PropertyName._amount)
		{
			value = VariantUtils.CreateFrom(in _amount);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._labelContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._scaleTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._amount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._cardImage, Variant.From(in _cardImage));
		info.AddProperty(PropertyName._enchantmentImage, Variant.From(in _enchantmentImage));
		info.AddProperty(PropertyName._labelContainer, Variant.From(in _labelContainer));
		info.AddProperty(PropertyName._scaleTween, Variant.From(in _scaleTween));
		info.AddProperty(PropertyName._amount, Variant.From(in _amount));
		info.AddSignalEventDelegate(SignalName.Clicked, backing_Clicked);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._titleLabel, out var value))
		{
			_titleLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._cardImage, out var value2))
		{
			_cardImage = value2.As<NTinyCard>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentImage, out var value3))
		{
			_enchantmentImage = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._labelContainer, out var value4))
		{
			_labelContainer = value4.As<MarginContainer>();
		}
		if (info.TryGetProperty(PropertyName._scaleTween, out var value5))
		{
			_scaleTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._amount, out var value6))
		{
			_amount = value6.As<int>();
		}
		if (info.TryGetSignalEventDelegate<ClickedEventHandler>(SignalName.Clicked, out var value7))
		{
			backing_Clicked = value7;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Clicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "entry", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalClicked(NDeckHistoryEntry entry)
	{
		EmitSignal(SignalName.Clicked, entry);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Clicked && args.Count == 1)
		{
			backing_Clicked?.Invoke(VariantUtils.ConvertTo<NDeckHistoryEntry>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Clicked)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
