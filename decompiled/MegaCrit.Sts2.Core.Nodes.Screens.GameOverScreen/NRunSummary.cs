using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
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
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

/// <summary>
/// Animates the Run Summary portion of the Death Screen.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/GameOverScreen/NRunSummary.cs")]
public class NRunSummary : Control
{
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
		/// Cached name for the 'SetControllerNav' method.
		/// </summary>
		public static readonly StringName SetControllerNav = "SetControllerNav";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_discoveryContainer' field.
		/// </summary>
		public static readonly StringName _discoveryContainer = "_discoveryContainer";

		/// <summary>
		/// Cached name for the '_discoveryHeader' field.
		/// </summary>
		public static readonly StringName _discoveryHeader = "_discoveryHeader";

		/// <summary>
		/// Cached name for the '_discoveredContents' field.
		/// </summary>
		public static readonly StringName _discoveredContents = "_discoveredContents";

		/// <summary>
		/// Cached name for the '_discoveredCards' field.
		/// </summary>
		public static readonly StringName _discoveredCards = "_discoveredCards";

		/// <summary>
		/// Cached name for the '_discoveredRelics' field.
		/// </summary>
		public static readonly StringName _discoveredRelics = "_discoveredRelics";

		/// <summary>
		/// Cached name for the '_discoveredPotions' field.
		/// </summary>
		public static readonly StringName _discoveredPotions = "_discoveredPotions";

		/// <summary>
		/// Cached name for the '_discoveredEnemies' field.
		/// </summary>
		public static readonly StringName _discoveredEnemies = "_discoveredEnemies";

		/// <summary>
		/// Cached name for the '_discoveredEpochs' field.
		/// </summary>
		public static readonly StringName _discoveredEpochs = "_discoveredEpochs";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_waitTween' field.
		/// </summary>
		public static readonly StringName _waitTween = "_waitTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Control _discoveryContainer;

	private Control _discoveryHeader;

	private Control _discoveredContents;

	private NDiscoveredItem _discoveredCards;

	private NDiscoveredItem _discoveredRelics;

	private NDiscoveredItem _discoveredPotions;

	private NDiscoveredItem _discoveredEnemies;

	private NDiscoveredItem _discoveredEpochs;

	private Tween? _tween;

	private Tween? _waitTween;

	private const int _maxItemsToList = 10;

	public Control? DefaultFocusedControl => _discoveredContents.GetChildren().OfType<Control>().FirstOrDefault((Control c) => c.IsVisible());

	public override void _Ready()
	{
		_discoveryContainer = GetNode<Control>("%DiscoveryContainer");
		_discoveryHeader = GetNode<Control>("%DiscoveryHeader");
		_discoveredContents = GetNode<Control>("%DiscoveredContents");
		_discoveredCards = GetNode<NDiscoveredItem>("%DiscoveredCards");
		_discoveredRelics = GetNode<NDiscoveredItem>("%DiscoveredRelics");
		_discoveredPotions = GetNode<NDiscoveredItem>("%DiscoveredPotions");
		_discoveredEnemies = GetNode<NDiscoveredItem>("%DiscoveredEnemies");
		_discoveredEpochs = GetNode<NDiscoveredItem>("%DiscoveredEpochs");
		_discoveredCards.Visible = false;
		_discoveredRelics.Visible = false;
		_discoveredPotions.Visible = false;
		_discoveredEnemies.Visible = false;
		_discoveredEpochs.Visible = false;
	}

