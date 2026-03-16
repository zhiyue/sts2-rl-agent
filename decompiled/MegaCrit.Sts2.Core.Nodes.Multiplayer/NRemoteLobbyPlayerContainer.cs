using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NRemoteLobbyPlayerContainer.cs")]
public class NRemoteLobbyPlayerContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName RefreshSoloLabelVisibility = "RefreshSoloLabelVisibility";

		public static readonly StringName Cleanup = "Cleanup";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _inviteButton = "_inviteButton";

		public static readonly StringName _soloLabel = "_soloLabel";

		public static readonly StringName _container = "_container";

		public static readonly StringName _displayLocalPlayer = "_displayLocalPlayer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private readonly List<NRemoteLobbyPlayer> _nodes = new List<NRemoteLobbyPlayer>();

	private StartRunLobby? _lobby;

	private NInvitePlayersButton _inviteButton;

	private MegaLabel _soloLabel;

	private Container _container;

	private bool _displayLocalPlayer;

	public override void _Ready()
	{
		_soloLabel = GetNode<MegaLabel>("%SoloLabel");
		_container = GetNode<Container>("Container");
		_inviteButton = GetNode<NInvitePlayersButton>("%InviteButton");
		_soloLabel.SetTextAutoSize(new LocString("main_menu_ui", "MULTIPLAYER_CHAR_SELECT.SOLO").GetFormattedText());
	}

	public void Initialize(StartRunLobby lobby, bool displayLocalPlayer)
	{
		foreach (NRemoteLobbyPlayer node in _nodes)
		{
			node.QueueFreeSafely();
		}
		_nodes.Clear();
		if (!lobby.NetService.Type.IsMultiplayer())
		{
			return;
		}
		_displayLocalPlayer = displayLocalPlayer;
		_inviteButton.Initialize(lobby);
		_lobby = lobby;
		foreach (LobbyPlayer player in _lobby.Players)
		{
			OnPlayerConnected(player);
		}
		RefreshSoloLabelVisibility();
	}

	public void OnPlayerConnected(LobbyPlayer player)
	{
		StartRunLobby lobby = _lobby;
		if (lobby != null && (player.id != lobby.LocalPlayer.id || _displayLocalPlayer))
		{
			NRemoteLobbyPlayer nRemoteLobbyPlayer = NRemoteLobbyPlayer.Create(player, lobby.NetService.Platform, lobby.NetService.Type == NetGameType.Singleplayer);
			_container.AddChildSafely(nRemoteLobbyPlayer);
			_container.MoveChild(_inviteButton.GetParent(), _container.GetChildCount() - 1);
			_nodes.Add(nRemoteLobbyPlayer);
			RefreshSoloLabelVisibility();
		}
	}

	public void OnPlayerDisconnected(LobbyPlayer player)
	{
		if (_lobby == null)
		{
			return;
		}
		int num = _nodes.FindIndex((NRemoteLobbyPlayer p) => p.PlayerId == player.id);
		if (num >= 0)
		{
			_container.RemoveChildSafely(_nodes[num]);
			_nodes.RemoveAt(num);
			foreach (NRemoteLobbyPlayer node in _nodes)
			{
				node.CancelShake();
			}
		}
		RefreshSoloLabelVisibility();
	}

	public void OnPlayerChanged(LobbyPlayer player)
	{
		StartRunLobby lobby = _lobby;
		if (lobby != null && (player.id != lobby.LocalPlayer.id || _displayLocalPlayer))
		{
			_nodes.FirstOrDefault((NRemoteLobbyPlayer p) => p.PlayerId == player.id)?.OnPlayerChanged(player);
		}
	}

	private void RefreshSoloLabelVisibility()
	{
		StartRunLobby lobby = _lobby;
		_soloLabel.Visible = (lobby == null || lobby.NetService.Type != NetGameType.Singleplayer) && lobby != null && lobby.Players.Count == 1;
	}

	public void Cleanup()
	{
		foreach (NRemoteLobbyPlayer node in _nodes)
		{
			node.QueueFreeSafely();
		}
		_nodes.Clear();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshSoloLabelVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Cleanup, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.RefreshSoloLabelVisibility && args.Count == 0)
		{
			RefreshSoloLabelVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Cleanup && args.Count == 0)
		{
			Cleanup();
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
		if (method == MethodName.RefreshSoloLabelVisibility)
		{
			return true;
		}
		if (method == MethodName.Cleanup)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._inviteButton)
		{
			_inviteButton = VariantUtils.ConvertTo<NInvitePlayersButton>(in value);
			return true;
		}
		if (name == PropertyName._soloLabel)
		{
			_soloLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Container>(in value);
			return true;
		}
		if (name == PropertyName._displayLocalPlayer)
		{
			_displayLocalPlayer = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._inviteButton)
		{
			value = VariantUtils.CreateFrom(in _inviteButton);
			return true;
		}
		if (name == PropertyName._soloLabel)
		{
			value = VariantUtils.CreateFrom(in _soloLabel);
			return true;
		}
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		if (name == PropertyName._displayLocalPlayer)
		{
			value = VariantUtils.CreateFrom(in _displayLocalPlayer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inviteButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._soloLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._displayLocalPlayer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._inviteButton, Variant.From(in _inviteButton));
		info.AddProperty(PropertyName._soloLabel, Variant.From(in _soloLabel));
		info.AddProperty(PropertyName._container, Variant.From(in _container));
		info.AddProperty(PropertyName._displayLocalPlayer, Variant.From(in _displayLocalPlayer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._inviteButton, out var value))
		{
			_inviteButton = value.As<NInvitePlayersButton>();
		}
		if (info.TryGetProperty(PropertyName._soloLabel, out var value2))
		{
			_soloLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._container, out var value3))
		{
			_container = value3.As<Container>();
		}
		if (info.TryGetProperty(PropertyName._displayLocalPlayer, out var value4))
		{
			_displayLocalPlayer = value4.As<bool>();
		}
	}
}
