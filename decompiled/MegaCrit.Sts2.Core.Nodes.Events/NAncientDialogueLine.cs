using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NAncientDialogueLine.cs")]
public class NAncientDialogueLine : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnPress = "OnPress";

		public static readonly StringName PlaySfx = "PlaySfx";

		public static readonly StringName SetAncientAsSpeaker = "SetAncientAsSpeaker";

		public static readonly StringName SetCharacterAsSpeaker = "SetCharacterAsSpeaker";

		public static readonly StringName SetSpeakerIconVisible = "SetSpeakerIconVisible";

		public static readonly StringName SetTransparency = "SetTransparency";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _iconNode = "_iconNode";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _targetAlpha = "_targetAlpha";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private const string _scenePath = "res://scenes/events/ancient_dialogue_line.tscn";

	private AncientDialogueLine _line;

	private AncientEventModel _ancient;

	private CharacterModel _character;

	private Control _iconNode;

	private Tween? _tween;

	private float _targetAlpha = 1f;

	public static NAncientDialogueLine Create(AncientDialogueLine line, AncientEventModel ancient, CharacterModel character)
	{
		NAncientDialogueLine nAncientDialogueLine = PreloadManager.Cache.GetScene("res://scenes/events/ancient_dialogue_line.tscn").Instantiate<NAncientDialogueLine>(PackedScene.GenEditState.Disabled);
		nAncientDialogueLine._line = line;
		nAncientDialogueLine._ancient = ancient;
		nAncientDialogueLine._character = character;
		return nAncientDialogueLine;
	}

	public override void _Ready()
	{
		ConnectSignals();
		LocString lineText = _line.LineText;
		_character.AddDetailsTo(lineText);
		GetNode<MegaRichTextLabel>("%Text").Text = lineText.GetFormattedText();
		switch (_line.Speaker)
		{
		case AncientDialogueSpeaker.Ancient:
			SetAncientAsSpeaker();
			break;
		case AncientDialogueSpeaker.Character:
			SetCharacterAsSpeaker();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		base.Modulate = StsColors.transparentWhite;
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.1);
	}

	protected override void OnUnfocus()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", _targetAlpha, 0.1);
	}

	protected override void OnFocus()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate:a", 1f, 0.1);
	}

	protected override void OnPress()
	{
	}

	public void PlaySfx()
	{
		SfxCmd.Play(_line.GetSfxOrFallbackPath());
	}

	private void SetAncientAsSpeaker()
	{
		Control node = GetNode<Control>("%AncientIcon");
		node.GetNode<TextureRect>("Icon").Texture = _ancient.RunHistoryIcon;
		node.GetNode<TextureRect>("Icon/Outline").Texture = _ancient.RunHistoryIconOutline;
		_iconNode = node;
		Control node2 = GetNode<Control>("%DialogueTailLeft");
		node2.Visible = true;
		MarginContainer node3 = GetNode<MarginContainer>("%TextContainer");
		node3.AddThemeConstantOverride(ThemeConstants.MarginContainer.marginLeft, 48);
		GetNode<Control>("%Bubble").SelfModulate = _ancient.DialogueColor;
		node2.SelfModulate = _ancient.DialogueColor;
	}

	private void SetCharacterAsSpeaker()
	{
		Control node = GetNode<Control>("%CharacterIcon");
		node.GetNode<TextureRect>("Icon").Texture = _character.IconTexture;
		node.GetNode<TextureRect>("Icon/Outline").Texture = _character.IconOutlineTexture;
		_iconNode = node;
		Control node2 = GetNode<Control>("%DialogueTailRight");
		node2.Visible = true;
		MarginContainer node3 = GetNode<MarginContainer>("%TextContainer");
		node3.AddThemeConstantOverride(ThemeConstants.MarginContainer.marginRight, 46);
		GetNode<Control>("%Bubble").SelfModulate = _character.DialogueColor;
		node2.SelfModulate = _character.DialogueColor;
	}

	public void SetSpeakerIconVisible()
	{
		_iconNode.Visible = true;
	}

	public void SetTransparency(float alpha)
	{
		_targetAlpha = alpha;
		base.Modulate = new Color(1f, 1f, 1f, alpha);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlaySfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAncientAsSpeaker, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCharacterAsSpeaker, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSpeakerIconVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetTransparency, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "alpha", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlaySfx && args.Count == 0)
		{
			PlaySfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAncientAsSpeaker && args.Count == 0)
		{
			SetAncientAsSpeaker();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCharacterAsSpeaker && args.Count == 0)
		{
			SetCharacterAsSpeaker();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSpeakerIconVisible && args.Count == 0)
		{
			SetSpeakerIconVisible();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTransparency && args.Count == 1)
		{
			SetTransparency(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.PlaySfx)
		{
			return true;
		}
		if (method == MethodName.SetAncientAsSpeaker)
		{
			return true;
		}
		if (method == MethodName.SetCharacterAsSpeaker)
		{
			return true;
		}
		if (method == MethodName.SetSpeakerIconVisible)
		{
			return true;
		}
		if (method == MethodName.SetTransparency)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._iconNode)
		{
			_iconNode = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._targetAlpha)
		{
			_targetAlpha = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._iconNode)
		{
			value = VariantUtils.CreateFrom(in _iconNode);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._targetAlpha)
		{
			value = VariantUtils.CreateFrom(in _targetAlpha);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetAlpha, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._iconNode, Variant.From(in _iconNode));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._targetAlpha, Variant.From(in _targetAlpha));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._iconNode, out var value))
		{
			_iconNode = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._targetAlpha, out var value3))
		{
			_targetAlpha = value3.As<float>();
		}
	}
}
