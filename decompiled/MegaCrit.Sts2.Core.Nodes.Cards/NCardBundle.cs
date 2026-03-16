using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NCardBundle.cs")]
public class NCardBundle : Control
{
	[Signal]
	public delegate void ClickedEventHandler(NCardBundle cardHolder);

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ReAddCardNodes = "ReAddCardNodes";

		public static readonly StringName OnClicked = "OnClicked";

		public static readonly StringName OnFocused = "OnFocused";

		public static readonly StringName OnUnfocused = "OnUnfocused";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Hitbox = "Hitbox";

		public static readonly StringName _hoverScale = "_hoverScale";

		public static readonly StringName smallScale = "smallScale";

		public static readonly StringName _cardHolder = "_cardHolder";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _cardTween = "_cardTween";
	}

	public new class SignalName : Control.SignalName
	{
		public static readonly StringName Clicked = "Clicked";
	}

	private const float _cardSeparation = 45f;

	private readonly Vector2 _hoverScale = Vector2.One * 0.85f;

	public readonly Vector2 smallScale = Vector2.One * 0.8f;

	private Control _cardHolder;

	private readonly List<NCard> _cardNodes = new List<NCard>();

	private Tween? _hoverTween;

	private Tween? _cardTween;

	private ClickedEventHandler backing_Clicked;

	public NClickableControl Hitbox { get; private set; }

	public IReadOnlyList<CardModel> Bundle { get; private set; }

	public IReadOnlyList<NCard> CardNodes => _cardNodes;

	private static string ScenePath => SceneHelper.GetScenePath("/cards/card_bundle");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

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

	public static NCardBundle? Create(IReadOnlyList<CardModel> bundle)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardBundle nCardBundle = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardBundle>(PackedScene.GenEditState.Disabled);
		nCardBundle.Name = "NCardBundle";
		nCardBundle.Scale = nCardBundle.smallScale;
		nCardBundle.Bundle = bundle;
		return nCardBundle;
	}

	public override void _Ready()
	{
		Hitbox = GetNode<NButton>("%Hitbox");
		_cardHolder = GetNode<Control>("%Cards");
		Hitbox.Connect(NClickableControl.SignalName.Focused, Callable.From<NClickableControl>(OnFocused));
		Hitbox.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NClickableControl>(OnUnfocused));
		Hitbox.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(OnClicked));
		for (int i = 0; i < Bundle.Count; i++)
		{
			NCard nCard = NCard.Create(Bundle[i]);
			_cardHolder = GetNode<Control>("%Cards");
			_cardHolder.AddChildSafely(nCard);
			nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			nCard.Position += new Vector2(-1f, 1f) * 45f * ((float)i - (float)Bundle.Count / 2f);
			float num = 0.5f + (float)i / (float)(Bundle.Count - 1) * 0.5f;
			nCard.Modulate = new Color(num, num, num);
			_cardNodes.Add(nCard);
		}
	}

	public IReadOnlyList<NCard> RemoveCardNodes()
	{
		_cardTween?.Kill();
		_cardTween = CreateTween().SetParallel();
		foreach (NCard cardNode in _cardNodes)
		{
			_cardTween.TweenProperty(cardNode, "modulate", Colors.White, 0.15000000596046448);
		}
		return CardNodes;
	}

	public void ReAddCardNodes()
	{
		_cardTween?.Kill();
		_cardTween = CreateTween().SetParallel();
		for (int i = 0; i < _cardNodes.Count; i++)
		{
			NCard nCard = _cardNodes[i];
			Vector2 globalPosition = nCard.GlobalPosition;
			nCard.GetParent()?.RemoveChildSafely(nCard);
			_cardHolder = GetNode<Control>("%Cards");
			_cardHolder.AddChildSafely(nCard);
			nCard.GlobalPosition = globalPosition;
			nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			_cardTween.TweenProperty(nCard, "position", new Vector2(-1f, 1f) * 45f * ((float)i - (float)_cardNodes.Count / 2f), 0.4000000059604645).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			float num = 0.5f + (float)i / (float)(_cardNodes.Count - 1) * 0.5f;
			_cardTween.TweenProperty(nCard, "modulate", new Color(num, num, num), 0.4000000059604645);
		}
	}

	private void OnClicked(NClickableControl _)
	{
		EmitSignal(SignalName.Clicked, this);
	}

	private void OnFocused(NClickableControl _)
	{
		_hoverTween?.Kill();
		base.Scale = _hoverScale;
	}

	private void OnUnfocused(NClickableControl _)
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.TweenProperty(this, "scale", smallScale, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public override void _ExitTree()
	{
		foreach (NCard cardNode in _cardNodes)
		{
			if (IsAncestorOf(cardNode))
			{
				cardNode.QueueFreeSafely();
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReAddCardNodes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ReAddCardNodes && args.Count == 0)
		{
			ReAddCardNodes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnClicked && args.Count == 1)
		{
			OnClicked(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocused && args.Count == 1)
		{
			OnFocused(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocused && args.Count == 1)
		{
			OnUnfocused(VariantUtils.ConvertTo<NClickableControl>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName.ReAddCardNodes)
		{
			return true;
		}
		if (method == MethodName.OnClicked)
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			Hitbox = VariantUtils.ConvertTo<NClickableControl>(in value);
			return true;
		}
		if (name == PropertyName._cardHolder)
		{
			_cardHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hitbox)
		{
			value = VariantUtils.CreateFrom<NClickableControl>(Hitbox);
			return true;
		}
		if (name == PropertyName._hoverScale)
		{
			value = VariantUtils.CreateFrom(in _hoverScale);
			return true;
		}
		if (name == PropertyName.smallScale)
		{
			value = VariantUtils.CreateFrom(in smallScale);
			return true;
		}
		if (name == PropertyName._cardHolder)
		{
			value = VariantUtils.CreateFrom(in _cardHolder);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom(in _cardTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._hoverScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.smallScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hitbox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Hitbox, Variant.From<NClickableControl>(Hitbox));
		info.AddProperty(PropertyName._cardHolder, Variant.From(in _cardHolder));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._cardTween, Variant.From(in _cardTween));
		info.AddSignalEventDelegate(SignalName.Clicked, backing_Clicked);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Hitbox, out var value))
		{
			Hitbox = value.As<NClickableControl>();
		}
		if (info.TryGetProperty(PropertyName._cardHolder, out var value2))
		{
			_cardHolder = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value3))
		{
			_hoverTween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cardTween, out var value4))
		{
			_cardTween = value4.As<Tween>();
		}
		if (info.TryGetSignalEventDelegate<ClickedEventHandler>(SignalName.Clicked, out var value5))
		{
			backing_Clicked = value5;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(SignalName.Clicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cardHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalClicked(NCardBundle cardHolder)
	{
		EmitSignal(SignalName.Clicked, cardHolder);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Clicked && args.Count == 1)
		{
			backing_Clicked?.Invoke(VariantUtils.ConvertTo<NCardBundle>(in args[0]));
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
