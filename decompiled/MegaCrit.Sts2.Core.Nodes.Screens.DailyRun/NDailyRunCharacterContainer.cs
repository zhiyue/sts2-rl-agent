using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;

[ScriptPath("res://src/Core/Nodes/Screens/DailyRun/NDailyRunCharacterContainer.cs")]
public class NDailyRunCharacterContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetIsReady = "SetIsReady";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _characterIconContainer = "_characterIconContainer";

		public static readonly StringName _playerNameLabel = "_playerNameLabel";

		public static readonly StringName _characterNameLabel = "_characterNameLabel";

		public static readonly StringName _ascensionLabel = "_ascensionLabel";

		public static readonly StringName _ascensionNumberLabel = "_ascensionNumberLabel";

		public static readonly StringName _readyIndicator = "_readyIndicator";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly LocString _ascensionLoc = new LocString("main_menu_ui", "DAILY_RUN_MENU.ASCENSION");

	private Control _characterIconContainer;

	private MegaLabel _playerNameLabel;

	private MegaLabel _characterNameLabel;

	private MegaLabel _ascensionLabel;

	private MegaLabel _ascensionNumberLabel;

	private Control _readyIndicator;

	public override void _Ready()
	{
		_characterIconContainer = GetNode<Control>("%CharacterIconContainer");
		_playerNameLabel = GetNode<MegaLabel>("%PlayerNameLabel");
		_characterNameLabel = GetNode<MegaLabel>("%CharacterNameLabel");
		_ascensionLabel = GetNode<MegaLabel>("%AscensionLabel");
		_ascensionNumberLabel = GetNode<MegaLabel>("%AscensionNumberLabel");
		_readyIndicator = GetNode<Control>("%ReadyIndicator");
	}

	public void Fill(CharacterModel character, ulong playerId, int ascension, INetGameService netService)
	{
		_ascensionLoc.Add("ascension", ascension);
		bool flag = netService.Type.IsMultiplayer();
		Control icon = character.Icon;
		foreach (Node child in _characterIconContainer.GetChildren())
		{
			_characterIconContainer.RemoveChildSafely(child);
		}
		_playerNameLabel.Visible = flag;
		_characterNameLabel.Modulate = (flag ? StsColors.cream : StsColors.gold);
		_characterIconContainer.AddChildSafely(icon);
		_characterNameLabel.SetTextAutoSize(character.Title.GetFormattedText());
		_playerNameLabel.SetTextAutoSize(PlatformUtil.GetPlayerName(netService.Platform, playerId));
		_ascensionLabel.SetTextAutoSize(_ascensionLoc.GetFormattedText());
		_ascensionNumberLabel.SetTextAutoSize(ascension.ToString());
	}

	public void SetIsReady(bool isReady)
	{
		_readyIndicator.Visible = isReady;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetIsReady, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isReady", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.SetIsReady && args.Count == 1)
		{
			SetIsReady(VariantUtils.ConvertTo<bool>(in args[0]));
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
		if (method == MethodName.SetIsReady)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._characterIconContainer)
		{
			_characterIconContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._playerNameLabel)
		{
			_playerNameLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._characterNameLabel)
		{
			_characterNameLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			_ascensionLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._ascensionNumberLabel)
		{
			_ascensionNumberLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			_readyIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._characterIconContainer)
		{
			value = VariantUtils.CreateFrom(in _characterIconContainer);
			return true;
		}
		if (name == PropertyName._playerNameLabel)
		{
			value = VariantUtils.CreateFrom(in _playerNameLabel);
			return true;
		}
		if (name == PropertyName._characterNameLabel)
		{
			value = VariantUtils.CreateFrom(in _characterNameLabel);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			value = VariantUtils.CreateFrom(in _ascensionLabel);
			return true;
		}
		if (name == PropertyName._ascensionNumberLabel)
		{
			value = VariantUtils.CreateFrom(in _ascensionNumberLabel);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			value = VariantUtils.CreateFrom(in _readyIndicator);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterIconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterNameLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionNumberLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._characterIconContainer, Variant.From(in _characterIconContainer));
		info.AddProperty(PropertyName._playerNameLabel, Variant.From(in _playerNameLabel));
		info.AddProperty(PropertyName._characterNameLabel, Variant.From(in _characterNameLabel));
		info.AddProperty(PropertyName._ascensionLabel, Variant.From(in _ascensionLabel));
		info.AddProperty(PropertyName._ascensionNumberLabel, Variant.From(in _ascensionNumberLabel));
		info.AddProperty(PropertyName._readyIndicator, Variant.From(in _readyIndicator));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._characterIconContainer, out var value))
		{
			_characterIconContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._playerNameLabel, out var value2))
		{
			_playerNameLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._characterNameLabel, out var value3))
		{
			_characterNameLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLabel, out var value4))
		{
			_ascensionLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._ascensionNumberLabel, out var value5))
		{
			_ascensionNumberLabel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._readyIndicator, out var value6))
		{
			_readyIndicator = value6.As<Control>();
		}
	}
}
