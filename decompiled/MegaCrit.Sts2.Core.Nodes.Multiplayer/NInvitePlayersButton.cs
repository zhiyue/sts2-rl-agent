using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NInvitePlayersButton.cs")]
public class NInvitePlayersButton : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateVisibility = "UpdateVisibility";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName _shaderMaterial = "_shaderMaterial";

		public static readonly StringName _container = "_container";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private ShaderMaterial _shaderMaterial;

	private Control _container;

	private StartRunLobby? _startRunLobby;

	public override void _Ready()
	{
		ConnectSignals();
		_container = GetParent<Control>();
		Control node = GetNode<Control>("Background");
		MegaRichTextLabel node2 = GetNode<MegaRichTextLabel>("Label");
		_shaderMaterial = (ShaderMaterial)node.Material;
		node2.SetTextAutoSize(new LocString("main_menu_ui", "INVITE").GetFormattedText());
		UpdateVisibility();
	}

	public void Initialize(StartRunLobby lobby)
	{
		_startRunLobby = lobby;
		_startRunLobby.PlayerConnected += OnPlayerConnected;
		_startRunLobby.PlayerDisconnected += OnPlayerDisconnected;
		UpdateVisibility();
	}

	public override void _ExitTree()
	{
		if (_startRunLobby != null)
		{
			_startRunLobby.PlayerConnected -= OnPlayerConnected;
			_startRunLobby.PlayerDisconnected -= OnPlayerDisconnected;
		}
	}

	private void OnPlayerConnected(LobbyPlayer player)
	{
		UpdateVisibility();
	}

	private void OnPlayerDisconnected(LobbyPlayer player)
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		_container.Visible = _startRunLobby != null && PlatformUtil.SupportsInviteDialog(_startRunLobby.NetService.Platform) && _startRunLobby.Players.Count < _startRunLobby.MaxPlayers;
	}

	protected override void OnRelease()
	{
		if (_startRunLobby != null)
		{
			PlatformUtil.OpenInviteDialog(_startRunLobby.NetService);
		}
	}

	protected override void OnFocus()
	{
		_shaderMaterial.SetShaderParameter(_s, 1.1f);
		_shaderMaterial.SetShaderParameter(_v, 1.1f);
	}

	protected override void OnUnfocus()
	{
		_shaderMaterial.SetShaderParameter(_s, 1f);
		_shaderMaterial.SetShaderParameter(_v, 1f);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateVisibility && args.Count == 0)
		{
			UpdateVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.UpdateVisibility)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._shaderMaterial)
		{
			_shaderMaterial = VariantUtils.ConvertTo<ShaderMaterial>(in value);
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
		if (name == PropertyName._shaderMaterial)
		{
			value = VariantUtils.CreateFrom(in _shaderMaterial);
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
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shaderMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._container, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._shaderMaterial, Variant.From(in _shaderMaterial));
		info.AddProperty(PropertyName._container, Variant.From(in _container));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._shaderMaterial, out var value))
		{
			_shaderMaterial = value.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._container, out var value2))
		{
			_container = value2.As<Control>();
		}
	}
}
