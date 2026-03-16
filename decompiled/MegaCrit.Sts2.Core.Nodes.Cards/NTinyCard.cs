using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NTinyCard.cs")]
public class NTinyCard : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetCardPortraitShape = "SetCardPortraitShape";

		public static readonly StringName SetBannerColor = "SetBannerColor";

		public static readonly StringName GetBannerColor = "GetBannerColor";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _cardBack = "_cardBack";

		public static readonly StringName _cardPortrait = "_cardPortrait";

		public static readonly StringName _cardPortraitShadow = "_cardPortraitShadow";

		public static readonly StringName _cardBanner = "_cardBanner";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private TextureRect _cardBack;

	private TextureRect _cardPortrait;

	private TextureRect _cardPortraitShadow;

	private Control _cardBanner;

	public override void _Ready()
	{
		ConnectSignals();
		_cardBack = GetNode<TextureRect>("%CardBack");
		_cardPortrait = GetNode<TextureRect>("%Portrait");
		_cardPortraitShadow = GetNode<TextureRect>("%PortraitShadow");
		_cardBanner = GetNode<Control>("%Banner");
	}

	public void SetCard(CardModel card)
	{
		SetCardBackColor(card.Pool);
		SetCardPortraitShape(card.Type);
		SetBannerColor(card.Rarity);
		_cardBack.Material = card.FrameMaterial;
	}

	public void Set(CardPoolModel cardPool, CardType type, CardRarity rarity)
	{
		SetCardBackColor(cardPool);
		SetCardPortraitShape(type);
		SetBannerColor(rarity);
		_cardBack.Material = cardPool.AllCards.First().FrameMaterial;
	}

	private void SetCardBackColor(CardPoolModel cardPool)
	{
		_cardBack.Modulate = cardPool.DeckEntryCardColor;
	}

	private void SetCardPortraitShape(CardType type)
	{
		switch (type)
		{
		case CardType.Attack:
			_cardPortrait.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/attack_portrait.png");
			_cardPortraitShadow.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/attack_portrait_shadow.png");
			break;
		case CardType.Power:
			_cardPortrait.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/power_portrait.png");
			_cardPortraitShadow.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/power_portrait_shadow.png");
			break;
		default:
			_cardPortrait.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/skill_portrait.png");
			_cardPortraitShadow.Texture = PreloadManager.Cache.GetCompressedTexture2D("res://images/packed/run_history/skill_portrait_shadow.png");
			break;
		}
	}

	private void SetBannerColor(CardRarity rarity)
	{
		_cardBanner.Modulate = GetBannerColor(rarity);
	}

	private Color GetBannerColor(CardRarity rarity)
	{
		switch (rarity)
		{
		case CardRarity.Basic:
		case CardRarity.Common:
			return new Color("9C9C9CFF");
		case CardRarity.Uncommon:
			return new Color("64FFFFFF");
		case CardRarity.Rare:
			return new Color("FFDA36FF");
		case CardRarity.Curse:
			return new Color("E669FFFF");
		case CardRarity.Event:
			return new Color("13BE1AFF");
		case CardRarity.Quest:
			return new Color("F46836FF");
		default:
			Log.Warn($"Unspecified Rarity: {rarity}");
			return Colors.White;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetCardPortraitShape, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetBannerColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetBannerColor, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "rarity", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetCardPortraitShape && args.Count == 1)
		{
			SetCardPortraitShape(VariantUtils.ConvertTo<CardType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetBannerColor && args.Count == 1)
		{
			SetBannerColor(VariantUtils.ConvertTo<CardRarity>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetBannerColor && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Color>(GetBannerColor(VariantUtils.ConvertTo<CardRarity>(in args[0])));
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
		if (method == MethodName.SetCardPortraitShape)
		{
			return true;
		}
		if (method == MethodName.SetBannerColor)
		{
			return true;
		}
		if (method == MethodName.GetBannerColor)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._cardBack)
		{
			_cardBack = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._cardPortrait)
		{
			_cardPortrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._cardPortraitShadow)
		{
			_cardPortraitShadow = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._cardBanner)
		{
			_cardBanner = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._cardBack)
		{
			value = VariantUtils.CreateFrom(in _cardBack);
			return true;
		}
		if (name == PropertyName._cardPortrait)
		{
			value = VariantUtils.CreateFrom(in _cardPortrait);
			return true;
		}
		if (name == PropertyName._cardPortraitShadow)
		{
			value = VariantUtils.CreateFrom(in _cardPortraitShadow);
			return true;
		}
		if (name == PropertyName._cardBanner)
		{
			value = VariantUtils.CreateFrom(in _cardBanner);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardBack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardPortrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardPortraitShadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardBanner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._cardBack, Variant.From(in _cardBack));
		info.AddProperty(PropertyName._cardPortrait, Variant.From(in _cardPortrait));
		info.AddProperty(PropertyName._cardPortraitShadow, Variant.From(in _cardPortraitShadow));
		info.AddProperty(PropertyName._cardBanner, Variant.From(in _cardBanner));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._cardBack, out var value))
		{
			_cardBack = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._cardPortrait, out var value2))
		{
			_cardPortrait = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._cardPortraitShadow, out var value3))
		{
			_cardPortraitShadow = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._cardBanner, out var value4))
		{
			_cardBanner = value4.As<Control>();
		}
	}
}
