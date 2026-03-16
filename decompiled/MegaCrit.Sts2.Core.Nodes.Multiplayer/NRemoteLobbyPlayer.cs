using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NRemoteLobbyPlayer.cs")]
public class NRemoteLobbyPlayer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshVisuals = "RefreshVisuals";

		public static readonly StringName CancelShake = "CancelShake";

		public new static readonly StringName _Process = "_Process";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName PlayerId = "PlayerId";

		public static readonly StringName _characterIcon = "_characterIcon";

		public static readonly StringName _readyIndicator = "_readyIndicator";

		public static readonly StringName _disconnectedIndicator = "_disconnectedIndicator";

		public static readonly StringName _nameplateLabel = "_nameplateLabel";

		public static readonly StringName _characterLabel = "_characterLabel";

		public static readonly StringName _platform = "_platform";

		public static readonly StringName _isSingleplayer = "_isSingleplayer";

		public static readonly StringName _playerId = "_playerId";

		public static readonly StringName _isReady = "_isReady";

		public static readonly StringName _isConnected = "_isConnected";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("ui/remote_lobby_player");

	private TextureRect _characterIcon;

	private Control _readyIndicator;

	private Control _disconnectedIndicator;

	private MegaLabel _nameplateLabel;

	private MegaLabel _characterLabel;

	private PlatformType _platform;

	private bool _isSingleplayer;

	private ScreenPunchInstance? _shake;

	private Vector2? _originalPosition;

	private ulong _playerId;

	private CharacterModel _character;

	private bool _isReady;

	private bool _isConnected;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public ulong PlayerId => _playerId;

	public static NRemoteLobbyPlayer Create(LobbyPlayer player, PlatformType platform, bool isSingleplayer)
	{
		NRemoteLobbyPlayer nRemoteLobbyPlayer = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRemoteLobbyPlayer>(PackedScene.GenEditState.Disabled);
		nRemoteLobbyPlayer._playerId = player.id;
		nRemoteLobbyPlayer._platform = platform;
		nRemoteLobbyPlayer._isSingleplayer = isSingleplayer;
		nRemoteLobbyPlayer._character = player.character;
		nRemoteLobbyPlayer._isReady = player.isReady;
		nRemoteLobbyPlayer._isConnected = true;
		return nRemoteLobbyPlayer;
	}

	public static NRemoteLobbyPlayer Create(LoadRunLobby runLobby, ulong playerId, PlatformType platform, bool isSingleplayer)
	{
		NRemoteLobbyPlayer nRemoteLobbyPlayer = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NRemoteLobbyPlayer>(PackedScene.GenEditState.Disabled);
		nRemoteLobbyPlayer._playerId = playerId;
		nRemoteLobbyPlayer._isSingleplayer = isSingleplayer;
		nRemoteLobbyPlayer._platform = runLobby.NetService.Platform;
		nRemoteLobbyPlayer._character = ModelDb.GetById<CharacterModel>(runLobby.Run.Players.First((SerializablePlayer p) => p.NetId == playerId).CharacterId);
		nRemoteLobbyPlayer._isReady = runLobby.IsPlayerReady(playerId);
		nRemoteLobbyPlayer._isConnected = runLobby.ConnectedPlayerIds.Contains(playerId);
		return nRemoteLobbyPlayer;
	}

	public override void _Ready()
	{
		_nameplateLabel = GetNode<MegaLabel>("%NameplateLabel");
		_characterLabel = GetNode<MegaLabel>("%CharacterLabel");
		_characterIcon = GetNode<TextureRect>("%CharacterIcon");
		_readyIndicator = GetNode<TextureRect>("%ReadyIndicator");
		_disconnectedIndicator = GetNode<TextureRect>("%DisconnectedIndicator");
		if (!_isSingleplayer)
		{
			_nameplateLabel.SetTextAutoSize(PlatformUtil.GetPlayerName(_platform, _playerId));
		}
		else
		{
			_characterLabel.SetTextAutoSize(string.Empty);
		}
		RefreshVisuals();
	}

	public void OnPlayerChanged(LobbyPlayer lobbyPlayer)
	{
		_playerId = lobbyPlayer.id;
		SetCharacter(lobbyPlayer.character);
		_isReady = lobbyPlayer.isReady;
		_isConnected = true;
		RefreshVisuals();
	}

	public void OnPlayerChanged(LoadRunLobby runLobby, ulong playerId)
	{
		SerializablePlayer serializablePlayer = runLobby.Run.Players.First((SerializablePlayer p) => p.NetId == playerId);
		SetCharacter(ModelDb.GetById<CharacterModel>(serializablePlayer.CharacterId));
		_isReady = runLobby.IsPlayerReady(playerId);
		_isConnected = runLobby.ConnectedPlayerIds.Contains(playerId);
		RefreshVisuals();
	}

	private void RefreshVisuals()
	{
		if (_isSingleplayer)
		{
			_nameplateLabel.SetTextAutoSize(_character.Title.GetFormattedText());
		}
		else
		{
			_characterLabel.SetTextAutoSize(_character.Title.GetFormattedText());
		}
		_characterIcon.Texture = _character.IconTexture;
		_readyIndicator.Visible = _isReady;
		_disconnectedIndicator.Visible = !_isConnected;
	}

	private void SetCharacter(CharacterModel character)
	{
		if (_character != character)
		{
			_shake?.Cancel();
			CancelShake();
			_originalPosition = base.Position;
			_shake = new ScreenPunchInstance(3f, 0.4000000059604645, 90f);
			_character = character;
		}
	}

	public void CancelShake()
	{
		_shake = null;
		if (_originalPosition.HasValue)
		{
			base.Position = _originalPosition.Value;
			_originalPosition = null;
		}
	}

	public override void _Process(double delta)
	{
		ScreenPunchInstance shake = _shake;
		if (shake != null && !shake.IsDone)
		{
			Vector2 vector = _shake?.Update(delta) ?? Vector2.Zero;
			base.Position = _originalPosition.Value + vector;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CancelShake, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.RefreshVisuals && args.Count == 0)
		{
			RefreshVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CancelShake && args.Count == 0)
		{
			CancelShake();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
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
		if (method == MethodName.RefreshVisuals)
		{
			return true;
		}
		if (method == MethodName.CancelShake)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._characterIcon)
		{
			_characterIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			_readyIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._disconnectedIndicator)
		{
			_disconnectedIndicator = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			_nameplateLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._characterLabel)
		{
			_characterLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._platform)
		{
			_platform = VariantUtils.ConvertTo<PlatformType>(in value);
			return true;
		}
		if (name == PropertyName._isSingleplayer)
		{
			_isSingleplayer = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._playerId)
		{
			_playerId = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._isReady)
		{
			_isReady = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isConnected)
		{
			_isConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.PlayerId)
		{
			value = VariantUtils.CreateFrom<ulong>(PlayerId);
			return true;
		}
		if (name == PropertyName._characterIcon)
		{
			value = VariantUtils.CreateFrom(in _characterIcon);
			return true;
		}
		if (name == PropertyName._readyIndicator)
		{
			value = VariantUtils.CreateFrom(in _readyIndicator);
			return true;
		}
		if (name == PropertyName._disconnectedIndicator)
		{
			value = VariantUtils.CreateFrom(in _disconnectedIndicator);
			return true;
		}
		if (name == PropertyName._nameplateLabel)
		{
			value = VariantUtils.CreateFrom(in _nameplateLabel);
			return true;
		}
		if (name == PropertyName._characterLabel)
		{
			value = VariantUtils.CreateFrom(in _characterLabel);
			return true;
		}
		if (name == PropertyName._platform)
		{
			value = VariantUtils.CreateFrom(in _platform);
			return true;
		}
		if (name == PropertyName._isSingleplayer)
		{
			value = VariantUtils.CreateFrom(in _isSingleplayer);
			return true;
		}
		if (name == PropertyName._playerId)
		{
			value = VariantUtils.CreateFrom(in _playerId);
			return true;
		}
		if (name == PropertyName._isReady)
		{
			value = VariantUtils.CreateFrom(in _isReady);
			return true;
		}
		if (name == PropertyName._isConnected)
		{
			value = VariantUtils.CreateFrom(in _isConnected);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._readyIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._disconnectedIndicator, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._nameplateLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._platform, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSingleplayer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._playerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isReady, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.PlayerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._characterIcon, Variant.From(in _characterIcon));
		info.AddProperty(PropertyName._readyIndicator, Variant.From(in _readyIndicator));
		info.AddProperty(PropertyName._disconnectedIndicator, Variant.From(in _disconnectedIndicator));
		info.AddProperty(PropertyName._nameplateLabel, Variant.From(in _nameplateLabel));
		info.AddProperty(PropertyName._characterLabel, Variant.From(in _characterLabel));
		info.AddProperty(PropertyName._platform, Variant.From(in _platform));
		info.AddProperty(PropertyName._isSingleplayer, Variant.From(in _isSingleplayer));
		info.AddProperty(PropertyName._playerId, Variant.From(in _playerId));
		info.AddProperty(PropertyName._isReady, Variant.From(in _isReady));
		info.AddProperty(PropertyName._isConnected, Variant.From(in _isConnected));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._characterIcon, out var value))
		{
			_characterIcon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._readyIndicator, out var value2))
		{
			_readyIndicator = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._disconnectedIndicator, out var value3))
		{
			_disconnectedIndicator = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._nameplateLabel, out var value4))
		{
			_nameplateLabel = value4.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._characterLabel, out var value5))
		{
			_characterLabel = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._platform, out var value6))
		{
			_platform = value6.As<PlatformType>();
		}
		if (info.TryGetProperty(PropertyName._isSingleplayer, out var value7))
		{
			_isSingleplayer = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._playerId, out var value8))
		{
			_playerId = value8.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._isReady, out var value9))
		{
			_isReady = value9.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isConnected, out var value10))
		{
			_isConnected = value10.As<bool>();
		}
	}
}