	public async Task AnimateInDiscoveries(RunState runState, CancellationToken ct)
	{
		Player player = LocalContext.GetMe(runState);
		if (player.DiscoveredCards.Count + player.DiscoveredRelics.Count + player.DiscoveredPotions.Count + player.DiscoveredEnemies.Count + player.DiscoveredEpochs.Count == 0)
		{
			Log.Info("No discoveries this time. Very sad");
			return;
		}
		Tween tween = CreateTween();
		tween.TweenProperty(_discoveryHeader, "modulate:a", 1f, 0.25);
		await Task.Delay(100, ct);
		if (!this.IsValid())
		{
			return;
		}
		if (player.DiscoveredCards.Count > 0)
		{
			string discoveryBodyText = GetDiscoveryBodyText(player.DiscoveredCards, (ModelId id) => SaveUtil.CardOrDeprecated(id).Title, "game_over_screen", "DISCOVERY_BODY_CARD", "CardCount");
			_discoveredCards.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_CARD"), discoveryBodyText));
			_discoveredCards.Visible = true;
			_discoveredCards.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredRelics.Count > 0)
		{
			string discoveryBodyText2 = GetDiscoveryBodyText(player.DiscoveredRelics, (ModelId id) => SaveUtil.RelicOrDeprecated(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_RELIC", "RelicCount");
			_discoveredRelics.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_RELIC"), discoveryBodyText2));
			_discoveredRelics.Visible = true;
			_discoveredRelics.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredPotions.Count > 0)
		{
			string discoveryBodyText3 = GetDiscoveryBodyText(player.DiscoveredPotions, (ModelId id) => SaveUtil.PotionOrDeprecated(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_POTION", "PotionCount");
			_discoveredPotions.SetHoverTip(new HoverTip(new LocString("game_over_screen", "DISCOVERY_HEADER_POTION"), discoveryBodyText3));
			_discoveredPotions.Visible = true;
			_discoveredPotions.Modulate = StsColors.transparentBlack;
		}
		if (player.DiscoveredEnemies.Count > 0)
		{
			string discoveryBodyText4 = GetDiscoveryBodyText(player.DiscoveredEnemies, (ModelId id) => SaveUtil.MonsterOrDeprecated(id).Title.GetFormattedText(), "game_over_screen", "DISCOVERY_BODY_ENEMY", "EnemyCount");
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
			_discoveredCards.SetText($"{player.DiscoveredCards.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredCards));
		}
		if (_discoveredRelics.Visible)
		{
			_discoveredRelics.SetText($"{player.DiscoveredRelics.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredRelics));
		}
		if (_discoveredPotions.Visible)
		{
			_discoveredPotions.SetText($"{player.DiscoveredPotions.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredPotions));
		}
		if (_discoveredEnemies.Visible)
		{
			_discoveredEnemies.SetText($"{player.DiscoveredEnemies.Count}");
			await TaskHelper.RunSafely(DiscoveryAnimHelper(_discoveredEnemies));
		}
		if (_discoveredEpochs.Visible)
		{
			_discoveredEpochs.SetText($"{player.DiscoveredEpochs.Count}");
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
		await _tween.AwaitFinished(this);
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

	public void SetControllerNav(Control? focusNeighborTop)
	{
		Control[] array = (from c in _discoveredContents.GetChildren().OfType<Control>()
			where c.IsVisible()
			select c).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			Control control = array[num];
			control.FocusNeighborTop = ((focusNeighborTop != null) ? focusNeighborTop.GetPath() : control.GetPath());
			control.FocusNeighborBottom = control.GetPath();
			control.FocusNeighborLeft = ((num > 0) ? array[num - 1].GetPath() : array[^1].GetPath());
			control.FocusNeighborRight = ((num < array.Length - 1) ? array[num + 1].GetPath() : array[0].GetPath());
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
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetControllerNav, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusNeighborTop", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.SetControllerNav && args.Count == 1)
		{
			SetControllerNav(VariantUtils.ConvertTo<Control>(in args[0]));
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
		if (method == MethodName.SetControllerNav)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
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
		if (name == PropertyName._discoveredContents)
		{
			_discoveredContents = VariantUtils.ConvertTo<Control>(in value);
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
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
		if (name == PropertyName._discoveredContents)
		{
			value = VariantUtils.CreateFrom(in _discoveredContents);
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveryContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveryHeader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredContents, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredCards, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredRelics, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredPotions, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredEnemies, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discoveredEpochs, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._waitTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._discoveryContainer, Variant.From(in _discoveryContainer));
		info.AddProperty(PropertyName._discoveryHeader, Variant.From(in _discoveryHeader));
		info.AddProperty(PropertyName._discoveredContents, Variant.From(in _discoveredContents));
		info.AddProperty(PropertyName._discoveredCards, Variant.From(in _discoveredCards));
		info.AddProperty(PropertyName._discoveredRelics, Variant.From(in _discoveredRelics));
		info.AddProperty(PropertyName._discoveredPotions, Variant.From(in _discoveredPotions));
		info.AddProperty(PropertyName._discoveredEnemies, Variant.From(in _discoveredEnemies));
		info.AddProperty(PropertyName._discoveredEpochs, Variant.From(in _discoveredEpochs));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._waitTween, Variant.From(in _waitTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._discoveryContainer, out var value))
		{
			_discoveryContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveryHeader, out var value2))
		{
			_discoveryHeader = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._discoveredContents, out var value3))
		{
			_discoveredContents = value3.As<Control>();
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
		if (info.TryGetProperty(PropertyName._tween, out var value9))
		{
			_tween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._waitTween, out var value10))
		{
			_waitTween = value10.As<Tween>();
		}
	}
}
