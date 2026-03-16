using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Shops;

[ScriptPath("res://src/Core/Nodes/Screens/Shops/NMerchantDialogue.cs")]
public class NMerchantDialogue : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ShowOnInventoryOpen = "ShowOnInventoryOpen";

		public static readonly StringName ShowForPurchaseAttempt = "ShowForPurchaseAttempt";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName _label = "_label";

		public static readonly StringName _dialogueBox = "_dialogueBox";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _bubble = "_bubble";

		public static readonly StringName _hsv = "_hsv";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private static readonly Vector2 _xRange = new Vector2(450f, 1450f);

	private MegaRichTextLabel _label;

	private Node2D _dialogueBox;

	private Tween? _tween;

	private Sprite2D _bubble;

	private ShaderMaterial _hsv;

	private MerchantDialogueSet _dialogueSet;

	public override void _Ready()
	{
		_label = GetNode<MegaRichTextLabel>("%Text");
		_dialogueBox = GetNode<Node2D>("%DialogueBox");
		_bubble = GetNode<Sprite2D>("%Bubble");
		base.Modulate = Colors.Transparent;
		_hsv = (ShaderMaterial)_bubble.Material;
		_hsv.SetShaderParameter(_h, 1f);
		_hsv.SetShaderParameter(_s, 1.2f);
		_hsv.SetShaderParameter(_v, 0.4f);
	}

	public void Initialize(MerchantDialogueSet dialogueSet)
	{
		_dialogueSet = dialogueSet;
	}

	public void ShowOnInventoryOpen()
	{
		ShowRandom(_dialogueSet.OpenInventoryLines);
	}

	public void ShowForPurchaseAttempt(PurchaseStatus status)
	{
		ShowRandom(_dialogueSet.GetPurchaseSuccessLines(status));
	}

	private void ShowRandom(IEnumerable<LocString> lines)
	{
		LocString locString = Rng.Chaotic.NextItem(lines);
		if (locString != null)
		{
			_label.Text = "[fly_in]" + locString.GetFormattedText() + "[/fly_in]";
			base.Modulate = StsColors.transparentWhite;
			base.Position = new Vector2(Rng.Chaotic.NextFloat(_xRange.X, _xRange.Y), base.Position.Y);
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			_tween.TweenProperty(this, "modulate:a", 1f, 0.25);
			_tween.TweenProperty(this, "scale", Vector2.One, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
			_tween.TweenProperty(_label, "visible_ratio", 1f, 0.4).From(0f);
			_tween.TweenProperty(_bubble, "scale", new Vector2(0.75f, 0.75f), 0.5).From(new Vector2(0.25f, 0.25f)).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			_tween.TweenProperty(_dialogueBox, "position:y", 0f, 0.5).From(-80f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
			_tween.Chain();
			_tween.TweenInterval(1.0);
			_tween.Chain();
			_tween.TweenProperty(this, "modulate:a", 0f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowOnInventoryOpen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowForPurchaseAttempt, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.ShowOnInventoryOpen && args.Count == 0)
		{
			ShowOnInventoryOpen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowForPurchaseAttempt && args.Count == 1)
		{
			ShowForPurchaseAttempt(VariantUtils.ConvertTo<PurchaseStatus>(in args[0]));
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
		if (method == MethodName.ShowOnInventoryOpen)
		{
			return true;
		}
		if (method == MethodName.ShowForPurchaseAttempt)
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
			_label = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._dialogueBox)
		{
			_dialogueBox = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._bubble)
		{
			_bubble = VariantUtils.ConvertTo<Sprite2D>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._label)
		{
			value = VariantUtils.CreateFrom(in _label);
			return true;
		}
		if (name == PropertyName._dialogueBox)
		{
			value = VariantUtils.CreateFrom(in _dialogueBox);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._bubble)
		{
			value = VariantUtils.CreateFrom(in _bubble);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._label, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogueBox, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bubble, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._label, Variant.From(in _label));
		info.AddProperty(PropertyName._dialogueBox, Variant.From(in _dialogueBox));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._bubble, Variant.From(in _bubble));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._label, out var value))
		{
			_label = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._dialogueBox, out var value2))
		{
			_dialogueBox = value2.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._bubble, out var value4))
		{
			_bubble = value4.As<Sprite2D>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value5))
		{
			_hsv = value5.As<ShaderMaterial>();
		}
	}
}
