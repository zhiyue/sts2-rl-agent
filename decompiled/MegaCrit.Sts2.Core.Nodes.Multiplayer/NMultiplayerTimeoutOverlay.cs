using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Quality;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

/// <summary>
/// This overlay is displayed over everything on the client when we lose connection with the host for a long period of
/// time.
/// It is in both Run and Game. The Game one is only used when we are in the middle of a transition, or if the run isn't
/// started yet. Usually, we want to use the Run one because it allows usage of menus (it's under the top bar), but
/// sometimes we get locked while we're loading and we still want to display this over everything.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerTimeoutOverlay.cs")]
public class NMultiplayerTimeoutOverlay : Control
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
		/// Cached name for the 'Relocalize' method.
		/// </summary>
		public static readonly StringName Relocalize = "Relocalize";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'IsShown' property.
		/// </summary>
		public static readonly StringName IsShown = "IsShown";

		/// <summary>
		/// Cached name for the '_gameLevel' field.
		/// </summary>
		public static readonly StringName _gameLevel = "_gameLevel";

		/// <summary>
		/// Cached name for the '_icon' field.
		/// </summary>
		public static readonly StringName _icon = "_icon";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const int _noResponseMsec = 3000;

	private const int _loadingNoResponseMsec = 8000;

	private bool _gameLevel;

	private TextureRect _icon;

	private NetClientGameService? _netService;

	public bool IsShown { get; private set; }

	public override void _Ready()
	{
		_icon = GetNode<TextureRect>("%Icon");
	}

	public void Relocalize()
	{
		MegaLabel node = GetNode<MegaLabel>("%Title");
		MegaRichTextLabel node2 = GetNode<MegaRichTextLabel>("%Description");
		node.SetTextAutoSize(new LocString("main_menu_ui", "TIMEOUT_OVERLAY.title").GetFormattedText());
		node2.SetTextAutoSize(new LocString("main_menu_ui", "TIMEOUT_OVERLAY.description").GetFormattedText());
		node.RefreshFont();
		node2.RefreshFont();
	}

	public void Initialize(INetGameService netService, bool isGameLevel)
	{
		if (netService is NetClientGameService netService2)
		{
			_netService = netService2;
			_gameLevel = isGameLevel;
			TaskHelper.RunSafely(UpdateLoop());
		}
	}

	private async Task UpdateLoop()
	{
		while ((_netService?.IsConnected ?? false) && this.IsValid())
		{
			ConnectionStats statsForPeer = _netService.GetStatsForPeer(_netService.HostNetId);
			if (statsForPeer == null)
			{
				return;
			}
			int num = (int)(statsForPeer.LastReceivedTime.HasValue ? (Time.GetTicksMsec() - statsForPeer.LastReceivedTime.Value) : 0);
			bool flag = _gameLevel == (_netService.IsGameLoading || !RunManager.Instance.IsInProgress);
			int num2 = (statsForPeer.RemoteIsLoading ? 8000 : 3000);
			bool flag2 = flag && num >= num2;
			if (!IsShown && flag2)
			{
				base.Visible = true;
			}
			else if (IsShown && !flag2)
			{
				base.Visible = false;
			}
			IsShown = flag2;
			await Task.Delay(200);
		}
		if (this.IsValid())
		{
			base.Visible = false;
		}
		_netService = null;
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
		list.Add(new MethodInfo(MethodName.Relocalize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Relocalize && args.Count == 0)
		{
			Relocalize();
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
		if (method == MethodName.Relocalize)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsShown)
		{
			IsShown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._gameLevel)
		{
			_gameLevel = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.IsShown)
		{
			value = VariantUtils.CreateFrom<bool>(IsShown);
			return true;
		}
		if (name == PropertyName._gameLevel)
		{
			value = VariantUtils.CreateFrom(in _gameLevel);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._gameLevel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsShown, Variant.From<bool>(IsShown));
		info.AddProperty(PropertyName._gameLevel, Variant.From(in _gameLevel));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsShown, out var value))
		{
			IsShown = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._gameLevel, out var value2))
		{
			_gameLevel = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value3))
		{
			_icon = value3.As<TextureRect>();
		}
	}
}
