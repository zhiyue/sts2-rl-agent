using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

[ScriptPath("res://src/Core/Nodes/Screens/GameOverScreen/NRunSummary.cs")]
public class NRunSummary : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _badgeContainer = "_badgeContainer";

		public static readonly StringName _discoveryContainer = "_discoveryContainer";

		public static readonly StringName _discoveryHeader = "_discoveryHeader";

		public static readonly StringName _discoveredCards = "_discoveredCards";

		public static readonly StringName _discoveredRelics = "_discoveredRelics";

		public static readonly StringName _discoveredPotions = "_discoveredPotions";

		public static readonly StringName _discoveredEnemies = "_discoveredEnemies";

		public static readonly StringName _discoveredEpochs = "_discoveredEpochs";

		public static readonly StringName _cardCount = "_cardCount";

		public static readonly StringName _relicCount = "_relicCount";

		public static readonly StringName _potionCount = "_potionCount";

		public static readonly StringName _enemyCount = "_enemyCount";

		public static readonly StringName _epochCount = "_epochCount";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _waitTween = "_waitTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _badgeContainer;

	private Control _discoveryContainer;

	private Control _discoveryHeader;

	private NDiscoveredItem _discoveredCards;

	private NDiscoveredItem _discoveredRelics;

	private NDiscoveredItem _discoveredPotions;

	private NDiscoveredItem _discoveredEnemies;

	private NDiscoveredItem _discoveredEpochs;

	private MegaLabel _cardCount;

	private MegaLabel _relicCount;

	private MegaLabel _potionCount;

	private MegaLabel _enemyCount;

	private MegaLabel _epochCount;

	private Tween? _tween;

	private Tween? _waitTween;

	private const int _maxItemsToList = 10;

	public override void _Ready()
	{
		_badgeContainer = GetNode<Control>("%BadgeContainer");
		_discoveryContainer = GetNode<Control>("%DiscoveryContainer");
		_discoveryHeader = GetNode<Control>("%DiscoveryHeader");
		_cardCount = GetNode<MegaLabel>("%CardCount");
		_relicCount = GetNode<MegaLabel>("%RelicCount");
		_potionCount = GetNode<MegaLabel>("%PotionCount");
		_enemyCount = GetNode<MegaLabel>("%EnemyCount");
		_epochCount = GetNode<MegaLabel>("%EpochCount");
		_discoveredCards = _cardCount.GetParent<NDiscoveredItem>();
		_discoveredRelics = _relicCount.GetParent<NDiscoveredItem>();
		_discoveredPotions = _potionCount.GetParent<NDiscoveredItem>();
		_discoveredEnemies = _enemyCount.GetParent<NDiscoveredItem>();
		_discoveredEpochs = _epochCount.GetParent<NDiscoveredItem>();
		_discoveredCards.Visible = false;
		_discoveredRelics.Visible = false;
		_discoveredPotions.Visible = false;
		_discoveredEnemies.Visible = false;
		_discoveredEpochs.Visible = false;
	}

	public async Task AnimateInDiscoveries(RunState runState)
	{
		Player player = LocalContext.GetMe(runState);
		if (player.DiscoveredCards.Count + player.DiscoveredRelics.Count + player.DiscoveredPotions.Count + player.DiscoveredEnemies.Count + player.DiscoveredEpochs.Count == 0)
		{
			Log.Info("No discoveries this time. Very sad");
			return;
		}
		Tween tween = CreateTween();
		tween.TweenProperty(_discoveryHeader, "modulate:a", 1f, 0.25);
		await Task.Delay(100);
		if (player.DiscoveredCards.Count > 0)
		{
			string discoveryBodyText = GetDiscoveryBodyText(player.DiscoveredCards, (ModelId id) => ModelDb.GetById<CardModel>(id).Title, "game_over_screen", "DISCOVERY_BODY_CARD", "CardCount");
			_discoveredCards.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_CARD"), discoveryBodyText));
			_discoveredCards.Visible = true;
			_discoveredCards.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredRelics.Count > 0)
		{
			string discoveryBodyText2 = GetDiscoveryBodyText(player.DiscoveredRelics, (ModelId id) => ModelDb.GetById<RelicModel>(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_RELIC", "RelicCount");
			_discoveredRelics.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_RELIC"), discoveryBodyText2));
			_discoveredRelics.Visible = true;
			_discoveredRelics.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredPotions.Count > 0)
		{
			string discoveryBodyText3 = GetDiscoveryBodyText(player.DiscoveredPotions, (ModelId id) => ModelDb.GetById<PotionModel>(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_POTION", "PotionCount");
			_discoveredPotions.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_POTION"), discoveryBodyText3));
			_discoveredPotions.Visible = true;
			_discoveredPotions.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredEnemies.Count > 0)
		{
			string discoveryBodyText4 = GetDiscoveryBodyText(player.DiscoveredEnemies, (ModelId id) => ModelDb.GetById<MonsterModel>(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_ENEMY", "EnemyCount");
			_discoveredEnemies.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_ENEMY"), discoveryBodyText4));
			_discoveredEnemies.Visible = true;
			_discoveredEnemies.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredEpochs.Count > 0)
		{
			LocString title = new LocString("game_over_screen", "DISCOVERY_HEADER_EPOCH");
			LocString locString = new LocString("game_over_screen", "DISCOVERY_BODY_EPOCH");
			locString.Add("EpochCount", player.DiscoveredEpochs.Count);
			HoverTip hoverTip = new HoverTip(title, locString);
			_discoveredEpochs.SetHoverTip(hoverTip);
			_discoveredEpochs.Visible = true;
			_discoveredEpochs.Modulate = StsColors.transparentBlack;
		}
		if (_discoveredCards.Visible)
		{
			_cardCount.SetTextAutoSize($"{player.DiscoveredCards.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredCards));
		}
		if (_discoveredRelics.Visible)
		{
			_relicCount.SetTextAutoSize($"{player.DiscoveredRelics.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredRelics));
		}
		if (_discoveredPotions.Visible)
		{
			_potionCount.SetTextAutoSize($"{player.DiscoveredPotions.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredPotions));
		}
		if (_discoveredEnemies.Visible)
		{
			_enemyCount.SetTextAutoSize($"{player.DiscoveredEnemies.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredEnemies));
		}
		if (_discoveredEpochs.Visible)
		{
			_epochCount.SetTextAutoSize($"{player.DiscoveredEpochs.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredEpochs));
		}
	}

	private async Task DiscoveryAnimHelper(Control node)
	{
		node.Modulate = StsColors.transparentBlack;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(node, "modulate", Colors.White, 0.3);
		_tween.TweenProperty(node, "position:y", 0f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(100f);
		await ToSignal(_tween, Tween.SignalName.Finished);
	}

	private static string GetDiscoveryBodyText<T>(List<T> discoveredIds, Func<T, string> getTitle, string locTable, string locKey, string countParam)
	{
		LocString locString = new LocString(locTable, locKey);
		locString.Add(countParam, discoveredIds.Count);
		string text = string.Join("\n", discoveredIds.Take(10).Select(getTitle));
		if (discoveredIds.Count > 10)
		{
			text += "\n....";
		}
		return locString.GetFormattedText() + "\n\n" + text;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._badgeContainer)
		{
			_badgeContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._discoveryContainer)
		{
			_discoveryContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._discoveryHeader)
		{
			_discoveryHeader = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._discoveredCards)
		{
			_discoveredCards = VariantUtils.ConvertTo<NDiscoveredItem>(in value);
			return true;
		}
		if (name == PropertyName._discoveredRelics)
		{
			_discoveredRelics = VariantUtils.ConvertTo<NDiscoveredItem>(in value);
			return true;
		}
		if (name == PropertyName._discoveredPotions)
		{
			_discoveredPotions = VariantUtils.ConvertTo<NDiscoveredItem>(in value);
			return true;
		}
		if (name == PropertyName._discoveredEnemies)
		{
			_discoveredEnemies = VariantUtils.ConvertTo<NDiscoveredItem>(in value);
			return true;
		}
		if (name == PropertyName._discoveredEpochs)
		{
			_discoveredEpochs = VariantUtils.ConvertTo<NDiscoveredItem>(in value);
			return true;
		}
		if (name == PropertyName._cardCount)
		{
			_cardCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicCount)
		{
			_relicCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._potionCount)
		{
			_potionCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._enemyCount)
		{
			_enemyCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._epochCount)
		{
			_epochCount = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._waitTween)
		{
			_waitTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._badgeContainer)
		{
			value = VariantUtils.CreateFrom(in _badgeContainer);
			return true;
		}
		if (name == PropertyName._discoveryContainer)
		{
			value = VariantUtils.CreateFrom(in _discoveryContainer);
			return true;
		}
		if (name == PropertyName._discoveryHeader)
		{
			value = VariantUtils.CreateFrom(in _discoveryHeader);
			return true;
		}
		if (name == PropertyName._discoveredCards)
		{
			value = VariantUtils.CreateFrom(in _discoveredCards);
			return true;
		}
		if (name == PropertyName._discoveredRelics)
		{
			value = VariantUtils.CreateFrom(in _discoveredRelics);
			return true;
		}
		if (name == PropertyName._discoveredPotions)
		{
			value = VariantUtils.CreateFrom(in _discoveredPotions);
			return true;
		}
		if (name == PropertyName._discoveredEnemies)
		{
			value = VariantUtils.CreateFrom(in _discoveredEnemies);
			return true;
		}
		if (name == PropertyName._discoveredEpochs)
		{
			value = VariantUtils.CreateFrom(in _discoveredEpochs);
			return true;
		}
		if (name == PropertyName._cardCount)
		{
			value = VariantUtils.CreateFrom(in _cardCount);
			return true;
		}
		if (name == PropertyName._relicCount)
		{
			value = VariantUtils.CreateFrom(in _relicCount);
			return true;
		}
		if (name == PropertyName._potionCount)
		{
			value = VariantUtils.CreateFrom(in _potionCount);
			return true;
		}
		if (name == PropertyName._enemyCount)
		{
			value = VariantUtils.CreateFrom(in _enemyCount);
			return true;
		}
		if (name == PropertyName._epochCount)
		{
			value = VariantUtils.CreateFrom(in _epochCount);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._waitTween)
		{
			value = VariantUtils.CreateFrom(in _waitTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._badgeContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveryContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveryHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredRelics, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredPotions, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredEnemies, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredEpochs, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enemyCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epochCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._waitTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._badgeContainer, Variant.From(in _badgeContainer));
		info.AddProperty(PropertyName._discoveryContainer, Variant.From(in _discoveryContainer));
		info.AddProperty(PropertyName._discoveryHeader, Variant.From(in _discoveryHeader));
		info.AddProperty(PropertyName._discoveredCards, Variant.From(in _discoveredCards));
		info.AddProperty(PropertyName._discoveredRelics, Variant.From(in _discoveredRelics));
		info.AddProperty(PropertyName._discoveredPotions, Variant.From(in _discoveredPotions));
		info.AddProperty(PropertyName._discoveredEnemies, Variant.From(in _discoveredEnemies));
		info.AddProperty(PropertyName._discoveredEpochs, Variant.From(in _discoveredEpochs));
		info.AddProperty(PropertyName._cardCount, Variant.From(in _cardCount));
		info.AddProperty(PropertyName._relicCount, Variant.From(in _relicCount));
		info.AddProperty(PropertyName._potionCount, Variant.From(in _potionCount));
		info.AddProperty(PropertyName._enemyCount, Variant.From(in _enemyCount));
		info.AddProperty(PropertyName._epochCount, Variant.From(in _epochCount));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._waitTween, Variant.From(in _waitTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._badgeContainer, out var value))
		{
			_badgeContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveryContainer, out var value2))
		{
			_discoveryContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveryHeader, out var value3))
		{
			_discoveryHeader = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveredCards, out var value4))
		{
			_discoveredCards = value4.As<NDiscoveredItem>();
		}
		if (info.TryGetProperty(PropertyName._discoveredRelics, out var value5))
		{
			_discoveredRelics = value5.As<NDiscoveredItem>();
		}
		if (info.TryGetProperty(PropertyName._discoveredPotions, out var value6))
		{
			_discoveredPotions = value6.As<NDiscoveredItem>();
		}
		if (info.TryGetProperty(PropertyName._discoveredEnemies, out var value7))
		{
			_discoveredEnemies = value7.As<NDiscoveredItem>();
		}
		if (info.TryGetProperty(PropertyName._discoveredEpochs, out var value8))
		{
			_discoveredEpochs = value8.As<NDiscoveredItem>();
		}
		if (info.TryGetProperty(PropertyName._cardCount, out var value9))
		{
			_cardCount = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicCount, out var value10))
		{
			_relicCount = value10.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._potionCount, out var value11))
		{
			_potionCount = value11.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._enemyCount, out var value12))
		{
			_enemyCount = value12.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._epochCount, out var value13))
		{
			_epochCount = value13.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value14))
		{
			_tween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._waitTween, out var value15))
		{
			_waitTween = value15.As<Tween>();
		}
	}
}
