using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NRemoteLoadLobbyPlayerContainer.cs")]
public class NRemoteLoadLobbyPlayerContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnPlayerConnected = "OnPlayerConnected";

		public static readonly StringName OnPlayerDisconnected = "OnPlayerDisconnected";

		public static readonly StringName OnPlayerChanged = "OnPlayerChanged";

		public static readonly StringName Cleanup = "Cleanup";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName _othersLabel = "_othersLabel";

		public static readonly StringName _container = "_container";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private readonly List<NRemoteLobbyPlayer> _nodes = new List<NRemoteLobbyPlayer>();

	private LoadRunLobby? _lobby;

	private MegaLabel? _othersLabel;

	private Control _container;

	public override void _Ready()
	{
		_othersLabel = GetNodeOrNull<MegaLabel>("OthersLabel");
		_container = GetNode<Control>("Container");
	}

	public void Initialize(LoadRunLobby runLobby, bool displayLocalPlayer)
	{
		_lobby = runLobby;
		if (_othersLabel != null)
		{
			LocString locString = new LocString("main_menu_ui", "MULTIPLAYER_LOAD_MENU.OTHERS");
			locString.Add("others", runLobby.Run.Players.Count - 1);
			_othersLabel.Text = locString.GetFormattedText();
		}
		foreach (SerializablePlayer player in runLobby.Run.Players)
		{
			if (player.NetId != _lobby.NetService.NetId || displayLocalPlayer)
			{
				NRemoteLobbyPlayer nRemoteLobbyPlayer = NRemoteLobbyPlayer.Create(runLobby, player.NetId, runLobby.NetService.Platform, runLobby.NetService.Type == NetGameType.Singleplayer);
				_container.AddChildSafely(nRemoteLobbyPlayer);
				_nodes.Add(nRemoteLobbyPlayer);
			}
		}
	}

	public void OnPlayerConnected(ulong playerId)
	{
		OnPlayerChanged(playerId);
	}

	public void OnPlayerDisconnected(ulong playerId)
	{
		OnPlayerChanged(playerId);
	}

	public void OnPlayerChanged(ulong playerId)
	{
		if (_lobby != null && playerId != _lobby.NetService.NetId)
		{
			int num = _nodes.FindIndex((NRemoteLobbyPlayer p) => p.PlayerId == playerId);
			if (num >= 0)
			{
				NRemoteLobbyPlayer nRemoteLobbyPlayer = _nodes[num];
				nRemoteLobbyPlayer.OnPlayerChanged(_lobby, playerId);
			}
		}
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
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPlayerConnected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPlayerDisconnected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPlayerChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.OnPlayerConnected && args.Count == 1)
		{
			OnPlayerConnected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerDisconnected && args.Count == 1)
		{
			OnPlayerDisconnected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerChanged && args.Count == 1)
		{
			OnPlayerChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
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
		if (method == MethodName.OnPlayerConnected)
		{
			return true;
		}
		if (method == MethodName.OnPlayerDisconnected)
		{
			return true;
		}
		if (method == MethodName.OnPlayerChanged)
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
		if (name == PropertyName._othersLabel)
		{
			_othersLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._othersLabel)
		{
			value = VariantUtils.CreateFrom(in _othersLabel);
			return true;
		}
		if (name == PropertyName._container)
		{
			value = VariantUtils.CreateFrom(in _container);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._othersLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._othersLabel, Variant.From(in _othersLabel));
		info.AddProperty(PropertyName._container, Variant.From(in _container));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._othersLabel, out var value))
		{
			_othersLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._container, out var value2))
		{
			_container = value2.As<Control>();
		}
	}
}
